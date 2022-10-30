//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.DestinationA MapWith<To>
        (
            this Test.SourceA source,
            int forMapWith
        )
        => new Test.DestinationA
        (
        )
        {
            Reference = source.Reference.Map<Test.DestinationB>(),
            ForMapWith = forMapWith
        };

        internal static Test.DestinationA MapWith<To>
        (
            this Test.SourceA source,
            Test.DestinationB reference = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}