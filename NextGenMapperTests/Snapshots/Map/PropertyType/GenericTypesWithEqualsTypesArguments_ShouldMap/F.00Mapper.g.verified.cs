//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<int> Map<To>(this Test.Source<int> source) => new Test.Destination<int>
        (
        )
        {
            Property = source.Property
        };

    }
}