using Microsoft.CodeAnalysis;
using System;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static T? As<T>(this SyntaxNode node) where T : SyntaxNode => node is T tNode ? tNode : default;

        public static ReadOnlySpan<IMethodSymbol> GetConstructors(this ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedType)
            {
                return Array.Empty<IMethodSymbol>();
            }

            return namedType.Constructors.AsSpan();
        }

        public static Span<IMethodSymbol> GetPublicConstructors(this ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedTypeSymbol)
            {
                return Array.Empty<IMethodSymbol>();
            }

            Span<IMethodSymbol> publicConstructors = new IMethodSymbol[namedTypeSymbol.Constructors.Length];
            var count = 0;
            foreach (var constructor in namedTypeSymbol.Constructors.AsSpan())
            {
                if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
                {
                    publicConstructors[count] = constructor;
                    count++;
                }
            }

            return publicConstructors.Slice(0, count);
        }

        public static ReadOnlySpan<IPropertySymbol> GetPublicProperties(this ITypeSymbol type)
        {
            var members = type.GetMembers().AsSpan();
            Span<IPropertySymbol> properties = new IPropertySymbol[members.Length];
            var count = 0;
            foreach (var member in members)
            {
                if (member is IPropertySymbol
                {
                    CanBeReferencedByName: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                } property)
                {
                    properties[count] = property;
                    count++;
                }
            }

            return properties.Slice(0, count);
        }

        public static Span<string> GetPublicPropertiesNames(this ITypeSymbol type)
        {
            var properties = type.GetPublicProperties();
            Span<string> names = new string[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                names[i] = properties[i].Name;
            }

            return names;
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
