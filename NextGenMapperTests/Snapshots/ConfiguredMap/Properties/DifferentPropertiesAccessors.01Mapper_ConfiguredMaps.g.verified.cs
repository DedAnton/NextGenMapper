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
            int ForMapWith
        )
        => new Test.Destination
        {
            Property1 = source.Property1,
            Property4 = source.Property4,
            Property5 = source.Property5,
            ForMapWith = ForMapWith
        };
    }
}