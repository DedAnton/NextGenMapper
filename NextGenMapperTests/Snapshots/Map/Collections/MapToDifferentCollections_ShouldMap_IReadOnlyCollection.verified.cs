//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.IReadOnlyCollection<int> Map<To>(this System.Collections.Generic.IEnumerable<int> source)
        {
            if (!source.TryGetSpan(out var span))
            {
                span = System.Linq.Enumerable.ToArray(source);
            }

            var destination = new System.Collections.Generic.List<int>(span.Length);
            for (var i = 0; i < span.Length; i++)
            {
                destination.Add(span[i]);
            }

            return destination;
        }

    }
}