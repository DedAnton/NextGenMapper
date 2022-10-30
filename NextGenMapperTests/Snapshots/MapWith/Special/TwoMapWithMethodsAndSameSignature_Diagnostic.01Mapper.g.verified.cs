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
            int forMapWith1
        )
        => new Test.Destination
        (
        )
        {
            ForMapWith1 = forMapWith1
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith1 = default!,
            int forMapWith2 = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}