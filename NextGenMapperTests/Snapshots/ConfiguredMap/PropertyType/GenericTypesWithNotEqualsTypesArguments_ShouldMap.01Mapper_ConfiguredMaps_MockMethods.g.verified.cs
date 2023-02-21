//HintName: Mapper_ConfiguredMaps_MockMethods.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<Test.ClassB> MapWith<To>
        (
            this Test.Source<Test.ClassA> source,
            Test.ClassB Property = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}