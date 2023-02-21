using Microsoft.CodeAnalysis;
using System;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static ReadOnlySpan<IMethodSymbol> GetConstructors(this ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedType)
            {
                return Array.Empty<IMethodSymbol>();
            }

            return namedType.Constructors.AsSpan();
        }

        public static SyntaxNode? GetFirstDeclaration(this ISymbol symbol)
        {
            if (symbol.DeclaringSyntaxReferences.Length > 0)
            {
                return symbol.DeclaringSyntaxReferences[0].GetSyntax();
            }

            return null;
        }

        public static string ToNotNullableString(this ITypeSymbol type) => type.ToDisplayString(NullableFlowState.None);
    }
}
