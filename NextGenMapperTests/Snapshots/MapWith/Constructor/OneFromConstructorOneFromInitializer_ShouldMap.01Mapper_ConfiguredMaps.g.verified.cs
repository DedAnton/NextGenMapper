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
            int forMapWith1,
            int ForMapWith2
        )
        => new Test.Destination
        (
            source.Property1,
            forMapWith1
        )
        {
            Property2 = source.Property2,
            ForMapWith2 = ForMapWith2
        };
    }
}