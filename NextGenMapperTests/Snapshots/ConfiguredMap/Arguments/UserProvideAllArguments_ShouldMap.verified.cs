//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int Property1,
            int Property2,
            int Property3,
            int Property4
        )
        => new Test.Destination
        {
            Property1 = Property1,
            Property2 = Property2,
            Property3 = Property3,
            Property4 = Property4
        };
    }
}