//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination
        (
            source.Property1,
            source.Property2,
            source.Property3,
            source.Property4,
            source.Property5,
            source.Property6,
            source.Property7,
            source.Property8,
            source.Property9,
            source.Property10,
            source.Property11,
            source.Property12,
            source.Property13,
            source.Property14,
            source.Property15,
            source.Property16
        )
        {
        };

    }
}