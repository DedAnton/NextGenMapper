//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            byte ForMapWith1
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith1 = ForMapWith1
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            short ForMapWith2
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith2 = ForMapWith2
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int ForMapWith3
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith3 = ForMapWith3
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            long ForMapWith4
        )
        => new Test.Destination
        {
            Property = source.Property,
            ForMapWith4 = ForMapWith4
        };
    }
}