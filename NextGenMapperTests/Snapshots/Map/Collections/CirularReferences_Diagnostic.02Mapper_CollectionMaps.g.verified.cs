//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination[] Map<To>(this Test.Source[] source)
        {
            var sourceCollection = new System.Span<Test.Source>(source);
            var length = sourceCollection.Length;
            var destination = new Test.Destination[length];
            for (var i = 0; i < length; i++)
            {
                destination[i] = sourceCollection[i].Map<Test.Destination>();
            }

            return destination;
        }
    }
}