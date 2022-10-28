//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this TypesFromDllTest.RecordFromDll source) => new Test.Destination
        (
            source.Property
        )
        {
        };

    }
}