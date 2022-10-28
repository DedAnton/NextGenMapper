//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith
        )
        => new Test.Destination
        (
            source.Property,
            forMapWith
        )
        {
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int uniqueParameterName = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}