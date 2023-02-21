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
            int ForMapWith1,
            int ForMapWith2
        )
        => new Test.Destination
        {
            Property1 = source.Property1,
            Property2 = source.Property2,
            ForMapWith1 = ForMapWith1,
            ForMapWith2 = ForMapWith2
        };
    }
}