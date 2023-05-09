//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.B Map<To>(this Test.A source) => new Test.B()
        {
            Property = source.Property
        };

        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            PropertyThatNeedMap = source.PropertyThatNeedMap?.Map<Test.B>()
        };
    }
}