//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination
        (
        )
        {
            Property = source.Property
        };

        internal static System.Collections.Generic.List<Test.Destination?> Map<To>(this System.Collections.Generic.List<Test.Source> source)
        {
            
            var destination = new System.Collections.Generic.List<Test.Destination?>(source.Count);
            #if NET5_0_OR_GREATER
            var sourceSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);
            #endif
            for (var i = 0; i < source.Count; i++)
            {
                #if NET5_0_OR_GREATER
                destination.Add(sourceSpan[i].Map<Test.Destination?>());
                #else
                destination.Add(source[i].Map<Test.Destination?>());
                #endif
            }

            return destination;
        }

    }
}