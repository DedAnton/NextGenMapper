using System.Runtime.CompilerServices;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class Unboxing
{
    private readonly object _boxedInt = 123;

    [Benchmark]
    public int Is()
    {
        if (_boxedInt is int unboxedInt)
        {
            return unboxedInt;
        }

        return 0;
    }

    [Benchmark]
    public int GetTypeAndCast()
    {
        if (_boxedInt.GetType() == typeof(int))
        {
            return (int)_boxedInt;
        }

        return 0;
    }

    [Benchmark]
    public int GetTypeAndUnbox()
    {
        if (_boxedInt.GetType() == typeof(int))
        {
            return Unsafe.Unbox<int>(_boxedInt);
        }

        return 0;
    }
}