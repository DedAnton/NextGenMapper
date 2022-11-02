﻿//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.List<Test.Destination> Map<To>(this Test.Source[] source)
        {
            var sourceCollection = new System.Span<Test.Source>(source);
            var length = sourceCollection.Length;
            var destination = new System.Collections.Generic.List<Test.Destination>(length);
            for (var i = 0; i < length; i++)
            {
                destination.Add(sourceCollection[i].Map<Test.Destination>());
            }

            return destination;
        }
    }
}