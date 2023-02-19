//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<Test.ClassB> MapWith<To>
        (
            this Test.Source<Test.ClassA> source,
            int ForMapWith
        )
        => new Test.Destination<Test.ClassB>
        {
            Property = source.Property.Map<Test.ClassB>(),
            ForMapWith = ForMapWith
        };
    }
}