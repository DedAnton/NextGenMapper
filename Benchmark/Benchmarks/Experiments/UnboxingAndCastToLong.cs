using System.Runtime.CompilerServices;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class UnboxingAndCastToLong
{
    private readonly object _boxed = 123;

    [Benchmark]
    public long ManyIfAndCast()
    {
        var type = _boxed.GetType();
        if (type == typeof(sbyte))
        {
            return (int)_boxed;
        }
        if (type == typeof(byte))
        {
            return (byte)_boxed;
        }
        if (type == typeof(short))
        {
            return (short)_boxed;
        }
        if (type == typeof(ushort))
        {
            return (ushort)_boxed;
        }
        if (type == typeof(int))
        {
            return (int)_boxed;
        }
        if (type == typeof(uint))
        {
            return (uint)_boxed;
        }
        if (type == typeof(long))
        {
            return (long)_boxed;
        }

        return 0;
    }

    [Benchmark]
    public long ManyIfAndCastAndUnsafeAs()
    {
        var type = _boxed.GetType();
        if (type == typeof(sbyte))
        {
            var unboxed = (sbyte)_boxed;
            return Unsafe.As<sbyte, long>(ref unboxed);
        }
        if (type == typeof(byte))
        {
            var unboxed = (byte)_boxed;
            return Unsafe.As<byte, long>(ref unboxed);
        }
        if (type == typeof(short))
        {
            var unboxed = (short)_boxed;
            return Unsafe.As<short, long>(ref unboxed);
        }
        if (type == typeof(ushort))
        {
            var unboxed = (ushort)_boxed;
            return Unsafe.As<ushort, long>(ref unboxed);
        }
        if (type == typeof(int))
        {
            var unboxed = (int)_boxed;
            return Unsafe.As<int, long>(ref unboxed);
        }
        if (type == typeof(uint))
        {
            var unboxed = (uint)_boxed;
            return Unsafe.As<uint, long>(ref unboxed);
        }
        if (type == typeof(long))
        {
            var unboxed = (long)_boxed;
            return unboxed;
        }

        return 0;
    }

    [Benchmark]
    public long ManyIfAndUnsafeUnboxAndUnsafeAs()
    {
        var type = _boxed.GetType();
        if (type == typeof(sbyte))
        {
            return Unsafe.As<sbyte, long>(ref Unsafe.Unbox<sbyte>(_boxed));
        }
        if (type == typeof(byte))
        {
            return Unsafe.As<byte, long>(ref Unsafe.Unbox<byte>(_boxed));
        }
        if (type == typeof(short))
        {
            return Unsafe.As<short, long>(ref Unsafe.Unbox<short>(_boxed));
        }
        if (type == typeof(ushort))
        {
            return Unsafe.As<ushort, long>(ref Unsafe.Unbox<ushort>(_boxed));
        }
        if (type == typeof(int))
        {
            return Unsafe.As<int, long>(ref Unsafe.Unbox<int>(_boxed));
        }
        if (type == typeof(uint))
        {
            return Unsafe.As<uint, long>(ref Unsafe.Unbox<uint>(_boxed));
        }
        if (type == typeof(long))
        {
            return Unsafe.Unbox<sbyte>(_boxed);
        }

        return 0;
    }

    [Benchmark]
    public long Switch()
    {
        return _boxed switch
        {
            sbyte => (sbyte)_boxed,
            byte => (byte)_boxed,
            short => (short)_boxed,
            ushort => (ushort)_boxed,
            int => (int)_boxed,
            uint => (uint)_boxed,
            long => (long)_boxed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
