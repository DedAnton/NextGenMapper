using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public static class MapDesignersHelper
    {
        public static bool IsEnumMapping(ITypeSymbol from, ITypeSymbol to)
            => from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum;

        public static bool IsCollectionMapping(ITypeSymbol from, ITypeSymbol to)
            => from.IsGenericEnumerable() && to.IsGenericEnumerable();

        public static List<ISymbol> GetPropertiesInitializedByConstructorAndInitializer(this IMethodSymbol constructor)
        {
            var constructorParametersNames = new HashSet<string>(constructor.GetParametersNames().ToArray(), StringComparer.InvariantCultureIgnoreCase);
            var members = new List<ISymbol>();
            foreach (var parameter in constructor.Parameters)
            {
                members.Add(parameter);
            }
            foreach (var constructorTypeProperty in constructor.ContainingType.GetPublicProperties())
            {
                if (!constructorTypeProperty.IsReadOnly && !constructorParametersNames.Contains(constructorTypeProperty.Name))
                {
                    members.Add(constructorTypeProperty);
                }
            }

            return members;
        }
    }
}
