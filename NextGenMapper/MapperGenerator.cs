using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var analyzer = new SyntaxAnalyzer();

            foreach(var mapMethodInvocation in receiver.MapMethodInvocations)
            {
                var result = analyzer.AnalyzeMapMethodInvocation(mapMethodInvocation);
                ImmutableArray<TypeMap> maps = (result?.from, result?.to) switch
                {
                    (Enum from, Enum to) => new EnumMapDesigner().DesignEnumMaps(from, to),
                    (Collection from, Collection to) => new CollectionMapDesigner().DesignCollectionMaps(from, to),
                    (Type from, Type to) => new ClassMapDesigner().DesignClassMaps(from, to),
                    _ => ImmutableArray.Create<TypeMap>()
                };
                foreach(var map in maps)
                {
                    planner.AddCommonMap(map);
                }
            }

            foreach (var mapperClassDeclaration in receiver.MapperClassDeclarations)
            {
                var mapMethod = analyzer.AnalyzeMapperClassDeclarations(mapperClassDeclaration);
                ImmutableArray<TypeMap> maps = mapMethod switch
                {
                    CustomMapMethod customMapMethod => new TypeCustomMapDesigner().DesignTypeCustomMaps(customMapMethod),
                    PartialMapMethod partialMapMethod => new ClassPartialMapDesigner().DesignClassPartialMaps(partialMapMethod),
                    PartialConstructorMapMethod partialConstructorMapMethod 
                        => new ClassPartialConstructorMapDesigner().DesignClassPartialConstructorMaps(partialConstructorMapMethod),
                    _ => ImmutableArray.Create<TypeMap>()
                };
                var usings = mapperClassDeclaration.Node.GetUsingsAndNamespace();
                foreach (var map in maps)
                {
                    planner.AddCustomMap(map, usings);
                }
            }

            var commonMappers = GenerateCommonMapper(planner);
            commonMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CommonMapper", SourceText.From(mapper, Encoding.UTF8)));

            var customMappers = GenerateCustomMappers(planner);
            customMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CustomMapper", SourceText.From(mapper, Encoding.UTF8)));
        }

        private List<string> GenerateCommonMapper(MapPlanner planner)
        {
            var commonMapperGenerator = new CommonMapperGenerator();
            var commonMapGroups = planner.MapGroups.Where(x => x.Priority == CodeAnalysis.MapPriority.Common);
            var commonMappers = commonMapGroups.Select(x => commonMapperGenerator.Generate(x));

            return commonMappers.ToList();
        }

        private List<string> GenerateCustomMappers(MapPlanner planner)
        {
            var customMapperGenerator = new CustomMapperGenerator();
            var customMapGroups = planner.MapGroups.Where(x => x.Priority == CodeAnalysis.MapPriority.Custom);
            var customMappers = customMapGroups.Select(x => customMapperGenerator.Generate(x));

            return customMappers.ToList();
        }
    }
}
