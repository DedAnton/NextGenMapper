using System;

namespace NextGenMapper.CodeSamples
{
    public static partial class Mapper
    {
        public static TTo Map<TTo>(this object source) => throw new InvalidOperationException();
    }
}
