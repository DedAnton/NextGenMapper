//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.ClassWithConstructorFromDll MapWith<To>
        (
            this Test.Source source,
            int pROpeRTyc
        )
        => new TypesFromDllTest.ClassWithConstructorFromDll
        (
            source.PropertyA,
            source.PropertyB,
            pROpeRTyc
        );
    }
}