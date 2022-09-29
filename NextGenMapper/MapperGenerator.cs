using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Validators;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
            //#endif 
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapperExtensions.g", ExtensionsSource.Source);
                i.AddSource("StartMapper.g", StartMapperSource.StartMapper);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var mapPlanner = new MapPlanner();
            var diagnosticReporter = new DiagnosticReporter();

            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            foreach(var userMapMethod in receiver.UserMapMethods)
            {
                if (userMapMethod.SemanticModel.GetDeclaredSymbol(userMapMethod.Node) is IMethodSymbol method
                    && !method.IsAsync
                    && method.IsExtensionMethod
                    && method.IsGenericMethod
                    && method.MethodKind == MethodKind.Ordinary
                    && !method.ReturnsByRef
                    && !method.ReturnsByRefReadonly
                    && !method.ReturnsVoid
                    && method.IsDefinition
                    && method.IsStatic
                    && method.Name == "Map"
                    && method.Parameters.Length == 1
                    && method.ReturnType is not ITypeParameterSymbol)
                {
                    mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                }
            }

            foreach (var mapMethodInvocation in receiver.MapMethodInvocations)
            {
                if (mapMethodInvocation.SemanticModel.GetSymbolInfo(mapMethodInvocation.Node.Expression).Symbol is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.MapFunctionFullName
                    && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapMethodInvocation.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol switch
                    {
                        ILocalSymbol invocatedVariable => invocatedVariable.Type,
                        IParameterSymbol invocatedParameter => invocatedParameter.Type,
                        IMethodSymbol { MethodKind: MethodKind.Constructor } invocatedConstructor => invocatedConstructor.ContainingType,
                        IMethodSymbol invocatedMethod => invocatedMethod.ReturnType,
                        IPropertySymbol invocatedProperty => invocatedProperty.Type,
                        IFieldSymbol invocatedField => invocatedField.Type, 
                        _ => null
                    } is ITypeSymbol fromType
                    && !mapPlanner.IsTypesMapAlreadyPlanned(fromType, method.ReturnType))
                {
                    var designer = new TypeMapDesigner(diagnosticReporter);
                    var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType, memberAccess.GetLocation());
                    foreach (var map in maps)
                    {
                        mapPlanner.AddMap(map);
                    }
                }
            }

            foreach (var mapWithMethodInvocation in receiver.MapWithMethodInvocations)
            {
                var invocationMethodSymbolInfo = mapWithMethodInvocation.SemanticModel.GetSymbolInfo(mapWithMethodInvocation.Node.Expression);
                var invocationMethodSymbol = invocationMethodSymbolInfo.Symbol;
                var isStubMethod = true;
                if (invocationMethodSymbol is null
                    && invocationMethodSymbolInfo.CandidateSymbols.Length == 1
                    && invocationMethodSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure
                    && mapWithMethodInvocation.Arguments.Length > 0)
                {
                    invocationMethodSymbol = invocationMethodSymbolInfo.CandidateSymbols[0];
                    isStubMethod = false;
                }

                if (invocationMethodSymbol is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.MapWithFunctionFullName
                    && mapWithMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapWithMethodInvocation.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol switch
                    {
                        ILocalSymbol invocatedVariable => invocatedVariable.Type,
                        IParameterSymbol invocatedParameter => invocatedParameter.Type,
                        IMethodSymbol { MethodKind: MethodKind.Constructor } invocatedConstructor => invocatedConstructor.ContainingType,
                        IMethodSymbol invocatedMethod => invocatedMethod.ReturnType,
                        IPropertySymbol invocatedProperty => invocatedProperty.Type,
                        IFieldSymbol invocatedField => invocatedField.Type,
                        _ => null
                    } is ITypeSymbol fromType)
                {
                    if (fromType.TypeKind == TypeKind.Enum && method.ReturnType.TypeKind == TypeKind.Enum)
                    {
                        diagnosticReporter.ReportMapWithNotSupportedForEnums(memberAccess.GetLocation());
                        continue;
                    }

                    if (isStubMethod)
                    {
                        diagnosticReporter.ReportMapWithMethodWithoutArgumentsError(memberAccess.GetLocation());
                        continue;
                    }

                    var designer = new TypeMapWithDesigner(diagnosticReporter);
                    var publicProperties = method.ReturnType.GetPublicProperties().ToArray();
                    var arguments = mapWithMethodInvocation.Arguments.Select(x =>
                    {
                        var propertyAsParamter = publicProperties
                            .FirstOrDefault(y => y.Name.Equals(x.NameColon?.Name.Identifier.Text, StringComparison.InvariantCultureIgnoreCase))
                            ?? publicProperties[Array.IndexOf(mapWithMethodInvocation.Arguments, x)];

                        return new MapWithInvocationAgrument(propertyAsParamter.Name.ToCamelCase(), propertyAsParamter.Type);
                    }).ToArray();

                    if (arguments.Length > 0 && arguments.Length == publicProperties.Length)
                    {
                        diagnosticReporter.ReportToManyArgumentsForMapWithError(memberAccess.GetLocation());
                        continue;
                    }

                    if (mapPlanner.IsTypesMapWithAlreadyPlanned(fromType, method.ReturnType, arguments))
                    {
                        diagnosticReporter.ReportMapWithBetterFunctionMemberNotFound(memberAccess.GetLocation(), fromType, method.ReturnType);
                    }

                    var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType, arguments, memberAccess.GetLocation());
                    foreach (var map in maps)
                    {
                        if (map is ClassMapWith classMapWith)
                        {
                            if (!mapPlanner.IsTypesMapWithStubAlreadyPlanned(classMapWith.From, classMapWith.To))
                            {
                                classMapWith.NeedGenerateStubMethod = true;
                            }
                            mapPlanner.AddMapWith(classMapWith);
                        }
                        else
                        {
                            mapPlanner.AddMap(map);
                        }
                    }
                }
            }

            foreach (var map in mapPlanner.Maps)
            {
                if (map is CollectionMap collectionMap
                        && !collectionMap.ItemFrom.Equals(collectionMap.ItemTo, SymbolEqualityComparer.IncludeNullability)
                        && !mapPlanner.IsTypesMapAlreadyPlanned(collectionMap.ItemFrom, collectionMap.ItemTo))
                {
                    diagnosticReporter.ReportMappingFunctionNotFound(collectionMap.MapLocation, collectionMap.ItemFrom, collectionMap.ItemTo);
                }

                if (map is ClassMap classMap)
                {
                    if (classMap is ClassMapWith classMapWith)
                    {
                        if (!mapPlanner.IsTypesMapWithAlreadyPlanned(classMapWith.From, classMapWith.To, classMapWith.Arguments))
                        {
                            diagnosticReporter.ReportMappingFunctionNotFound(classMapWith.MapLocation, classMapWith.From, classMapWith.To);
                        }
                    }
                    else
                    {
                        if (!mapPlanner.IsTypesMapAlreadyPlanned(classMap.From, classMap.To))
                        {
                            diagnosticReporter.ReportMappingFunctionNotFound(classMap.MapLocation, classMap.From, classMap.To);
                        }
                    }

                    foreach (var constructorProperty in classMap.ConstructorProperties)
                    {
                        if (!constructorProperty.IsSameTypes
                            && !mapPlanner.IsTypesMapAlreadyPlanned(constructorProperty.FromType, constructorProperty.ToType)
                            && !ImplicitNumericConversionValidator.HasImplicitConversion(constructorProperty.FromType, constructorProperty.ToType))
                        {
                            diagnosticReporter.ReportMappingFunctionForPropertyNotFound(
                                classMap.MapLocation, classMap.From, constructorProperty.FromName, constructorProperty.FromType, classMap.To, constructorProperty.ToName, constructorProperty.ToType);
                        }
                    }

                    foreach (var initializerProperty in classMap.InitializerProperties)
                    {
                        if (!initializerProperty.IsSameTypes
                            && !mapPlanner.IsTypesMapAlreadyPlanned(initializerProperty.FromType, initializerProperty.ToType)
                            && !ImplicitNumericConversionValidator.HasImplicitConversion(initializerProperty.FromType, initializerProperty.ToType))
                        {
                            diagnosticReporter.ReportMappingFunctionForPropertyNotFound(
                                classMap.MapLocation, classMap.From, initializerProperty.FromName, initializerProperty.FromType, classMap.To, initializerProperty.ToName, initializerProperty.ToType);
                        }
                    }
                }
            }
            var mapperClassBuilder = new MapperClassBuilder();
            var mapperSourceCode = mapperClassBuilder.Generate(mapPlanner.Maps);
            context.AddSource($"Mapper.g", mapperSourceCode);

            diagnosticReporter.GetDiagnostics().ForEach(x => context.ReportDiagnostic(x));
        }
    }
}
