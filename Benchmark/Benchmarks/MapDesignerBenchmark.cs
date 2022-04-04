using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapperTests;
using System.Collections.Generic;

namespace Benchmark.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net50)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MapDesignerBenchmark
    {
        [BenchmarkCategory("Properties"), Benchmark]
        [ArgumentsSource(nameof(GenerateCommonClassesMapPairs))]
        public List<ClassMap> Properties(TypesMapPair mapPair) => new (new ClassMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To).ToArray());

        [BenchmarkCategory("NestedClasses"), Benchmark]
        [ArgumentsSource(nameof(GenerateNestedClassesMapPairs))]
        public List<ClassMap> Nested(TypesMapPair mapPair) => new (new ClassMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To).ToArray());

        [BenchmarkCategory("Enums"), Benchmark]
        [ArgumentsSource(nameof(GenerateEnumsMapPairs))]
        public EnumMap Enums(TypesMapPair mapPair) => new EnumMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To);

        public IEnumerable<TypesMapPair> GenerateCommonClassesMapPairs()
        {
            var groups = new List<(string Description, string Classes)>()
            {
                ("props_1_init", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 0, 0)),
                ("props_1_cstr", TestTypeSourceGenerator.GenerateClassMapPair(1, 1, 0, 0)),
                ("props_10_init", TestTypeSourceGenerator.GenerateClassMapPair(10, 0, 0, 0)),
                ("props_10_cstr", TestTypeSourceGenerator.GenerateClassMapPair(10, 10, 0, 0)),
                ("props_100_init", TestTypeSourceGenerator.GenerateClassMapPair(100, 0, 0, 0)),
                ("props_100_cstr", TestTypeSourceGenerator.GenerateClassMapPair(100, 100, 0, 0)),
            };

            foreach (var (Description, Classes) in groups)
            {
                var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
                var compilation = source.CreateCompilation("test");
                var from = compilation.GetTypeByMetadataName("Test.Source");
                var to = compilation.GetTypeByMetadataName("Test.Destination");

                yield return new TypesMapPair(Description, from, to);
            }
        }

        public IEnumerable<TypesMapPair> GenerateNestedClassesMapPairs()
        {
            var groups = new List<(string Description, string Classes)>()
            {
                ("depth_1", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 1)),
                ("depth_2", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 2)),
                ("depth_3", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 3)),
                ("depth_4", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 4)),
            };

            foreach (var (Description, Classes) in groups)
            {
                var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
                var compilation = source.CreateCompilation("test");
                var from = compilation.GetTypeByMetadataName("Test.Source");
                var to = compilation.GetTypeByMetadataName("Test.Destination");

                yield return new TypesMapPair(Description, from, to);
            }
        }

        public IEnumerable<TypesMapPair> GenerateEnumsMapPairs()
        {
            var groups = new List<(string Description, string Classes)>()
            {
                ("fields_1", TestTypeSourceGenerator.GenerateEnumMapPair(1, 0)),
                ("fields_10", TestTypeSourceGenerator.GenerateEnumMapPair(10, 0)),
                ("fields_100", TestTypeSourceGenerator.GenerateEnumMapPair(100, 0))
            };

            foreach (var (Description, Classes) in groups)
            {
                var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
                var compilation = source.CreateCompilation("test");
                var from = compilation.GetTypeByMetadataName("Test.SourceEnum");
                var to = compilation.GetTypeByMetadataName("Test.DestinationEnum");

                yield return new TypesMapPair(Description, from, to);
            }
        }
    }

    public class TypesMapPair
    {
        public TypesMapPair(string name, ITypeSymbol from, ITypeSymbol to)
        {
            Name = name;
            From = from;
            To = to;
        }

        public string Name { get; set; }
        public ITypeSymbol From { get; set; }
        public ITypeSymbol To { get; set; }

        public override string ToString() => Name;
    }
}
