//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this TypesFromDllTest.EnumFromDll source) => source switch
        {
            TypesFromDllTest.EnumFromDll.A => Test.Destination.A,
            TypesFromDllTest.EnumFromDll.B => Test.Destination.B,
            TypesFromDllTest.EnumFromDll.C => Test.Destination.C,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping TypesFromDllTest.EnumFromDll to Test.Destination")
        };

    }
}