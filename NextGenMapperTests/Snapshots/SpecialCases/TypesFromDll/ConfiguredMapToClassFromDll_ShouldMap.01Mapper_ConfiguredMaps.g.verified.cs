//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.ClassFromDll MapWith<To>
        (
            this Test.Source source,
            int PropertyB
        )
        => new TypesFromDllTest.ClassFromDll
        {
            PropertyA = source.PropertyA,
            PropertyB = PropertyB
        };
    }
}