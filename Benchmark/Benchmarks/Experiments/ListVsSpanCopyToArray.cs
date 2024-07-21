namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class ListVsSpanCopyToArray
{
    [Params(1, 10, 100, 1000, 10000)]
    public int Length;

    private int[] sourceArray;

    [GlobalSetup]
    public void Setup()
    {
        sourceArray = Enumerable.Range(0, Length).ToArray();
    }

    [Benchmark]
    public List<int> CreateList()
    {
        var list = new List<int>(sourceArray.Length);
        for (int i = 0; i < sourceArray.Length; i++)
        {
            list.Add(sourceArray[i]);
        }

        return list;
    }

    [Benchmark]
    public int[] CreateArray()
    {
        var array = new int[sourceArray.Length];
        var pos = 0;
        for (int i = 0; i < sourceArray.Length; i++)
        {
            array[pos] = sourceArray[i];
            pos++;
        }

        return array.AsSpan(0, pos).ToArray();
    }

    [Benchmark(Baseline = true)]
    public Span<int> CreateArrayReturnSpan()
    {
        var array = new int[sourceArray.Length];
        var pos = 0;
        for (int i = 0; i < sourceArray.Length; i++)
        {
            array[pos] = sourceArray[i];
            pos++;
        }

        return array.AsSpan(0, pos);
    }
}
