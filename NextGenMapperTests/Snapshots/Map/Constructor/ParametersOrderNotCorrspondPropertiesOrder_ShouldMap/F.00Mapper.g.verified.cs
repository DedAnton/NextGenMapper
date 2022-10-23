//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination
        (
            source.Property4,
            source.Property2,
            source.Property3,
            source.Property1
        )
        {
        };

    }
}