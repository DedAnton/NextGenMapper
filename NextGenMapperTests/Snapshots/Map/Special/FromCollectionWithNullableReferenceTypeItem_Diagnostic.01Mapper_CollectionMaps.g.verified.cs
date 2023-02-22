//HintName: Mapper_CollectionMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static System.Collections.Generic.List<Test.Destination> Map<To>(this System.Collections.Generic.List<Test.Source?> source)
        {
            #if NET5_0_OR_GREATER
            var sourceCollection = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);
            var length = sourceCollection.Length;
            #else
            var sourceCollection = source;
            var length = sourceCollection.Count;
            #endif
            var destination = new System.Collections.Generic.List<Test.Destination>(length);
            for (var i = 0; i < length; i++)
            {
                destination.Add(sourceCollection[i].Map<Test.Destination>());
            }

            return destination;
        }
    }
}