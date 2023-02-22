//HintName: Mapper_ConfiguredMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this TypesFromDllTest.ClassWithConstructorFromDll source,
            int ForMapWith
        )
        => new Test.Destination
        {
            PropertyA = source.PropertyA,
            PropertyB = source.PropertyB,
            PropertyC = source.PropertyC,
            ForMapWith = ForMapWith
        };
    }
}