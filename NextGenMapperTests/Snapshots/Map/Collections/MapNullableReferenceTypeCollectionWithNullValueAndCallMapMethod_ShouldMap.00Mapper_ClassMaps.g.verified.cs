//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source) => new Destination
        (
            source.Property
        );
    }
}