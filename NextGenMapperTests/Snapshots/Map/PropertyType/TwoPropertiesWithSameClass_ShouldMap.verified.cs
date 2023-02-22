//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.InnerDestination Map<To>(this Test.InnerSource source) => new Test.InnerDestination()
        {
            Property = source.Property
        };

        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            Inner1 = source.Inner1.Map<Test.InnerDestination>(),
            Inner2 = source.Inner2.Map<Test.InnerDestination>()
        };
    }
}