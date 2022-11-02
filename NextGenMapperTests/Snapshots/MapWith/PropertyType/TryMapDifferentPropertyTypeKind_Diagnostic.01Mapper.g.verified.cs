//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith
        )
        => new Test.Destination
        {
            Property1 = source.Property1.Map<Test.EnumA>(),
            Property2 = source.Property2.Map<Test.StructA>(),
            Property3 = source.Property3.Map<int[]>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            Test.EnumA property1 = default!,
            Test.StructA property2 = default!,
            int[] property3 = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}