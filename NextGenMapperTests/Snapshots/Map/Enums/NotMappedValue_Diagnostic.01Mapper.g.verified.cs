﻿//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => source switch
        {
            Test.Source.A => Test.Destination.A,
            Test.Source.B => Test.Destination.B,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), "Error when mapping Test.Source to Test.Destination")
        };
    }
}