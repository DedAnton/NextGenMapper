using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners;

public static class MapDesignersHelper
{
    public static bool IsEnumMapping(ITypeSymbol from, ITypeSymbol to)
        => from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum;

    public static bool IsCollectionMapping(ITypeSymbol from, ITypeSymbol to)
        => from.IsGenericEnumerable() && to.IsGenericEnumerable();

    public static bool IsClassMapping(ITypeSymbol from, ITypeSymbol to)
        => from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class;

    public static List<ISymbol> GetPropertiesInitializedByConstructorAndInitializer(this IMethodSymbol constructor, List<Assigment> assigments)
    {
        var propertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assigment in assigments)
        {
            propertiesInitializedByConstructor.Add(assigment.Property);
        }
        var members = new List<ISymbol>();
        foreach (var parameter in constructor.Parameters)
        {
            members.Add(parameter);
        }
        foreach (var property in constructor.ContainingType.GetPublicProperties())
        {
            if (property is
                {
                    IsReadOnly: false,
                    SetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                }
                && !propertiesInitializedByConstructor.Contains(property.Name))
            {
                members.Add(property);
            }
        }

        return members;
    }
}
