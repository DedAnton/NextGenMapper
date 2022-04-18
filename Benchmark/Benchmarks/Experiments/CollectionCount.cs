namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class CollectionCount
{
    [Params(100, 1000, 10000, 100000, 1000000)]
    public int Length;

    private IEnumerable<int> enumerable;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var array = Enumerable.Range(0, Length).Select(x => x).ToArray();
        var list = array.ToList();
        //enumerable = Enumerable.Range(0, Length).Select(x => x);
        enumerable = list;
    }

    [Benchmark]
    public int EnumerableCount() => enumerable.Count();

    [Benchmark]
    public bool NotEnumerableCount() => MyTryGetNonEnumerableCount(enumerable, out _);

    //[Benchmark]
    //public bool LinqNotEnumerableCount() => enumerable.TryGetNonEnumeratedCount(out _);

    public bool MyTryGetNonEnumerableCount<T>(IEnumerable<T> input, out int count)
    {
        if (input is ICollection<T> collection)
        {
            count = collection.Count;
            return true;
        }
        else if (input is IReadOnlyCollection<T> readOnlyCollection)
        {
            count = readOnlyCollection.Count;
            return true;
        }
        else if (input is T[] array)
        {
            count = array.Length;
            return true;
        }

        count = 0;

        return false;
    }
}