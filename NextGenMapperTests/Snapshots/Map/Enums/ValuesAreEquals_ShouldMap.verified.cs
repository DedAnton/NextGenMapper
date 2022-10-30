//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => source switch
        {
            Test.Source.A => Test.Destination.D,
            Test.Source.B => Test.Destination.E,
            Test.Source.C => Test.Destination.F,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping Test.Source to Test.Destination")
        };

    }
}