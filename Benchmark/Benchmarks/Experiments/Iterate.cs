using System.Collections.Immutable;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Iterate
{
    [Params(10, 100, 1000, 10000)]
    public int Length;

    private ImmutableArray<int> immutableArray;
    private int[] array;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var collection = Enumerable.Range(0, Length).Select(x => x).ToArray();

        immutableArray = ImmutableArray.Create(collection);
        array = collection.ToArray();
    }

    [BenchmarkCategory("ImmutableArrayIterate"), Benchmark(Baseline = true)]
    public int ImmutableArrayForEach()
    {
        var sum = 0;
        foreach (var item in immutableArray)
        {
            sum += item;
        }

        return sum;
    }

    [BenchmarkCategory("ImmutableArrayIterate"), Benchmark]
    public int ImmutableArrayFor()
    {
        var sum = 0;
        for (var i = 0; i < Length; i++)
        {
            sum += immutableArray[i];
        }

        return sum;
    }

    [BenchmarkCategory("ImmutableArrayIterate"), Benchmark]
    public int ArrayForEach()
    {
        var sum = 0;
        foreach (var item in array)
        {
            sum += item;
        }

        return sum;
    }

    [BenchmarkCategory("ImmutableArrayIterate"), Benchmark]
    public int ArrayFor()
    {
        var sum = 0;
        for (var i = 0; i < Length; i++)
        {
            sum += array[i];
        }

        return sum;
    }
}