//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.List<int> Map<To>(this int[] source)
        {
            var destination = new System.Collections.Generic.List<int>(source.Length);
            var sourceSpan = new System.Span<int>(source);
            for (var i = 0; i < source.Length; i++)
            {
                destination.Add(sourceSpan[i]);
            }

            return destination;
        }

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            int forMapWith
        )
        => new Test.Destination
        (
        )
        {
            SameProperty = source.SameProperty.Map<System.Collections.Generic.List<int>>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination MapWith<To>
        (
            this Test.Source source,
            System.Collections.Generic.List<int> sameProperty = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}