using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private MapPlanner _mapPlanner;
        private DiagnosticReporter _diagnosticReporter;

        public MapperGenerator()
        {
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!System.Diagnostics.Debugger.IsAttached)
//            {
//                System.Diagnostics.Debugger.Launch();
//            }
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
            _mapPlanner = new();
            _diagnosticReporter = new();

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
                    && mapMethodInvocation.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is ILocalSymbol invocatingVariable
                    && !_mapPlanner.IsTypesMapAlreadyPlanned(invocatingVariable.Type, method.ReturnType))
                {
                    var designer = new TypeMapDesigner(_diagnosticReporter);
                    var maps = designer.DesignMapsForPlanner(invocatingVariable.Type, method.ReturnType);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(map, new());
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
                    && mapWithMethodInvocation.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is ILocalSymbol invocatingVariable
                    && !_mapPlanner.IsTypesMapAlreadyPlanned(invocatingVariable.Type, method.ReturnType)
                    && invocatingVariable.Type.TypeKind == TypeKind.Class && method.ReturnType.TypeKind == TypeKind.Class)
                {
                    if (isStubMethod)
                    {
                        _diagnosticReporter.ReportMapWithMethodWithoutArgumentsError(memberAccess.GetLocation());
                    }

                    var designer = new TypeMapWithDesigner(_diagnosticReporter);
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
                        _diagnosticReporter.ReportToManyArgumentsForMapWithError(memberAccess.GetLocation());
                    }

                    var maps = designer.DesignMapsForPlanner(invocatingVariable.Type, method.ReturnType, arguments);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(map, new());
                    }
                }
            }

            var prefix = 1;
            var mapperClassBuilder = new MapperClassBuilder();
            foreach (var mapGroup in _mapPlanner.MapGroups)
            {
                var mapperSourceCode = mapperClassBuilder.Generate(mapGroup);
                context.AddSource($"{prefix}_Mapper", mapperSourceCode);
                prefix++;
            }

            _diagnosticReporter.GetDiagnostics().ForEach(x => context.ReportDiagnostic(x));
        }

        private void AddMapToPlanner(TypeMap map, HashSet<string> usings)
        {
            if (map is ClassMapWith)
            {
                _mapPlanner.AddCustomMap(map, usings);
            }
            else
            {
                _mapPlanner.AddCommonMap(map);
            }
        }
    }
}
