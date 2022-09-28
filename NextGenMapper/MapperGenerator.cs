using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System;
using System.Collections.Generic;
using System.Linq;

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
                i.AddSource("MapperExtensions", ExtensionsSource.Source);
                i.AddSource("StartMapper", StartMapperSource.StartMapper);
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
                    var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(mapPlanner, map, new());
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
                    } is ITypeSymbol fromType
                    && !mapPlanner.IsTypesMapAlreadyPlanned(fromType, method.ReturnType)
                    && fromType.TypeKind == TypeKind.Class && method.ReturnType.TypeKind == TypeKind.Class)
                {
                    if (isStubMethod)
                    {
                        diagnosticReporter.ReportMapWithMethodWithoutArgumentsError(memberAccess.GetLocation());
                    }

                    var designer = new TypeMapWithDesigner(diagnosticReporter);
                    var publicProperties = method.ReturnType.GetPublicProperties().ToArray();
                    var arguments = mapWithMethodInvocation.Arguments.Select(x =>
                    {
                        var propertyAsParamter = publicProperties
                            .FirstOrDefault(y => y.Name.Equals(x.NameColon?.Name.Identifier.Text, StringComparison.InvariantCultureIgnoreCase))
                            ?? publicProperties[Array.IndexOf(mapWithMethodInvocation.Arguments, x)];

                        return new MapWithInvocationAgrument(propertyAsParamter.Name.ToCamelCase(), propertyAsParamter.Type);
                    }).ToList();

                    if (arguments.Count == publicProperties.Length)
                    {
                        //TODO: create only stub method if this happened
                        diagnosticReporter.ReportToManyArgumentsForMapWithError(memberAccess.GetLocation());
                    }

                    var maps = designer.DesignMapsForPlanner(fromType, method.ReturnType, arguments);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(mapPlanner, map, new());
                    }
                }
            }

            var prefix = 1;
            var mapperClassBuilder = new MapperClassBuilder();
            foreach (var mapGroup in mapPlanner.MapGroups)
            {
                var mapperSourceCode = mapperClassBuilder.Generate(mapGroup);
                context.AddSource($"{prefix}_Mapper", mapperSourceCode);
                prefix++;
            }

            diagnosticReporter.GetDiagnostics().ForEach(x => context.ReportDiagnostic(x));
        }

        private void AddMapToPlanner(MapPlanner planner, TypeMap map, HashSet<string> usings)
        {
            if (map is ClassMapWith)
            {
                planner.AddCustomMap(map, usings);
            }
            else
            {
                planner.AddCommonMap(map);
            }
        }
    }
}
