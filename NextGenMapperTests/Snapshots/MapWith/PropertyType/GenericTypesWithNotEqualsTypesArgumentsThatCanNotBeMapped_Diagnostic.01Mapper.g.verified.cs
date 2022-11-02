﻿//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<string> MapWith<To>
        (
            this Test.Source<int> source,
            int forMapWith
        )
        => new Test.Destination<string>
        {
            Property = source.Property.Map<string>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination<string> MapWith<To>
        (
            this Test.Source<int> source,
            string property = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}