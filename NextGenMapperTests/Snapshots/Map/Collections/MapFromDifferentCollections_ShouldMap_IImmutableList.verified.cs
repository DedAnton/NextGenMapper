//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this System.Collections.Immutable.IImmutableList<int> source)
        {
            var sourceCollection = source;
            var length = sourceCollection.Count;
            var destination = new int[length];
            for (var i = 0; i < length; i++)
            {
                destination[i] = sourceCollection[i];
            }

            return destination;
        }
    }
}