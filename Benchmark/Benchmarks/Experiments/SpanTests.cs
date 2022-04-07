namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SpanTests
{
    [Params(10, 100, 1000)]
    public int Length;

    private int[] array;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var collection = Enumerable.Range(0, Length).Select(x => x).ToArray();

        array = collection.ToArray();
    }


    [BenchmarkCategory("CircleReferences"), Benchmark(Baseline = true)]
    public int[] JustReturnArray() => array;

    [BenchmarkCategory("CircleReferences"), Benchmark]
    public Span<int> ReturnAsSpan() => array.AsSpan();
}

