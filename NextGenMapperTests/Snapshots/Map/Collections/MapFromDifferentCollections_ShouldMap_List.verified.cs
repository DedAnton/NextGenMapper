//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this System.Collections.Generic.List<int> source)
        {
            #if NET5_0_OR_GREATER
            var sourceCollection = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);
            var length = sourceCollection.Length;
            #else
            var sourceCollection = source;
            var length = sourceCollection.Count;
            #endif
            var destination = new int[length];
            for (var i = 0; i < length; i++)
            {
                destination[i] = sourceCollection[i];
            }

            return destination;
        }
    }
}