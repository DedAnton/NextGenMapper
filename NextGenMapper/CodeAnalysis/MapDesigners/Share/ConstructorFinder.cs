using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ConstructorFinder
    {
        public ConstructorFinder() 
        {

        }

        public IMethodSymbol? GetOptimalConstructor(ITypeSymbol from, ITypeSymbol to, IEnumerable<string>? byUser = null)
        {
            byUser ??= new List<string>();
            var constructors = to.GetPublicConstructors().OrderByDescending(x => x.Parameters.Length);
            if (constructors.IsEmpty())
            {
                return null;
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Complement(byUser)
                .Complement(from.GetPropertiesNames())
                .Complement(GetFlattenPropertiesNames(from))
                .IsEmpty());

            var unflattenConstructor = constructors.FirstOrDefault(x => x
                .Parameters.Where(y => GetOptimalUnflatteningConstructor(from, y.Type, y.Name) == null)
                .Select(x => x.Name)
                .Complement(byUser)
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            return GetParametersCount(constructor) > GetParametersCount(unflattenConstructor)
                ? constructor
                : unflattenConstructor;
        }

        public IMethodSymbol? GetOptimalUnflatteningConstructor(ITypeSymbol from, ITypeSymbol to, string unflattingPropertyName)
        {
            var constructors = to.GetPublicConstructors().OrderByDescending(x => x.Parameters.Length);
            if (constructors.IsEmpty())
            {
                return null;
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Select(y => $"{unflattingPropertyName}{y}")
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            var flattenProperties = to
                .GetPropertiesNames()
                .Select(x => $"{unflattingPropertyName}{x}")
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var isUnflattening = from.GetPropertiesNames().Any(x => flattenProperties.Contains(x));
            if (!isUnflattening)
            {
                return null;
            }

            return constructor;
        }

        private List<string> GetFlattenPropertiesNames(ITypeSymbol type)
            => type.GetProperties().SelectMany(x => x.Type.GetProperties().Select(y => $"{x.Name}{y.Name}")).ToList();

        private int GetParametersCount(IMethodSymbol? method) => method?.Parameters.Length ?? -1;
    }
}
