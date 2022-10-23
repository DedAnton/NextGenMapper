//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this System.Collections.Generic.List<int> source)
        {
            
            var destination = new int[source.Count];
            #if NET5_0_OR_GREATER
            var sourceSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);
            #endif
            for (var i = 0; i < source.Count; i++)
            {
                #if NET5_0_OR_GREATER
                destination[i] = sourceSpan[i];
                #else
                destination[i] = source[i];
                #endif
            }

            return destination;
        }

    }
}