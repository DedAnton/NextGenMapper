//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.DestinationB Map<To>(this Test.SourceB source) => new Test.DestinationB()
        {
            Reference = source.Reference.Map<Test.DestinationC>()
        };

        internal static Test.DestinationC Map<To>(this Test.SourceC source) => new Test.DestinationC()
        {
            Reference = source.Reference.Map<Test.DestinationA>()
        };
    }
}