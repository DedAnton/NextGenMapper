//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this byte[] source)
        {
            var destination = new int[source.Length];
            var sourceSpan = new System.Span<byte>(source);
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = sourceSpan[i];
            }

            return destination;
        }

    }
}