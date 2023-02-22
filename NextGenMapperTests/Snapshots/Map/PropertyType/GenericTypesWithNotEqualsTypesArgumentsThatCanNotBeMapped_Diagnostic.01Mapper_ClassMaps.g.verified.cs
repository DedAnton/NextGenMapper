//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<string> Map<To>(this Test.Source<int> source) => new Test.Destination<string>()
        {
            Property = source.Property.Map<string>()
        };
    }
}