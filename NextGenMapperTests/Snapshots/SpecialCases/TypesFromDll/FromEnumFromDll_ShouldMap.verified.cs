//HintName: Mapper_EnumMaps.g.cs
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
            _ => throw new System.ArgumentOutOfRangeException(nameof(source), "Error when mapping Test.Source to Test.Destination")
        };
    }
}