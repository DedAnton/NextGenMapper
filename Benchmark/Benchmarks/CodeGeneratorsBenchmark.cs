using Benchmark.Utils;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeGeneration;

namespace Benchmark.Benchmarks;

[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CodeGeneratorsBenchmark
{
    [BenchmarkCategory("Common"), Benchmark]
    [ArgumentsSource(nameof(GenerateCommonClassesMapPairs))]
    public string New(BenchmarkInput input) => new MapperClassBuilder().Generate(input.MapGroup);

    public IEnumerable<BenchmarkInput> GenerateCommonClassesMapPairs()
    {
        var classes = new List<(string Description, string Classes)>()
        {
            ("props_1_init", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 0, 0)),
            ("props_1_cstr", TestTypeSourceGenerator.GenerateClassMapPair(1, 1, 0, 0)),
            ("props_10_init", TestTypeSourceGenerator.GenerateClassMapPair(10, 0, 0, 0)),
            ("props_10_cstr", TestTypeSourceGenerator.GenerateClassMapPair(10, 10, 0, 0)),
            ("props_100_init", TestTypeSourceGenerator.GenerateClassMapPair(100, 0, 0, 0)),
            ("props_100_cstr", TestTypeSourceGenerator.GenerateClassMapPair(100, 100, 0, 0)),
        };

        foreach (var (Description, Classes) in classes)
        {
            var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
            var compilation = source.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.Source");
            var to = compilation.GetTypeByMetadataName("Test.Destination");
            var maps = new ClassMapDesigner(new()).DesignMapsForPlanner(from, to);
            var mapGroup = new MapGroup(maps[0], new() { "using NextGenMapper.Extensions;" }, MapPriority.Common);
            for(var i = 1; i < maps.Count; i++)
            {
                mapGroup.Add(maps[i]);
            }

            yield return new BenchmarkInput(Description, mapGroup);
        }
    }

    public class BenchmarkInput
    {
        public BenchmarkInput(string name, MapGroup mapGroup)
        {
            Name = name;
            MapGroup = mapGroup;
        }

        public string Name { get; set; }
        public MapGroup MapGroup { get; set; }

        public override string ToString() => Name;
    }
}