//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.EnumFromDll Map<To>(this Test.Source source) => source switch
        {
            Test.Source.A => TypesFromDllTest.EnumFromDll.A,
            Test.Source.B => TypesFromDllTest.EnumFromDll.B,
            Test.Source.C => TypesFromDllTest.EnumFromDll.C,
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), "Error when mapping Test.Source to Test.Destination")
        };
    }
}