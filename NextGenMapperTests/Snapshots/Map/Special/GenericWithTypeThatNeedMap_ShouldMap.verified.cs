//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<Test.B> Map<To>(this Test.Source<Test.A> source) => new Test.Destination<Test.B>()
        {
            Property = source.Property.Map<Test.B>()
        };

        internal static Test.B Map<To>(this Test.A source) => new Test.B
        (
            source.InnerProperty
        );
    }
}