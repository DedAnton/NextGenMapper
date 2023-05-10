﻿//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Immutable.ImmutableList<int> Map<To>(this System.Collections.Generic.IEnumerable<int> source)
        {
            if (!source.TryGetSpan(out var sourceCollection))
            {
                sourceCollection = System.Linq.Enumerable.ToArray(source);
            }
            var length = sourceCollection.Length;
            var destination = System.Collections.Immutable.ImmutableList.CreateBuilder<int>();
            for (var i = 0; i < length; i++)
            {
                destination.Add(sourceCollection[i]);
            }

            return destination.ToImmutable();
        }
    }
}