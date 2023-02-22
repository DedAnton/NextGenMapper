namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class HashSetVsSpan
{
    [Params(10, 100, 1000, 10000)]
    public int Length;

    private int[] array;
    private HashSet<int> hashSet;

    private int targetWorth;
    private int targetMedium;
    private int targetBest;

    [GlobalSetup]
    public void Setup()
    {
        array = Enumerable.Range(0, Length).ToArray();
        hashSet = new HashSet<int>(array);
        targetWorth = array[^1];
        targetMedium = array[array.Length / 2];
        targetBest = array[0];
    }

    [Benchmark]
    public bool ArrayWorth()
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == targetWorth)
            {
                return true;
            }
        }

        return false;
    }

    [Benchmark]
    public bool ArrayMedium()
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == targetMedium)
            {
                return true;
            }
        }

        return false;
    }

    [Benchmark]
    public bool ArrayBest()
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == targetBest)
            {
                return true;
            }
        }

        return false;
    }

    [Benchmark]
    public bool HashSet() => hashSet.Contains(targetMedium);

    [Benchmark]
    public bool HashSetWithCreating()
    {
        var set = new HashSet<int>(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            set.Add(array[i]);
        }

        return set.Contains(targetMedium);
    }
}