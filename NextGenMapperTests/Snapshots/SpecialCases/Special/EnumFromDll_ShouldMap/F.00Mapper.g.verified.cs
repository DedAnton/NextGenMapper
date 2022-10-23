//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this EnumFromDllTest.EnumFromDll source) => source switch
        {
            EnumFromDllTest.EnumFromDll.A => Test.Destination.A,
            EnumFromDllTest.EnumFromDll.B => Test.Destination.B,
            EnumFromDllTest.EnumFromDll.C => Test.Destination.C,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping EnumFromDllTest.EnumFromDll to Test.Destination")
        };

    }
}