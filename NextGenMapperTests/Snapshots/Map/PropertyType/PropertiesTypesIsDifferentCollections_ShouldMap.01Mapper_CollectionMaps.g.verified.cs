//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.List<int> Map<To>(this int[] source)
        {
            var sourceCollection = new System.Span<int>(source);
            var length = sourceCollection.Length;
            var destination = new System.Collections.Generic.List<int>(length);
            for (var i = 0; i < length; i++)
            {
                destination.Add(sourceCollection[i]);
            }

            return destination;
        }
    }
}