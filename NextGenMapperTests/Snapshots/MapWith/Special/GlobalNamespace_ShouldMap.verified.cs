//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination MapWith<To>
        (
            this Source source,
            int property
        )
        => new Destination
        (
        )
        {
            Property = property
        };

    }
}