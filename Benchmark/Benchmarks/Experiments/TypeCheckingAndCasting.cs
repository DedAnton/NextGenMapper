using System.Runtime.CompilerServices;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class TypeCheckingAndCasting
{
    private object stringAsObject = "1234";

    [Benchmark]
    public bool Is() => stringAsObject is string;

    [Benchmark]
    public bool GetTypeEqualTypeOf() => stringAsObject.GetType() == typeof(string);

    [Benchmark]
    public string As() => stringAsObject as string;

    [Benchmark]
    public string UnsafeAs()
    {
        if (stringAsObject.GetType() == typeof(string))
        {
            return Unsafe.As<string>(stringAsObject);
        }

        return "";
    }
}
