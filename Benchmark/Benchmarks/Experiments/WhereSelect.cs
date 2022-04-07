using BenchmarkDotNet.Engines;
using System.Collections.Immutable;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class WhereSelect
{
    [Params(10, 100, 1000)]
    public int ItemsCount;

    private List<int> collection;
    private Consumer consumer;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        collection = Enumerable.Range(0, ItemsCount).Select(x => x).ToList();
        consumer = new Consumer();
    }

    //[BenchmarkCategory("WhereSelect"), Benchmark]
    public List<int> CommonList()
    {
        var result = new List<int>();
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                result.Add(item + 5);
            }
        }

        return result;
    }

    [BenchmarkCategory("WhereSelect"), Benchmark]
    public Span<int> CommonArray()
    {
        var result = new int[collection.Count];
        var i = 0;
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                result[i] = item + 5;
                i++;
            }
        }

        return result.AsSpan(0, i).ToArray();
    }

    [BenchmarkCategory("WhereSelect"), Benchmark]
    public Span<int> CommonArraySpan()
    {
        var result = new int[collection.Count];
        var i = 0;
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                result[i] = item + 5;
                i++;
            }
        }

        return result.AsSpan(0, i);
    }

    //[BenchmarkCategory("WhereSelect"), Benchmark]
    public void Yield() => YieldMethod().Consume(consumer);
    private IEnumerable<int> YieldMethod()
    {
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                yield return item + 5;
            }
        }
    }

    [BenchmarkCategory("WhereSelect"), Benchmark]
    public ImmutableArray<int> ImmutableArrays()
    {
        var result = ImmutableArray.Create<int>();
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                result = result.Add(item + 5);
            }
        }

        return result;
    }

    //[BenchmarkCategory("WhereSelect"), Benchmark]
    public ImmutableList<int> ImmutableLists()
    {
        var result = ImmutableList.Create<int>();
        foreach (var item in collection)
        {
            if (item % 10 == 0)
            {
                result = result.Add(item + 5);
            }
        }

        return result;
    }

    //[BenchmarkCategory("WhereSelect"), Benchmark]
    public void Linq() => collection.Where(i => i % 10 == 0).Select(i => i + 5).Consume(consumer);
}
