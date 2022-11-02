//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            SameProperty = source.SameProperty.Map<Test.ClassB>()
        };

        internal static Test.ClassB Map<To>(this Test.ClassA source) => new Test.ClassB()
        {
            Property = source.Property
        };
    }
}