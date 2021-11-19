using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static T? As<T>(this SyntaxNode node) where T : SyntaxNode => node is T tNode ? tNode : default;

        public static bool IsGenericEnumerable(this ITypeSymbol type) =>
            type.AllInterfaces.Any(x => x.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            || type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T;

        public static List<EnumField> GetFields(this EnumDeclarationSyntax enumDeclaration)
            => enumDeclaration.Members.Select(x => new EnumField(x.Identifier.ValueText, x.EqualsValue?.Value?.As<LiteralExpressionSyntax>()?.Token.Value?.UnboxToLong())).ToList();

        public static bool HasAttribute(this ISymbol? symbol, string attributeFullName)
            => symbol?.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == attributeFullName) ?? false;

        public static IReadOnlyList<IMethodSymbol> GetPublicConstructors(this ITypeSymbol type)
            => (type as INamedTypeSymbol)?.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public).ToList() ?? new();

        public static List<IParameterSymbol> GetParameters(this IMethodSymbol method) => method.Parameters.ToList();

        public static List<string> GetParametersNames(this IMethodSymbol method) => method.Parameters.Select(x => x.Name).ToList();

        public static List<IPropertySymbol> GetProperties(this ITypeSymbol type)
            => type.GetMembers().OfType<IPropertySymbol>()
            .Where(x => x.CanBeReferencedByName && x.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        public static List<string> GetPropertiesNames(this ITypeSymbol type) => type.GetProperties().Select(x => x.Name).ToList();

        public static IPropertySymbol? FindProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetProperties().FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static List<string> GetUsings(this SyntaxNode node)
            => node.Ancestors().OfType<CompilationUnitSyntax>().Single().Usings.Select(x => x.ToString()).ToList();

        public static string GetNamespace(this SyntaxNode node)
            => node.Ancestors().OfType<NamespaceDeclarationSyntax>().Single().Name.ToString();

        public static SyntaxNode? GetFirstDeclaration(this ISymbol symbol)
            => symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        public static bool IsPrimitive(this ITypeSymbol type) => (int)type.SpecialType is int and >= 7 and <= 20;

        public static bool IsDefaultLiteralExpression(this ArgumentSyntax argument)
            => argument.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression;
    }
}
