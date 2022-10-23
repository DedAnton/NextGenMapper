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
        )
        {
            Property1 = source.Property1,
            Property4 = source.Property4,
            Property5 = source.Property5,
            Property7 = source.Property7,
            Property10 = source.Property10,
            Property11 = source.Property11,
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default,
            int property4 = default,
            int property5 = default,
            int property7 = default,
            int property8 = default,
            int property9 = default,
            int property10 = default,
            int property11 = default,
            int property12 = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}