//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.ClassFromDll Map<To>(this Test.Source source) => new TypesFromDllTest.ClassFromDll()
        {
            PropertyA = source.PropertyA
        };
    }
}