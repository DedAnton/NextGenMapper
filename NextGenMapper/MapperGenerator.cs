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

            var planner = new MapPlanner();
            foreach (var mapperClassDeclaration in receiver.mapperClassDeclarations)
            {
                if (mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(mapperClassDeclaration.Node).HasAttribute(Annotations.MapperAttributeFullName))
                {
                    var usings = mapperClassDeclaration.Node.GetUsingsAndNamespace();
                    var maps = HandleCustomMapperClass(mapperClassDeclaration.SemanticModel, mapperClassDeclaration.Node);
                    foreach(var map in maps)
                    {
                        AddMapToPlanner(map, planner, usings);
                    }
                }
            }

            foreach (var mapMethodInvocation in receiver.mapMethodInvocations)
            {
                if (mapMethodInvocation.SemanticModel.GetSymbol(mapMethodInvocation.Node.Expression) is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.FunctionFullName
                    && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapMethodInvocation.SemanticModel.GetSymbol(memberAccess.Expression) is ILocalSymbol invocatingVariable
                    && !planner.IsTypesMapAlreadyPlanned(invocatingVariable.Type, method.ReturnType))
                {
                    var maps = MapInvocation(invocatingVariable.Type, method.ReturnType);
                    foreach (var map in maps)
                    {
                        AddMapToPlanner(map, planner, new());
                    }
                } 
            }

            var commonMappers = GenerateCommonMapper(planner);
            commonMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CommonMapper", SourceText.From(mapper, Encoding.UTF8)));

            var customMappers = GenerateCustomMappers(planner);
            customMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CustomMapper", SourceText.From(mapper, Encoding.UTF8)));
        }

        private List<TypeMap> MapInvocation(ITypeSymbol from, ITypeSymbol to)
        {
            var maps = new List<TypeMap>();
            if (from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum)
            {
                var designer = new EnumMapDesigner();
                maps.Add(designer.DesignMapsForPlanner(from, to));
            }
            else if (from.IsGenericEnumerable() && to.IsGenericEnumerable())
            {
                var designer = new CollectionMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(from, to));
            }
            else if (from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class)
            {
                var designer = new ClassMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(from, to));
            }

            return maps;
        }

        private List<TypeMap> HandleCustomMapperClass(SemanticModel semanticModel, ClassDeclarationSyntax node)
        {
            var maps = new List<TypeMap>();
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasSingleParameterWithType()))
            {
                if (semanticModel.GetDeclaredSymbol(method) is not IMethodSymbol userMethod)
                {
                    continue;
                }

                var(to, from) = semanticModel.GetReturnAndParameterType(method);
                if (userMethod.Parameters.Length == 1
                    && userMethod.HasAttribute(Annotations.PartialAttributeName)
                    && method.GetObjectCreateionExpression() is { } objCreationExpression
                    && semanticModel.GetSymbol(objCreationExpression) is IMethodSymbol constructor)
                {
                    if (objCreationExpression.ArgumentList?.Arguments.Any(x => x.IsDefaultLiteralExpression()) == true)
                    {
                        var designer = new ClassPartialConstructorMapDesigner();
                        maps.AddRange(designer.DesignMapsForPlanner(from, to, constructor, method));
                    }
                    else
                    {
                        var designer = new ClassPartialMapDesigner();
                        maps.AddRange(designer.DesignMapsForPlanner(from, to, constructor, method));
                    }
                }
                else
                {
                    var designer = new TypeCustomMapDesigner(semanticModel);
                    maps.Add(designer.DesignMapsForPlanner(method));
                }

            }

            return maps;
        }

        private void AddMapToPlanner(TypeMap map, MapPlanner planner, List<string> usings)
        {
            if (map is ClassPartialConstructorMap or ClassPartialMap or TypeCustomMap)
            {
                planner.AddCustomMap(map, usings);
            }
            else
            {
                planner.AddCommonMap(map);
            }
        }

        private List<string> GenerateCommonMapper(MapPlanner planner)
        {
            var commonMapperGenerator = new CommonMapperGenerator();
            var commonMapGroups = planner.MapGroups.Where(x => x.Priority == MapPriority.Common);
            var commonMappers = commonMapGroups.Select(x => commonMapperGenerator.Generate(x));

            return commonMappers.ToList();
        }

        private List<string> GenerateCustomMappers(MapPlanner planner)
        {
            var customMapperGenerator = new CustomMapperGenerator();
            var customMapGroups = planner.MapGroups.Where(x => x.Priority == MapPriority.Custom);
            var customMappers = customMapGroups.Select(x => customMapperGenerator.Generate(x));

            return customMappers.ToList();
        }
    }
}
