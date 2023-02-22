//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this TypesFromDllTest.ClassFromDll source) => new Test.Destination()
        {
            PropertyA = source.PropertyA
        };
    }
}