//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination()
        {
            Property = source.Property
        };

        internal static System.Collections.Generic.List<Test.Destination> Map<To>(this System.Collections.Generic.IEnumerable<Test.Source> source)
        {
            if (!source.TryGetSpan(out var sourceCollection))
            {
                sourceCollection = System.Linq.Enumerable.ToArray(source);
            }
            var length = sourceCollection.Length;
            var destination = new System.Collections.Generic.List<Test.Destination>(length);
            for (var i = 0; i < length; i++)
            {
                destination.Add(sourceCollection[i].Map<Test.Destination>());
            }

            return destination;
        }
    }
}