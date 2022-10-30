//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination<int> MapWith<To>
        (
            this Test.Source<int> source,
            int forMapWith
        )
        => new Test.Destination<int>
        (
        )
        {
            Property = source.Property,
            ForMapWith = forMapWith
        };

        internal static Test.Destination<int> MapWith<To>
        (
            this Test.Source<int> source,
            int property = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}