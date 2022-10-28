//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.RecordFromDll Map<To>(this Test.Source source) => new TypesFromDllTest.RecordFromDll
        (
            source.Property
        )
        {
        };

    }
}