//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this System.Collections.Generic.ICollection<int> source)
        {
            if (!source.TryGetSpan(out var span))
            {
                span = System.Linq.Enumerable.ToArray(source);
            }

            var destination = new int[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                destination[i] = span[i];
            }

            return destination;
        }

    }
}