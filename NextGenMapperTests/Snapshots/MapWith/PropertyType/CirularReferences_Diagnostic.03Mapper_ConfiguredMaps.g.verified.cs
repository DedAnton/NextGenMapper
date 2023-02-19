//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.DestinationA MapWith<To>
        (
            this Test.SourceA source,
            int ForMapWith
        )
        => new Test.DestinationA
        {
            Reference = source.Reference.Map<Test.DestinationB>(),
            ForMapWith = ForMapWith
        };
    }
}