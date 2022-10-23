//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.EnumB Map<To>(this Test.EnumA source) => source switch
        {
            Test.EnumA.Good => Test.EnumB.Good,
            Test.EnumA.Bad => Test.EnumB.Bad,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping Test.EnumA to Test.EnumB")
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith
        )
        => new Test.Destination
        (
        )
        {
            SameProperty = source.SameProperty.Map<Test.EnumB>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            Test.EnumB sameProperty = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}