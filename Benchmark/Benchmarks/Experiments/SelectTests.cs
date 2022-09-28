using BenchmarkDotNet.Engines;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class SelectTests
{
    [Params(100)]
    public int Length;

    private int[] list;
    private Consumer consumer;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        consumer = new Consumer();
        var rnd = new Random(12345);
        list = Enumerable.Range(0, Length).Select(x => rnd.Next()).ToArray();
    }

    [Benchmark]
    public void LinqSelect() => list.Select(x => x).Consume(consumer);

    [Benchmark]
    public void MySelect() => list.MySelect(x => x).Consume(consumer);

    [Benchmark(Baseline = true)]
    public Span<int> SpanFor() => SpanFor(list, x => x);

    [Benchmark]
    public Span<int> SpanForWithoutSelector() => SpanForWithoutSelector(list);

    private Span<int> SpanFor(int[] ints, Func<int, int> selector)
    {
        var intsSpan = ints.AsSpan();
        Span<int> span = new int[ints.Length];
        for (var i = 0; i < ints.Length; i++)
        {
            span[i] = selector(intsSpan[i]);
        }

        return span;
    }

    private Span<int> SpanForWithoutSelector(int[] ints)
    {
        var intsSpan = ints.AsSpan();
        Span<int> span = new int[ints.Length];
        for (var i = 0; i < ints.Length; i++)
        {
            span[i] = intsSpan[i];
        }

        return span;
    }
}

internal static class MapperExtensions
{
    internal static IEnumerable<TResult> MySelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        foreach (var item in source)
        {
            yield return selector(item);
        }
    }
}
