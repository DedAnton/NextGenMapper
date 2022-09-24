using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string source)
        {
            var sourceSpan = source.AsSpan();
            Span<char> resultSpan = new char[sourceSpan.Length];
            sourceSpan.CopyTo(resultSpan);
            resultSpan[0] = char.ToLowerInvariant(sourceSpan[0]);
            return resultSpan.ToString();
        }
    }
}
