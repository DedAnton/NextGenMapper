using System;

namespace NextGenMapper.Extensions
{
    public static class NumericExtensions
    {
        public static long UnboxToLong(this object number)
            => number switch
            {
                sbyte => (sbyte)number,
                byte => (byte)number,
                short => (short)number,
                ushort => (ushort)number,
                int => (int)number,
                uint => (uint)number,
                long => (long)number,
                ulong => (long)number,
                //TODO: maybe need handle this exception and add diagnostic
                _ => throw new ArgumentOutOfRangeException(nameof(number), $"{nameof(number)} must be sbyte, byte, short, ushort, int, uint or long. (ulong is supported but casting to long)")
            };
    }
}
