//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            byte forMapWith1
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith1 = forMapWith1
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property = default!,
            byte forMapWith1 = default!,
            short forMapWith2 = default!,
            int forMapWith3 = default!,
            long forMapWith4 = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            short forMapWith2
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith2 = forMapWith2
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith3
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith3 = forMapWith3
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            long forMapWith4
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith4 = forMapWith4
        };
    }
}