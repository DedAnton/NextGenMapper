﻿using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;

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

            var fromPropertiesNames = new HashSet<string>(from.GetPublicPropertiesNames().ToArray(), StringComparer.InvariantCultureIgnoreCase);

            bool ValidateCommonCostructor(IMethodSymbol constructor)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    if (!byUser.Contains(parameter.Name)
                        && !fromPropertiesNames.Contains(parameter.Name))
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

            return commonConstructor;
        }

        private class ConstructorComparer : IComparer<IMethodSymbol>
        {
            public int Compare(IMethodSymbol? x, IMethodSymbol? y) => x!.Parameters.Length.CompareTo(y!.Parameters.Length) * -1;
        }
    }
}
