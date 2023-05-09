//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.B Map<To>(this Test.A source) => new Test.B()
        {
            Property = source.Property
        };
    }
}