//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.ClassB Map<To>(this Test.ClassA source) => new Test.ClassB()
        {
            Property = source.Property
        };
    }
}