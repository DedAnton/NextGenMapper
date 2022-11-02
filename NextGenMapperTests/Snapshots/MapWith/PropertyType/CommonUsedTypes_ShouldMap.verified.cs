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
            int forMapWith
        )
        => new Test.Destination
        {
            Property1 = source.Property1,
            Property2 = source.Property2,
            Property3 = source.Property3,
            Property4 = source.Property4,
            Property5 = source.Property5,
            Property6 = source.Property6,
            Property7 = source.Property7,
            Property8 = source.Property8,
            Property9 = source.Property9,
            Property10 = source.Property10,
            Property11 = source.Property11,
            Property12 = source.Property12,
            Property13 = source.Property13,
            Property14 = source.Property14,
            Property15 = source.Property15,
            Property16 = source.Property16,
            Property17 = source.Property17,
            Property18 = source.Property18,
            Property19 = source.Property19,
            Property20 = source.Property20,
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            byte property1 = default!,
            sbyte property2 = default!,
            short property3 = default!,
            ushort property4 = default!,
            int property5 = default!,
            uint property6 = default!,
            long property7 = default!,
            ulong property8 = default!,
            float property9 = default!,
            double property10 = default!,
            decimal property11 = default!,
            bool property12 = default!,
            char property13 = default!,
            string property14 = default!,
            object property15 = default!,
            System.DateTime property16 = default!,
            System.DateTimeOffset property17 = default!,
            System.DateOnly property18 = default!,
            System.TimeOnly property19 = default!,
            System.TimeSpan property20 = default!,
            int forMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }
    }
}