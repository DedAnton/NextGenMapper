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
            int property1,
            int property2,
            int property3,
            int property4
        )
        => new Test.Destination
        (
            property1,
            property2,
            property3,
            property4
        );
    }
}