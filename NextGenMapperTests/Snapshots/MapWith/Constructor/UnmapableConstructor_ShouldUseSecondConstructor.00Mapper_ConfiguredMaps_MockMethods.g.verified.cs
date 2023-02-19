//HintName: Mapper_ConfiguredMaps_MockMethods.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default!,
            int notMatched = default!,
            int Property2 = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default!,
            int Property2 = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}