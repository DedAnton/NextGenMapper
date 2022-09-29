using Benchmark.Utils;
using NextGenMapper.CodeAnalysis;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CircleReferencesList
{
    [Params(1, 10, 20, 50, 100, 1000)]
    public int ReferencesCount;

    private HashSet<(ITypeSymbol, ITypeSymbol)> hashSet;
    private List<(ITypeSymbol, ITypeSymbol)> list;
    private (ITypeSymbol, ITypeSymbol) testUnit;

    [GlobalSetup(Targets = new string[] { nameof(HashSet), nameof(List) })]
    public void SetupBenchmark()
    {
        var classes = Enumerable.Range(0, ReferencesCount)
            .Select(x => TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 0, 0, "Nested", $"Source{x}", $"Destination{x}"));
        var source = TestTypeSourceGenerator.GenerateClassesSource(string.Join("\r\n", classes));
        var compilation = source.CreateCompilation("test");
        var references = Enumerable.Range(0, ReferencesCount)
            .Select(x => ((ITypeSymbol)compilation.GetTypeByMetadataName($"Test.Source{x}"), (ITypeSymbol)compilation.GetTypeByMetadataName($"Test.Destination{x}")))
            .ToArray();
        hashSet = new HashSet<(ITypeSymbol, ITypeSymbol)>(references, new MapTypesEqualityComparer());
        list = new List<(ITypeSymbol, ITypeSymbol)>(references);
        testUnit = references[^1];
    }


    [BenchmarkCategory("CircleReferences"), Benchmark(Baseline = true)]
    public bool HashSet() => hashSet.Contains(testUnit);

    [BenchmarkCategory("CircleReferences"), Benchmark]
    public bool List() => list.Contains(testUnit, new MapTypesEqualityComparer());
}
