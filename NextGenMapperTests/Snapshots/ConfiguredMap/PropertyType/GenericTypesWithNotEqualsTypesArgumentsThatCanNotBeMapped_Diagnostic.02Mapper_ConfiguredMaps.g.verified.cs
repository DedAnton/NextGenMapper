//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<string> MapWith<To>
        (
            this Test.Source<int> source,
            int ForMapWith
        )
        => new Test.Destination<string>
        {
            Property = source.Property.Map<string>(),
            ForMapWith = ForMapWith
        };
    }
}