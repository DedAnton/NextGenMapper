namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CharComparision
{
    private char charA;
    private char charB;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        charA = 'a';
        charB = 'b';
    }

    [BenchmarkCategory("CharComparision"), Benchmark(Baseline = true)]
    public bool Equal() => charA.Equals(charB);
    [BenchmarkCategory("CharComparision"), Benchmark]
    public bool EqualOperator() => charA == charB;
    [BenchmarkCategory("CharComparision"), Benchmark]
    public bool Compare() => charA.CompareTo(charB) == 0;
    [BenchmarkCategory("CharComparision"), Benchmark]
    public bool ConvertToByte() => (byte)charA == (byte)charB;
}