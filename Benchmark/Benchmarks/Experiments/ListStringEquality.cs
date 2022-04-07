namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ListStringEquality
{
    [Params(1, 10, 20, 50, 100, 1000)]
    public int StringsCount;

    private HashSet<string> hashSet;
    private List<string> list;
    private string testUnit;

    [GlobalSetup(Targets = new string[] { nameof(HashSet), nameof(ListWithComparer), nameof(ListWithoutComparer) })]
    public void SetupBenchmark()
    {
        var strings = Enumerable.Range(0, StringsCount).Select(x => $"string{x}").ToArray();

        testUnit = strings[^1];
        hashSet = new HashSet<string>(strings, StringComparer.InvariantCultureIgnoreCase);
        list = new List<string>(strings);
    }


    [BenchmarkCategory("StringEquality"), Benchmark(Baseline = true)]
    public bool HashSet() => hashSet.Contains(testUnit);

    [BenchmarkCategory("StringEquality"), Benchmark]
    public bool ListWithComparer() => list.Contains(testUnit, StringComparer.InvariantCultureIgnoreCase);

    [BenchmarkCategory("StringEquality"), Benchmark]
    public bool ListWithoutComparer() => list.Contains(testUnit);
}