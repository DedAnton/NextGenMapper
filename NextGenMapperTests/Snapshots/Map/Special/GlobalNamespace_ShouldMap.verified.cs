//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source) => new Destination()
        {
            Property = source.Property
        };
    }
}