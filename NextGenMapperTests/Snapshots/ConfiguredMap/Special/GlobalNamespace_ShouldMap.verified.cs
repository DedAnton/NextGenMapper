//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination MapWith<To>
        (
            this Source source,
            int Property
        )
        => new Destination
        {
            Property = Property
        };
    }
}