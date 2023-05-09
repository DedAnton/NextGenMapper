//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination?[] Map<To>(this Source?[] source)
        {
            var sourceCollection = new System.Span<Source?>(source);
            var length = sourceCollection.Length;
            var destination = new Destination?[length];
            for (var i = 0; i < length; i++)
            {
                destination[i] = sourceCollection[i]?.Map<Destination?>();
            }

            return destination;
        }
    }
}