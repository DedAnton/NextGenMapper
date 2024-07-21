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
            Test.B? PropertyThatNeedMap
        )
        => new Test.Destination
        {
            PropertyThatNeedMap = PropertyThatNeedMap,
            Property = source.Property
        };
    }
}