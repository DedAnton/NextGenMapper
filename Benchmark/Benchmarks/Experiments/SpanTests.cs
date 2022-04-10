using System.Collections.Immutable;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SpanTests
{
    [Params(1, 10, 100, 1000, 10000)]
    public int Length;

    private int[] array;
    private ImmutableHashSet<string> immutableHashSet;
    private HashSet<string> hashSet;
    private string[] stringArray;
    private string testString;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var collection = Enumerable.Range(0, Length).Select(x => x).ToArray();
        immutableHashSet = ImmutableHashSet.Create<string>(StringComparer.InvariantCultureIgnoreCase);
        stringArray = new string[collection.Length];
        for(int i = 0; i < collection.Length; i++)
        {
            immutableHashSet.Add(collection[i].ToString());
            stringArray[i] = collection[i].ToString();
        }
        testString = (Length - 1).ToString();

        array = collection.ToArray();
        hashSet = new HashSet<string>(stringArray, StringComparer.InvariantCultureIgnoreCase);
    }


    //[BenchmarkCategory("CircleReferences"), Benchmark(Baseline = true)]
    public int[] JustReturnArray() => array;

    //[BenchmarkCategory("CircleReferences"), Benchmark]
    public Span<int> ReturnAsSpan() => array.AsSpan();


    [BenchmarkCategory("Contains"), Benchmark(Baseline = true)]
    public bool Contains_ImmutableHashSet() => immutableHashSet.Contains(testString);

    [BenchmarkCategory("Contains"), Benchmark]
    public bool Contains_HashSet() => hashSet.Contains(testString);

    [BenchmarkCategory("Contains"), Benchmark]
    public bool Contains_HashSet_WithCreating()
    {
        var localHashSet = new HashSet<string>(stringArray, StringComparer.InvariantCultureIgnoreCase);

        return localHashSet.Contains(testString);
    }

    [BenchmarkCategory("Contains"), Benchmark]
    public bool Contains_Array()
    {
        foreach(var item in stringArray)
        {
            if (item.Equals(testString, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

