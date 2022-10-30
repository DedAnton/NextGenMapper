﻿//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination
        (
        )
        {
            SameProperty = source.SameProperty.Map<System.Collections.Generic.List<int>>()
        };

        internal static System.Collections.Generic.List<int> Map<To>(this int[] source)
        {
            var destination = new System.Collections.Generic.List<int>(source.Length);
            var sourceSpan = new System.Span<int>(source);
            for (var i = 0; i < source.Length; i++)
            {
                destination.Add(sourceSpan[i]);
            }

            return destination;
        }

    }
}