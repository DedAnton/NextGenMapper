namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class StringComparision
{
    [Params(10, 100)]
    public int StringLength;

    private string strA;
    private string strB;

    private char[] charArrayA;
    private char[] charArrayB;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        Span<char> charArray = stackalloc char[StringLength];
        for (int i = 0; i < charArray.Length; i++)
        {
            charArray[i] = 'a';
        }

        strA = charArray.ToString();
        strB = charArray.ToString();
        charArrayA = charArray.ToArray();
        charArrayB = charArray.ToArray();
    }

    [BenchmarkCategory("StringsComparision"), Benchmark(Baseline = true)]
    public bool Equal() => strA.Equals(strB);
    [BenchmarkCategory("StringsComparision"), Benchmark]
    public bool Compare() => strA.CompareTo(strB) == 0;

    [BenchmarkCategory("StringsComparision_IgnoreCase"), Benchmark(Baseline = true)]
    public bool Equal_IgnoreCase() => strA.Equals(strB, StringComparison.InvariantCultureIgnoreCase);
    [BenchmarkCategory("StringsComparision_IgnoreCase"), Benchmark]
    public bool ToUpperInvariant_IgnoreCase() => strA.ToUpperInvariant() == strB.ToUpperInvariant();

    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool Ordinal() => strA.Equals(strB, StringComparison.Ordinal);
    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool OrdinalIgnoreCase() => strA.Equals(strB, StringComparison.OrdinalIgnoreCase);
    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool CurrentCulture() => strA.Equals(strB, StringComparison.CurrentCulture);
    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool CurrentCultureIgnoreCase() => strA.Equals(strB, StringComparison.CurrentCultureIgnoreCase);
    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool InvariantCulture() => strA.Equals(strB, StringComparison.InvariantCulture);
    [BenchmarkCategory("DifferentStringComparison"), Benchmark]
    public bool InvariantCultureIgnoreCase() => strA.Equals(strB, StringComparison.InvariantCultureIgnoreCase);

    [BenchmarkCategory("CharArray"), Benchmark]
    public bool Enumeration_EqualOperator()
    {
        for (var i = 0; i < charArrayA.Length; i++)
        {
            if (charArrayA[i] != charArrayB[i])
            {
                return false;
            }
        }

        return true;
    }
    [BenchmarkCategory("CharArray"), Benchmark]
    public bool Enumeration_CastToByte()
    {
        for (var i = 0; i < charArrayA.Length; i++)
        {
            if ((byte)charArrayA[i] != (byte)charArrayB[i])
            {
                return false;
            }
        }

        return true;
    }
}