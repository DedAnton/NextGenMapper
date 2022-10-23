//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination
        (
        )
        {
            Property = source.Property
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
            Property = source.Property,
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}