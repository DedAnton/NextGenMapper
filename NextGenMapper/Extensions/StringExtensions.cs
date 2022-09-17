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

        public static string LeadingSpace(this string source, int addLeadingSpacesCount)
        {
            var splittedLines = source.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var strBuilder = new StringBuilder();
            foreach (var line in splittedLines)
            {
                if (addLeadingSpacesCount > 0)
                {
                    strBuilder.AppendLine(line.PadLeft(line.Length + addLeadingSpacesCount));
                }
                else
                {
                    var removeCount = Math.Abs(addLeadingSpacesCount);
                    if (removeCount < line.Length)
                    {
                        var trimmed = line.Substring(0, removeCount).TrimStart();
                        strBuilder.AppendLine(trimmed + line.Substring(removeCount));
                    }
                    else
                    {
                        strBuilder.AppendLine(line.TrimStart());
                    }
                }
            }

            return strBuilder.ToString().TrimEnd('\n').TrimEnd('\r');
        }

        public static string RemoveLeadingSpace(this string source, int count) => source.LeadingSpace(count * -1);

        public static string Join(this IEnumerable<string> strings, int indent = 1, string separator = "\r\n") => string.Join(separator.LeadingSpace(4 * indent), strings);

        public static string InterpolateAndJoin<T>(this IEnumerable<T> objects, Func<T, string> pattern, int intend = 1, string separator = "\r\n") => string.Join(separator, objects.Select(pattern)).LeadingSpace(intend * 4);

        public static string TernarInterpolateAndJoin<T>(this IEnumerable<T> objects, Func<T, bool> condition, Func<T, string> truePattern, Func<T, string> falsePattern, int intend = 1, string separator = "\r\n")
            => string.Join(separator, objects.Select(x => condition(x) ? truePattern(x) : falsePattern(x))).LeadingSpace(intend * 4);

        public static string TwoTernarInterpolateAndJoin<T>(
            this IEnumerable<T> objects,
            Func<T, bool> conditionOne,
            Func<T, bool> conditionTwo,
            Func<T, string> patternOne,
            Func<T, string> patternTwo,
            Func<T, string> defaultPattern,
            int intend = 1,
            string separator = "\r\n")
            => string.Join(separator, objects.Select(x => conditionOne(x) ? patternOne(x) : conditionTwo(x) ? patternTwo(x) : defaultPattern(x))).LeadingSpace(intend * 4);

        public static string RemoveDots(this string str) => str.Replace(".", string.Empty);
    }
}
