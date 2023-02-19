//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<int> MapWith<To>
        (
            this Test.Source<int> source,
            int ForMapWith
        )
        => new Test.Destination<int>
        {
            Property = source.Property,
            ForMapWith = ForMapWith
        };
    }
}