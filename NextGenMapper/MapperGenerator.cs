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

            //var sw1 = new OneOffStopwatch();
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapperExtensions", ExtensionsSource.Source);
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("PartialAttribute", Annotations.PartialAttributeText);
                i.AddSource("StartMapper", StartMapperSource.StartMapper);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            //Console.WriteLine($"initialize: {sw1.Stop()}");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            _mapPlanner = new();
            _diagnosticReporter = new();

            //var sw2 = new OneOffStopwatch();
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            foreach (var mapperClassDeclaration in receiver.MapperClassDeclarations)
            {
                if (mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(mapperClassDeclaration.Node).HasAttribute(Annotations.MapperAttributeFullName))
                {
                    var usings = mapperClassDeclaration.Node.GetUsings();
                    usings.Add($"using {mapperClassDeclaration.Node.GetNamespace()};");

                    var maps = HandleCustomMapperClass(mapperClassDeclaration.SemanticModel, mapperClassDeclaration.Node);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(map, usings);
                    }
                }
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

            //Console.WriteLine($"prepare maps for mappers: {sw2.Stop()}");

            //var sw3 = new OneOffStopwatch();

            var prefix = 1;
            var mapperClassBuilder = new MapperClassBuilder();
            foreach (var mapGroup in _mapPlanner.MapGroups)
            {
                var mapperSourceCode = mapperClassBuilder.Generate(mapGroup);
                context.AddSource($"{prefix}_Mapper", mapperSourceCode);
                prefix++;
            }
            //Console.WriteLine($"generate mappers: {sw3.Stop()}");

            _diagnosticReporter.GetDiagnostics().ForEach(x => context.ReportDiagnostic(x));

        }

        private List<TypeMap> HandleCustomMapperClass(SemanticModel semanticModel, ClassDeclarationSyntax node)
        {
            var maps = new List<TypeMap>();
            foreach (var member in node.Members)
            {
                if (member is MethodDeclarationSyntax method)
                {
                    if (semanticModel.GetDeclaredSymbol(method) is not IMethodSymbol userMethod)
                    {
                        continue;
                    }
                    if (userMethod.Parameters.Length != 1)
                    {
                        _diagnosticReporter.ReportParameterNotFoundError(method.GetLocation());
                        continue;
                    }
                    if (userMethod.ReturnsVoid)
                    {
                        _diagnosticReporter.ReportReturnTypeNotFoundError(method.GetLocation());
                        continue;
                    }
                    var from = userMethod.Parameters[0].Type;
                    var to = userMethod.ReturnType;

                    if (userMethod.Parameters.Length == 1
                        && userMethod.HasAttribute(Annotations.PartialAttributeName)
                        && method.GetObjectCreationExpression() is { } objCreationExpression
                        && semanticModel.GetSymbolInfo(objCreationExpression).Symbol is IMethodSymbol constructor)
                    {
                        var isPartialConstructorMap = false;
                        if (objCreationExpression.ArgumentList != null)
                        {
                            foreach (var argument in objCreationExpression.ArgumentList.Arguments)
                            {
                                if (argument.IsDefaultLiteralExpression())
                                {
                                    isPartialConstructorMap = true;
                                }
                            }
                        }

                        if (isPartialConstructorMap)
                        {
                            var designer = new ClassPartialConstructorMapDesigner(_diagnosticReporter);
                            maps.AddRange(designer.DesignMapsForPlanner(from, to, constructor, method));
                        }
                        else
                        {
                            var designer = new ClassPartialMapDesigner(_diagnosticReporter);
                            maps.AddRange(designer.DesignMapsForPlanner(from, to, constructor, method));
                        }
                    }
                    else
                    {
                        var designer = new TypeCustomMapDesigner();
                        maps.Add(designer.DesignMapsForPlanner(from, to, method));
                    }
                }
            }

            return maps;
        }

        private void AddMapToPlanner(TypeMap map, HashSet<string> usings)
        {
            if (map is ClassPartialConstructorMap or ClassPartialMap or TypeCustomMap or ClassMapWith)
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
