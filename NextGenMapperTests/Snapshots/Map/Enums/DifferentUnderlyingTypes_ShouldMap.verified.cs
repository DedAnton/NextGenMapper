//HintName: Mapper_EnumMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => source switch
        {
            Test.Source.Min => Test.Destination.Min,
            Test.Source.Normal => Test.Destination.Normal,
            Test.Source.Max => Test.Destination.Max,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), "Error when mapping Test.Source to Test.Destination")
        };
    }
}