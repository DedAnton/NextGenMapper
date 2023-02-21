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
            int property9,
            int property10,
            int property11,
            int property12,
            int property13,
            int property14,
            int property15,
            int property16
        )
        => new Test.Destination
        (
            source.Property1,
            source.Property2,
            source.Property3,
            source.Property4,
            source.Property5,
            source.Property6,
            source.Property7,
            source.Property8,
            property9,
            property10,
            property11,
            property12,
            property13,
            property14,
            property15,
            property16
        );
    }
}