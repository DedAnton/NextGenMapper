//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith
        )
        => new Test.Destination
        (
            source.Property4,
            source.Property2,
            forMapWith,
            source.Property3,
            source.Property1
        )
        {
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property4 = default,
            int property2 = default,
            int forMapWith = default,
            int property3 = default,
            int property1 = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}