//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property9,
            int property10,
            int property11,
            int property12,
            int property13,
            int property14,
            int property15,
            int property16
        )
        => new Test.Destination
        (
            source.Property1,
            source.Property2,
            source.Property3,
            source.Property4,
            source.Property5,
            source.Property6,
            source.Property7,
            source.Property8,
            property9,
            property10,
            property11,
            property12,
            property13,
            property14,
            property15,
            property16
        );

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int property1 = default!,
            int property2 = default!,
            int property3 = default!,
            int property4 = default!,
            int property5 = default!,
            int property6 = default!,
            int property7 = default!,
            int property8 = default!,
            int property9 = default!,
            int property10 = default!,
            int property11 = default!,
            int property12 = default!,
            int property13 = default!,
            int property14 = default!,
            int property15 = default!,
            int property16 = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}