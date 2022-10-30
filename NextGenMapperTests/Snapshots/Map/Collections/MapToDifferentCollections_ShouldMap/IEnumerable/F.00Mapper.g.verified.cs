﻿//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.IEnumerable<long> Map<To>(this System.Collections.Generic.IEnumerable<int> source)
        {
            if (!source.TryGetSpan(out var span))
            {
                span = System.Linq.Enumerable.ToArray(source);
            }

            var destination = new long[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                destination[i] = span[i];
            }

            return destination;
        }

    }
}