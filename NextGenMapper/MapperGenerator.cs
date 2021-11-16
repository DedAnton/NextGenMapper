using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private readonly MapPlanner _mapPlanner;
        private readonly DiagnosticReporter _diagnosticReporter;

        public MapperGenerator()
        {
            _mapPlanner = new();
            _diagnosticReporter = new();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapperExtensions", ExtensionsSource.Source);
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("PartialAttribute", Annotations.PartialAttributeText);
                i.AddSource("StartMapper", StartMapperSource.StartMapper);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            foreach (var mapperClassDeclaration in receiver.MapperClassDeclarations)
            {
                if (mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(mapperClassDeclaration.Node).HasAttribute(Annotations.MapperAttributeFullName))
                {
                    var usings = mapperClassDeclaration.Node.GetUsingsAndNamespace();
                    var maps = HandleCustomMapperClass(mapperClassDeclaration.SemanticModel, mapperClassDeclaration.Node);
                    foreach(var map in maps)
                    {
                        AddMapToPlanner(map, usings);
                    }
                }
            }

            foreach (var mapMethodInvocation in receiver.MapMethodInvocations)
            {
                if (mapMethodInvocation.SemanticModel.GetSymbol(mapMethodInvocation.Node.Expression) is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.FunctionFullName
                    && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapMethodInvocation.SemanticModel.GetSymbol(memberAccess.Expression) is ILocalSymbol invocatingVariable
                    && !_mapPlanner.IsTypesMapAlreadyPlanned(invocatingVariable.Type, method.ReturnType))
                {
                    var maps = MapInvocation(invocatingVariable.Type, method.ReturnType);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(map, new());
                    }
                } 
            }

            var commonMappers = GenerateCommonMapper();
            commonMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CommonMapper", SourceText.From(mapper, Encoding.UTF8)));

            var customMappers = GenerateCustomMappers();
            customMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CustomMapper", SourceText.From(mapper, Encoding.UTF8)));

            _diagnosticReporter.GetDiagnostics().ForEach(x => context.ReportDiagnostic(x));
        }

        private List<TypeMap> MapInvocation(ITypeSymbol from, ITypeSymbol to)
        {
            var maps = new List<TypeMap>();
            if (from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum)
            {
                var designer = new EnumMapDesigner(_diagnosticReporter);
                maps.Add(designer.DesignMapsForPlanner(from, to));
            }
            else if (from.IsGenericEnumerable() && to.IsGenericEnumerable())
            {
                var designer = new CollectionMapDesigner(_diagnosticReporter);
                maps.AddRange(designer.DesignMapsForPlanner(from, to));
            }
            else if (from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class)
            {
                var designer = new ClassMapDesigner(_diagnosticReporter);
                maps.AddRange(designer.DesignMapsForPlanner(from, to));
            }

            return maps;
        }

        private List<TypeMap> HandleCustomMapperClass(SemanticModel semanticModel, ClassDeclarationSyntax node)
        {
            var maps = new List<TypeMap>();
            foreach (var method in node.GetMethodsDeclarations())
            {
                if (semanticModel.GetDeclaredSymbol(method) is not IMethodSymbol userMethod)
                {
                    continue;
                }
                if (userMethod.Parameters.Count() != 1)
                {
                    _diagnosticReporter.ReportParameterNotFoundError(method.GetLocation());
                    continue;
                }
                if (userMethod.ReturnsVoid)
                {
                    _diagnosticReporter.ReportReturnTypeNotFoundError(method.GetLocation());
                    continue;
                }
                var from = userMethod.Parameters.Single().Type;
                var to = userMethod.ReturnType;

                if (userMethod.Parameters.Length == 1
                    && userMethod.HasAttribute(Annotations.PartialAttributeName)
                    && method.GetObjectCreateionExpression() is { } objCreationExpression
                    && semanticModel.GetSymbol(objCreationExpression) is IMethodSymbol constructor)
                {
                    if (objCreationExpression.ArgumentList?.Arguments.Any(x => x.IsDefaultLiteralExpression()) == true)
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

            return maps;
        }

        private void AddMapToPlanner(TypeMap map, List<string> usings)
        {
            if (map is ClassPartialConstructorMap or ClassPartialMap or TypeCustomMap)
            {
                _mapPlanner.AddCustomMap(map, usings);
            }
            else
            {
                _mapPlanner.AddCommonMap(map);
            }
        }

        private List<string> GenerateCommonMapper()
        {
            var commonMapperGenerator = new CommonMapperGenerator();
            var commonMapGroups = _mapPlanner.MapGroups.Where(x => x.Priority == MapPriority.Common);
            var commonMappers = commonMapGroups.Select(x => commonMapperGenerator.Generate(x));

            return commonMappers.ToList();
        }

        private List<string> GenerateCustomMappers()
        {
            var customMapperGenerator = new CustomMapperGenerator();
            var customMapGroups = _mapPlanner.MapGroups.Where(x => x.Priority == MapPriority.Custom);
            var customMappers = customMapGroups.Select(x => customMapperGenerator.Generate(x));

            return customMappers.ToList();
        }
    }
}
