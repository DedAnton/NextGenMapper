using BenchmarkDotNet.Engines;
using System.Collections.Immutable;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Sorting
{
    [Params(10, 100, 1000, 10000)]
    public int Length;

    private Consumer consumer;
    private ImmutableArray<int> immutableArray;
    private int[] array;
    private IntComparerOne comparerOne;
    private IntComparerTwo comparerTwo;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        //var collection = Enumerable.Range(0, Length).Select(x => x).Reverse().ToArray();
        var collection = GenerateRandomNumber(Length);

        consumer = new Consumer();
        comparerOne = new IntComparerOne();
        comparerTwo = new IntComparerTwo();
        immutableArray = ImmutableArray.Create(collection);
        array = collection.ToArray();
    }

    [BenchmarkCategory("Sorting"), Benchmark(Baseline = true)]
    public void Linq_OrderBy() => immutableArray.OrderBy(x => x).Consume(consumer);

    [BenchmarkCategory("Sorting"), Benchmark]
    public ImmutableArray<int> Sort_Comparison() => immutableArray.Sort((x, y) => x > y ? 1 : y > x ? -1 : 0);

    [BenchmarkCategory("Sorting"), Benchmark]
    public ImmutableArray<int> Sort_IComparer() => immutableArray.Sort(comparerOne);
    public class IntComparerOne : IComparer<int>
    {
        public int Compare(int x, int y) => x > y ? 1 : y > x ? -1 : 0;
    }

    [BenchmarkCategory("Sorting"), Benchmark]
    public ImmutableArray<int> Sort_IComparer_Compare() => immutableArray.Sort(comparerTwo);
    public class IntComparerTwo : IComparer<int>
    {
        public int Compare(int x, int y) => x.CompareTo(y);
    }

    [BenchmarkCategory("Sorting"), Benchmark]
    public Span<int> Span_Sort_IComparer_Compare()
    {
        var span = array.AsSpan();
        span.Sort(comparerTwo);

        return span;
    }

    [BenchmarkCategory("Sorting"), Benchmark]
    public int[] BubbleSortArray() => SortArray(ref array);

    [BenchmarkCategory("Sorting"), Benchmark]
    public Span<int> BubbleSortSpan() => SortSpan(array);

    [BenchmarkCategory("Sorting"), Benchmark]
    public Span<int> BubbleSortSpan_IComparer() => SortSpan_IComparer(array, comparerTwo);

    private Span<int> SortSpan_IComparer(Span<int> span, IComparer<int> comparer)
    {
        var n = span.Length;
        bool swapRequired;
        for (int i = 0; i < n - 1; i++)
        {
            swapRequired = false;
            for (int j = 0; j < n - i - 1; j++)
                if (comparer.Compare(span[j], span[j + 1]) > 0)
                {
                    (span[j + 1], span[j]) = (span[j], span[j + 1]);
                    swapRequired = true;
                }
            if (swapRequired == false)
                break;
        }
        return span;
    }

    private Span<int> SortSpan(Span<int> span)
    {
        var n = span.Length;
        bool swapRequired;
        for (int i = 0; i < n - 1; i++)
        {
            swapRequired = false;
            for (int j = 0; j < n - i - 1; j++)
                if (span[j] > span[j + 1])
                {
                    var tempVar = span[j];
                    span[j] = span[j + 1];
                    span[j + 1] = tempVar;
                    swapRequired = true;
                }
            if (swapRequired == false)
                break;
        }
        return span;
    }

    private int[] SortArray(ref int[] arr)
    {
        var n = arr.Length;
        bool swapRequired;
        for (int i = 0; i < n - 1; i++)
        {
            swapRequired = false;
            for (int j = 0; j < n - i - 1; j++)
                if (arr[j] > arr[j + 1])
                {
                    var tempVar = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = tempVar;
                    swapRequired = true;
                }
            if (swapRequired == false)
                break;
        }
        return arr;
    }

    public static int[] GenerateRandomNumber(int size)
    {
        var array = new int[size];
        var rand = new Random();
        var maxNum = 10000;
        for (int i = 0; i < size; i++)
            array[i] = rand.Next(maxNum + 1);
        return array;
    }
}