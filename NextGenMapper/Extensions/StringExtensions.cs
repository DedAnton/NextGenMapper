using System;
using System.Text;

namespace NextGenMapper.Extensions
{
    public static class StringExtensions
    {
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
    }
}
