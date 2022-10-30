using Benchmark.Utils;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SymbolEquality : SourceGeneratorVerifier
{
    private ITypeSymbol symbolFrom;
    private ITypeSymbol symbolTo;
    private string textFrom;
    private string textTo;

    public override string TestGroup => throw new NotImplementedException();

    [GlobalSetup(Targets = new string[] { nameof(Symbol), nameof(Text), nameof(TextFromSymbol) })]
    public void SetupBenchmark()
    {
        var classes = TestTypeSourceGenerator.GenerateClassMapPair(10, 0, 0, 0, "Nested", "Source", "Destination");
        var source = TestTypeSourceGenerator.GenerateClassesSource(classes);
        var compilation = CreateCompilation(new[] { source }, "bench");
        symbolFrom = compilation.GetTypeByMetadataName("Test.Source");
        symbolTo = compilation.GetTypeByMetadataName("Test.Destination");
        textFrom = "Test.Source";
        textTo = "Test.Destination";
    }

    [BenchmarkCategory("Equality"), Benchmark]
    public bool Symbol() => SymbolEqualityComparer.Default.Equals(symbolFrom, symbolTo);
    [BenchmarkCategory("Equality"), Benchmark]
    public bool Text() => textFrom == textTo;
    [BenchmarkCategory("Equality"), Benchmark]
    public bool TextFromSymbol() => symbolFrom.ToString() == symbolTo.ToString();
}
