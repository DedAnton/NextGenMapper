using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static T? As<T>(this SyntaxNode node) where T : SyntaxNode => node is T tNode ? tNode : default;

        public static bool IsGenericEnumerable(this ITypeSymbol type)
        {
            if (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return true;
            }

            foreach (var @interface in type.AllInterfaces)
            {
                if (@interface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                {
                    return true;
                }
            }

            return false;
        }

        //public static EnumField[] GetFields(this EnumDeclarationSyntax enumDeclaration)
        //{
        //    var fields = new EnumField[enumDeclaration.Members.Count];
        //    for (int i = 0; i < enumDeclaration.Members.Count; i++)
        //    {
        //        fields[i] = new EnumField(
        //            enumDeclaration.Members[i].Identifier.ValueText,
        //            enumDeclaration.Members[i].EqualsValue?.Value?.As<LiteralExpressionSyntax>()?.Token.Value?.UnboxToLong());
        //    }

        //    return fields;
        //}

        public static Span<IMethodSymbol> GetPublicConstructors(this ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedTypeSymbol)
            {
                return Array.Empty<IMethodSymbol>();
            }

            Span<IMethodSymbol> publicConstructors = new IMethodSymbol[namedTypeSymbol.Constructors.Length];
            var count = 0;
            foreach (var constructor in namedTypeSymbol.Constructors)
            {
                if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
                {
                    publicConstructors[count] = constructor;
                    count++;
                }
            }

            return publicConstructors.Slice(0, count);
        }

        public static Span<string> GetParametersNames(this IMethodSymbol method)
        {
            Span<string> names = new string[method.Parameters.Length];
            for (int i = 0; i < method.Parameters.Length; i++)
            {
                names[i] = method.Parameters[i].Name;
            }

            return names;
        }

        public static Span<IPropertySymbol> GetPublicProperties(this ITypeSymbol type)
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

        public static Dictionary<string, IPropertySymbol> GetPublicPropertiesDictionary(this ITypeSymbol type)
        {
            var members = type.GetMembers().AsSpan();
            var properties = new Dictionary<string, IPropertySymbol>(members.Length, StringComparer.InvariantCulture);
            foreach (var member in members)
            {
                if (member is IPropertySymbol
                    {
                        CanBeReferencedByName: true,
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                    } property)
                {
                    properties.Add(property.Name, property);
                }
            }

            return properties;
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

        public static IPropertySymbol? FindPublicProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCulture)
        {
            foreach (var property in type.GetPublicProperties())
            {
                if (property is
                    {
                        IsWriteOnly: false,
                        GetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                    } 
                    && property.Name.Equals(name, comparision))
                {
                    return property;
                }
            }

            return null;
        }

        public static SyntaxNode? GetFirstDeclaration(this ISymbol symbol)
        {
            if (symbol.DeclaringSyntaxReferences.Length > 0)
            {
                return symbol.DeclaringSyntaxReferences[0].GetSyntax();
            }

            return null;
        }

        public static bool IsPrimitive(this ITypeSymbol type) => (sbyte)type.SpecialType >= 7 && (sbyte)type.SpecialType <= 20;

        public static string ToNotNullableString(this ITypeSymbol type) => type.ToDisplayString(NullableFlowState.None);
    }
}
