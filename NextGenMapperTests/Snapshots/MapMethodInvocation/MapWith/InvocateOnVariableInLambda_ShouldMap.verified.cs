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
            int propertyB
        )
        => new Test.Destination
        (
        )
        {
            PropertyB = propertyB
        };

    }
}