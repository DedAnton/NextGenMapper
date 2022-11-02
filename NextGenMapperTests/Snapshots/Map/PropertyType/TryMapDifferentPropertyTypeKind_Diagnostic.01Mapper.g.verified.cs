//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            Property1 = source.Property1.Map<Test.EnumA>(),
            Property2 = source.Property2.Map<Test.StructA>(),
            Property3 = source.Property3.Map<int[]>()
        };
    }
}