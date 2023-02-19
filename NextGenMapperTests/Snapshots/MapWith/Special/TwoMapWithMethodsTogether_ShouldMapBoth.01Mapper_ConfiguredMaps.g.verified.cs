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
            int ForMapWith1
        )
        => new Test.Destination
        {
            ForMapWith1 = ForMapWith1
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            long ForMapWith2
        )
        => new Test.Destination
        {
            ForMapWith2 = ForMapWith2
        };
    }
}