using System;
using System.Collections.Immutable;
using SystemUnsafe = System.Runtime.CompilerServices.Unsafe;

namespace NextGenMapper.Utils;
internal static class Unsafe
{
    public static ImmutableArray<T> CastArrayToImmutableArray<T>(ref T[] array)
        => SystemUnsafe.As<T[], ImmutableArray<T>>(ref array);

    public static ImmutableArray<T> SpanToImmutableArray<T>(Span<T> span)
    {
        var newArray = span.ToArray();

        return CastArrayToImmutableArray(ref newArray);
    }
}
