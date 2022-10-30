//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this TypesFromDllTest.ClassWithConstructorFromDll source) => new Test.Destination
        (
        )
        {
            PropertyA = source.PropertyA,
            PropertyB = source.PropertyB,
            PropertyC = source.PropertyC
        };

    }
}