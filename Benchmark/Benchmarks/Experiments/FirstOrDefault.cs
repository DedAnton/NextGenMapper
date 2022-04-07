using System.Collections.Immutable;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class FirstOrDefault
{
    [Params(10, 100, 1000)]
    public int Length;

    private ImmutableArray<Item> array;
    private List<Item> list;
    private IEnumerable<Item> enumerable;
    private int test;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var collection = Enumerable.Range(0, Length).Select(x => new Item(x, $"item {x}")).ToArray();

        array = ImmutableArray.Create(collection);
        list = new List<Item>(collection);
        enumerable = new List<Item>(collection);
        test = collection.Length - 1;
    }

    [BenchmarkCategory("FirstOrDefault"), Benchmark]
    public Item IEnumerable() => enumerable.FirstOrDefault(x => x.Id == test);

    [BenchmarkCategory("FirstOrDefault"), Benchmark]
    public Item List() => list.FirstOrDefault(x => x.Id == test);

    [BenchmarkCategory("FirstOrDefault"), Benchmark]
    public Item Array_FirstOrDefault() => array.FirstOrDefault(x => x.Id == test);

    [BenchmarkCategory("FirstOrDefault"), Benchmark(Baseline = true)]
    public Item Array_ForEach()
    {
        foreach (var item in array)
        {
            if (item.Id == test)
                return item;
        }

        return null;
    }

    public record Item(int Id, string Name);
}