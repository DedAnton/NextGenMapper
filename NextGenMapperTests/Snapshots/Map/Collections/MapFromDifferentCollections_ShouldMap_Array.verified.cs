//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static int[] Map<To>(this byte[] source)
        {
            var sourceCollection = new System.Span<byte>(source);
            var length = sourceCollection.Length;
            var destination = new int[length];
            for (var i = 0; i < length; i++)
            {
                destination[i] = sourceCollection[i];
            }

            return destination;
        }
    }
}