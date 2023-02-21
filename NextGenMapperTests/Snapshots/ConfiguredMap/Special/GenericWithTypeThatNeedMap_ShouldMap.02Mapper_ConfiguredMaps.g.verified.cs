//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<Test.B> MapWith<To>
        (
            this Test.Source<Test.A> source,
            int ForMapWith
        )
        => new Test.Destination<Test.B>
        {
            Property = source.Property.Map<Test.B>(),
            ForMapWith = ForMapWith
        };
    }
}