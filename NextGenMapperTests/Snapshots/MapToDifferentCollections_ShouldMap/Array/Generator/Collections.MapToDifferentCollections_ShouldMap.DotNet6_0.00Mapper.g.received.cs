//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static long[] Map<To>(this int[] source)
        {
            var destination = new long[source.Length];
            var sourceSpan = new System.Span<int>(source);
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = sourceSpan[i];
            }

            return destination;
        }

    }
}