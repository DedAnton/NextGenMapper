//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            Property1 = source.Property1,
            Property4 = source.Property4,
            Property5 = source.Property5,
            Property7 = source.Property7,
            Property10 = source.Property10,
            Property11 = source.Property11
        };
    }
}