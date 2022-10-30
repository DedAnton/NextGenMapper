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
        (
        )
        {
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default!,
            int property2 = default!,
            int property3 = default!,
            int property4 = default!,
            int property5 = default!,
            int property6 = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}