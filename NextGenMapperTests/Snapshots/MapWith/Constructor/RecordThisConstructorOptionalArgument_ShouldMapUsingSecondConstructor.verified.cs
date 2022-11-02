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
            source.PropertyA,
            source.PropertyB,
            source.PropertyC
        )
        {
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int propertyC = default!,
            int propertyD = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int parameterA = default!,
            int parameterB = default!,
            int parameterC = default!,
            int propertyD = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}