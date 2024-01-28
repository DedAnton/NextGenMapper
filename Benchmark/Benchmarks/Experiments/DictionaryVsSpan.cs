namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class DictionaryVsSpan
{
    [Params(1, 10, 100, 1000, 10000)]
    public int Length;

    private string[] array;
    private Dictionary<string, string> dictionary;

    [GlobalSetup]
    public void Setup()
    {
        array = Enumerable.Range(0, Length).Select(x => x.ToString()).ToArray();
        dictionary = array.ToDictionary(x => x, x => x, StringComparer.InvariantCulture);
    }

    [Benchmark]
    public Dictionary<string, string> CreateDictionary()
    {
        var dict = new Dictionary<string, string>(array.Length, StringComparer.InvariantCulture);
        foreach (var item in array.AsSpan())
        {
            dict.Add(item, item);
        }

        return dict;
    }

    [Benchmark]
    public void Dictionary()
    {
        foreach (var item in array.AsSpan())
        {
            if (dictionary.TryGetValue(item, out var value))
            {
                break;
            }
        }
    }

    [Benchmark]
    public void Array()
    {
        foreach (var item in array.AsSpan())
        {
            foreach (var item2 in array.AsSpan())
                if (item.Equals(item2, StringComparison.InvariantCulture))
                {
                    break;
                }
        }
    }
}