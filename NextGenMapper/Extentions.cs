using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    public static class Extentions
    {
        public static bool HasAttribute(this ISymbol symbol, string attributeFullName)
            => symbol.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == attributeFullName);

        public static ISymbol GetSymbol(this GeneratorSyntaxContext context, SyntaxNode node)
            => context.SemanticModel.GetDeclaredSymbol(node);

        public static SymbolInfo GetSymbolInfo(this GeneratorSyntaxContext context, SyntaxNode node)
            => context.SemanticModel.GetSymbolInfo(node);

        public static string LeadingSpace(this string source, int addLeadingSpacesCount)
        {
            var splittedLines = source.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var strBuilder = new StringBuilder();
            foreach(var line in splittedLines)
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
                    else if (removeCount > line.Length)
                    {
                        strBuilder.AppendLine(line.TrimStart());
                    }
                    else
                    {
                        strBuilder.AppendLine(line.Trim());
                    }
                }
            }

            return strBuilder.ToString();
        }
    }
}
