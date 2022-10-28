//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.ClassWithConstructorFromDll Map<To>(this Test.Source source) => new TypesFromDllTest.ClassWithConstructorFromDll
        (
            source.PropertyA,
            source.PropertyB,
            source.PropertyC
        )
        {
        };

    }
}