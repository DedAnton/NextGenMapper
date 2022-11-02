//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.B Map<To>(this Test.A source) => new Test.B
        (
            source.InnerProperty
        );

        internal static Test.Destination<Test.B> MapWith<To>
        (
            this Test.Source<Test.A> source,
            int forMapWith
        )
        => new Test.Destination<Test.B>
        {
            Property = source.Property.Map<Test.B>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination<Test.B> MapWith<To>
        (
            this Test.Source<Test.A> source,
            Test.B property = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}