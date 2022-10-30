//HintName: Mapper.g.cs
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
            SameProperty = source.SameProperty.Map<Test.EnumB>()
        };

        internal static Test.EnumB Map<To>(this Test.EnumA source) => source switch
        {
            Test.EnumA.Good => Test.EnumB.Good,
            Test.EnumA.Bad => Test.EnumB.Bad,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping Test.EnumA to Test.EnumB")
        };

    }
}