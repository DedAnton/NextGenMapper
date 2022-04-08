using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ConstructorFinder
    {
        private readonly ConstructorComparer _constructorComparer = new();

        public IMethodSymbol? GetOptimalConstructor(ITypeSymbol from, ITypeSymbol to, HashSet<string> byUser)
        {
            var constructors = to.GetPublicConstructors();
            BubbleSort.SortSpan(ref constructors, _constructorComparer);
            if (constructors.Length == 0)
            {
                return null;
            }

            var fromPropertiesNames = from.GetPropertiesNames().ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);
            var flattenPropertiesNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            Span<char> flattenPropertyName = stackalloc char[128];
            foreach (var property in from.GetProperties())
            {
                foreach (var nestedProperty in property.Type.GetProperties())
                {
                    property.Name.AsSpan().CopyTo(flattenPropertyName);
                    nestedProperty.Name.AsSpan().CopyTo(flattenPropertyName.Slice(property.Name.Length));
                    //flattenPropertyName.ToString();
                    //flattenPropertiesNames.Add($"{property.Name}{flattenProperty.Name}");

                    flattenPropertiesNames.Add(flattenPropertyName.Slice(0, property.Name.Length + nestedProperty.Name.Length).ToString());
                }
            }

            bool ValidateCommonCostructor(IMethodSymbol constructor)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    if (!byUser.Contains(parameter.Name)
                        && !fromPropertiesNames.Contains(parameter.Name)
                        && !flattenPropertiesNames.Contains(parameter.Name))
                    {
                        return false;
                    }
                }

                return true;
            }

            IMethodSymbol? commonConstructor = null;
            foreach (var constructor in constructors)
            {
                if (ValidateCommonCostructor(constructor))
                {
                    commonConstructor = constructor;
                    break;
                }
            }

            bool ValidateUnflattenCostructor(IMethodSymbol constructor)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    if (GetOptimalUnflatteningConstructor(from, parameter.Type, parameter.Name) == null
                        && !byUser.Contains(parameter.Name)
                        && !fromPropertiesNames.Contains(parameter.Name))
                    {
                        return false;
                    }
                }

                return true;
            }

            IMethodSymbol? unflattenConstructor = null;
            foreach (var constructor in constructors)
            {
                if (ValidateUnflattenCostructor(constructor))
                {
                    unflattenConstructor = constructor;
                    break;
                }
            }

            return GetParametersCount(commonConstructor) > GetParametersCount(unflattenConstructor)
                ? commonConstructor
                : unflattenConstructor;
        }

        public IMethodSymbol? GetOptimalUnflatteningConstructor(ITypeSymbol from, ITypeSymbol to, string unflattingPropertyName)
        {
            if (to.SpecialType != SpecialType.None)
            {
                return null;
            }
            var constructors = to.GetPublicConstructors();
            BubbleSort.SortSpan(ref constructors, _constructorComparer);
            if (constructors.Length == 0)
            {
                return null;
            }

            var fromPropertiesNames = from.GetPropertiesNames().ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);

            bool ValidateCommonCostructor(IMethodSymbol constructor)
            {
                Span<char> name = stackalloc char[128];
                foreach (var parameter in constructor.Parameters)
                {
                    unflattingPropertyName.AsSpan().CopyTo(name);
                    parameter.Name.AsSpan().CopyTo(name.Slice(unflattingPropertyName.Length));
                    if (!fromPropertiesNames.Contains(name.Slice(0, unflattingPropertyName.Length+parameter.Name.Length).ToString()))
                    {
                        return false;
                    }
                }

                return true;
            }

            IMethodSymbol? commonConstructor = null;
            foreach (var constructor in constructors)
            {
                if (ValidateCommonCostructor(constructor))
                {
                    commonConstructor = constructor;
                    break;
                }
            }

            Span<char> name = stackalloc char[128];
            unflattingPropertyName.AsSpan().CopyTo(name);
            var flattenProperties = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(var property in to.GetProperties())
            {
                property.Name.AsSpan().CopyTo(name.Slice(unflattingPropertyName.Length));
                flattenProperties.Add(name.Slice(0, unflattingPropertyName.Length + property.Name.Length).ToString());
            }

            foreach(var property in from.GetProperties())
            {
                if (flattenProperties.Contains(property.Name))
                {
                    return commonConstructor;
                }
            }

            return null;
        }

        private int GetParametersCount(IMethodSymbol? method) => method?.Parameters.Length ?? -1;

        class ConstructorComparer : IComparer<IMethodSymbol>
        {
            public int Compare(IMethodSymbol? x, IMethodSymbol? y) => x!.Parameters.Length.CompareTo(y!.Parameters.Length) * -1;
        }
    }
}
