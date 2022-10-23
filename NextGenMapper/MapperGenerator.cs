using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Validators;
using NextGenMapper.CodeGeneration;
using NextGenMapper.PostInitialization;
using System;
using System.Collections.Generic;

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

            foreach (var userMapMethod in receiver.UserMapMethods)
            {
                if (userMapMethod.SemanticModel.GetDeclaredSymbol(userMapMethod.Node) is IMethodSymbol method
                    && !method.IsAsync
                    && method.MethodKind == MethodKind.Ordinary
                    && method.IsDefinition
                    && method.IsStatic
                    && method.Name == "Map"
                    && method.Parameters.Length == 1
                    && method.ReturnType is not ITypeParameterSymbol)
                {
                    if (!method.IsExtensionMethod)
                    {
                        //TODO: Research for OutOfRangeException
                        diagnosticReporter.ReportMapMethodMustBeExtension(method.Locations[0]);
                        mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                        continue;
                    }

                    if (!method.IsGenericMethod || method.TypeParameters.Length != 1)
                    {
                        //TODO: Research for OutOfRangeException
                        diagnosticReporter.ReportMapMethodMustBeGeneric(method.Locations[0]);
                        mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                        continue;
                    }

                    if (method.ReturnsVoid)
                    {
                        //TODO: Research for OutOfRangeException
                        diagnosticReporter.ReportMapMethodMustNotReturnVoid(method.Locations[0]);
                        mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                        continue;
                    }

                    if (method.DeclaredAccessibility != Accessibility.Internal)
                    {
                        //TODO: Research for OutOfRangeException
                        diagnosticReporter.ReportMapMethodMustBeInternal(method.Locations[0]);
                        mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                        continue;
                    }

                    mapPlanner.AddUserDefinedMap(method.Parameters[0].Type, method.ReturnType);
                }
            }

            foreach (var mapMethodInvocation in receiver.MapMethodInvocations)
            {
                if (mapMethodInvocation.SemanticModel.GetSymbolInfo(mapMethodInvocation.Node.Expression).Symbol is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.MapFunctionFullName
                    && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapMethodInvocation.SemanticModel.GetTypeInfo(memberAccess.Expression).Type is ITypeSymbol fromType
                    && !mapPlanner.IsTypesMapAlreadyPlanned(fromType, method.ReturnType))
                {
                    var mapInvocationLocation = memberAccess.GetLocation();

                    if (fromType.TypeKind != method.ReturnType.TypeKind
                        && !MapDesignersHelper.IsCollectionMapping(fromType, method.ReturnType))
                    {
                        diagnosticReporter.ReportTypesKindsMismatch(mapInvocationLocation, fromType, method.ReturnType);
                        continue;
                    }

                    if (fromType.TypeKind == TypeKind.Struct || method.ReturnType.TypeKind == TypeKind.Struct)
                    {
                        diagnosticReporter.ReportStructNotSupported(mapInvocationLocation);
                        continue;
                    }

                    if (SymbolEqualityComparer.IncludeNullability.Equals(fromType, method.ReturnType))
                    {
                        diagnosticReporter.ReportMappedTypesEquals(mapInvocationLocation);
                        continue;
                    }

                    var designer = new TypeMapDesigner(diagnosticReporter, mapPlanner);
                    var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType, mapInvocationLocation);
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
                    && mapWithMethodInvocation.SemanticModel.GetTypeInfo(memberAccess.Expression).Type is ITypeSymbol fromType)
                {
                    var mapInvocationLocation = memberAccess.GetLocation();

                    if (!MapDesignersHelper.IsClassMapping(fromType, method.ReturnType))
                    {
                        diagnosticReporter.ReportNotSupportetForMapWith(mapInvocationLocation, fromType, method.ReturnType);
                        continue;
                    }

                    if (SymbolEqualityComparer.IncludeNullability.Equals(fromType, method.ReturnType))
                    {
                        diagnosticReporter.ReportMappedTypesEquals(mapInvocationLocation);
                        continue;
                    }

                    var argumentsNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                    foreach (var argument in mapWithMethodInvocation.Arguments)
                    {
                        var argumentName = argument.NameColon?.Name.Identifier.ValueText;
                        if (argumentName is not null)
                        {
                            argumentsNames.Add(argumentName);
                        }
                        else
                        {
                            diagnosticReporter.ReportMapWithArgumentMustBeNamed(mapInvocationLocation);
                        }
                    }

                    var designer = new TypeMapWithDesigner(diagnosticReporter, mapPlanner);

                    var mapWithStubs = designer.DesignStubMethodMap(fromType, method.ReturnType, mapInvocationLocation);
                    if (isStubMethod)
                    {
                        diagnosticReporter.ReportMapWithMethodWithoutArgumentsError(mapInvocationLocation);
                        foreach (var mapWithStub in mapWithStubs)
                        {
                            if (!mapPlanner.IsTypesMapWithStubAlreadyPlanned(mapWithStub.From, mapWithStub.To, mapWithStub.Parameters))
                            {
                                var argumentsFromParameters = new MapWithInvocationAgrument[mapWithStub.Parameters.Length];
                                for (var i = 0; i < mapWithStub.Parameters.Length; i++)
                                {
                                    argumentsFromParameters[i] = new MapWithInvocationAgrument(mapWithStub.Parameters[i].Name, mapWithStub.Parameters[i].Type);
                                }
                                if (!mapPlanner.IsTypesMapWithAlreadyPlanned(mapWithStub.From, mapWithStub.To, argumentsFromParameters))
                                {
                                    mapPlanner.AddMapWithStub(mapWithStub);
                                }
                            }
                        }
                        continue;
                    }
                    else
                    {
                        var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType, argumentsNames, mapInvocationLocation);
                        foreach (var map in maps)
                        {
                            if (map is ClassMapWith classMapWith)
                            {
                                if (mapPlanner.IsTypesMapWithAlreadyPlanned(classMapWith.From, classMapWith.To, classMapWith.Arguments))
                                {
                                    diagnosticReporter.ReportDuplicateMapWithFunction(mapInvocationLocation, fromType, method.ReturnType);
                                }
                                mapPlanner.AddMapWith(classMapWith);

                                foreach (var mapWithStub in mapWithStubs)
                                {
                                    if (!mapPlanner.IsTypesMapWithStubAlreadyPlanned(mapWithStub.From, mapWithStub.To, mapWithStub.Parameters))
                                    {
                                        var argumentsFromParameters = new MapWithInvocationAgrument[mapWithStub.Parameters.Length];
                                        for (var i = 0; i < mapWithStub.Parameters.Length; i++)
                                        {
                                            argumentsFromParameters[i] = new MapWithInvocationAgrument(mapWithStub.Parameters[i].Name, mapWithStub.Parameters[i].Type);
                                        }
                                        if (!mapPlanner.IsTypesMapWithAlreadyPlanned(mapWithStub.From, mapWithStub.To, argumentsFromParameters))
                                        {
                                            var isParametersEqualArguments = classMapWith.Arguments.Length == mapWithStub.Parameters.Length;
                                            for (var i = 0; i < classMapWith.Arguments.Length; i++)
                                            {
                                                isParametersEqualArguments = isParametersEqualArguments
                                                    && SymbolEqualityComparer.IncludeNullability.Equals(classMapWith.Arguments[i].Type, mapWithStub.Parameters[i].Type);
                                            }
                                            if (!isParametersEqualArguments)
                                            {
                                                mapPlanner.AddMapWithStub(mapWithStub);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                mapPlanner.AddMap(map);
                            }
                        }
                    }
                }
            }

            foreach (var map in mapPlanner.Maps)
            {
                if (map is CollectionMap collectionMap
                    && !collectionMap.ItemFrom.Equals(collectionMap.ItemTo, SymbolEqualityComparer.IncludeNullability)
                    && !mapPlanner.IsTypesMapAlreadyPlanned(collectionMap.ItemFrom, collectionMap.ItemTo)
                    && !ImplicitNumericConversionValidator.HasImplicitConversion(collectionMap.ItemFrom, collectionMap.ItemTo))
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
