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
            int forMapWith1,
            int forMapWith2
        )
        => new Test.Destination
        (
            source.Property1,
            forMapWith1
        )
        {
            Property2 = source.Property2,
            ForMapWith2 = forMapWith2
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default!,
            int forMapWith1 = default!,
            int property2 = default!,
            int forMapWith2 = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}