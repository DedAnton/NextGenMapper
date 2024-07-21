using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

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

        public static SyntaxNode? GetFirstDeclarationSyntax(this ISymbol symbol)
        {
            if (symbol.Locations.FirstOrDefault() is Location location
                && location.SourceTree is not null)
            {
                return location.SourceTree.GetCompilationUnitRoot().FindNode(location.SourceSpan);
            }

            return null;
        }

        public static string ToNotNullableString(this ITypeSymbol type) => type.ToDisplayString(NullableFlowState.None);
    }
}
