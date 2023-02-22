//HintName: Mapper_EnumMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.EnumB Map<To>(this Test.EnumA source) => source switch
        {
            Test.EnumA.Good => Test.EnumB.Good,
            Test.EnumA.Bad => Test.EnumB.Bad,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), "Error when mapping Test.Source to Test.Destination")
        };
    }
}