using Microsoft.CodeAnalysis;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;

namespace NextGenMapper.Mapping.Designers;

internal static class DesignersHelper
{
    public static Dictionary<string, IPropertySymbol> GetPublicReadablePropertiesDictionary(this ITypeSymbol type)
    {
        var members = type.GetMembers().AsSpan();
        var properties = new Dictionary<string, IPropertySymbol>(members.Length, StringComparer.InvariantCulture);
        foreach (var member in members)
        {
            if (member is IPropertySymbol
                {
                    CanBeReferencedByName: true,
                    IsWriteOnly: false,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
                    GetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                } property)
            {
                properties.Add(property.Name, property);
            }
        }

        return properties;
    }

    public static ReadOnlySpan<IPropertySymbol> GetPublicWritableProperties(
        this ITypeSymbol type)
    {
        var members = type.GetMembers().AsSpan();
        var properties = new ValueListBuilder<IPropertySymbol>(members.Length);
        foreach (var member in members)
        {
            if (member is IPropertySymbol
                {
                    CanBeReferencedByName: true,
                    IsReadOnly: false,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
                    SetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                } property)
            {
                properties.Append(property);
            }
        }

        return properties.AsSpan();
    }

    public static bool IsPotentialNullReference(ITypeSymbol source, ITypeSymbol destination)
        => (source.NullableAnnotation, destination.NullableAnnotation) switch
        {
            (NullableAnnotation.NotAnnotated or NullableAnnotation.None, _) => false,
            (NullableAnnotation.Annotated, NullableAnnotation.Annotated) => false,
            _ => true
        };
}
