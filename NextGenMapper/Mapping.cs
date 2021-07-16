using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper
{
    public class Mapping
    {
        public ITypeSymbol TypeFrom { get; }
        public ITypeSymbol TypeTo { get; }
        public List<MappingProperty> Properties { get; set; } = new List<MappingProperty>();

        public string From => TypeFrom.ToDisplayString();
        public string To => TypeTo.ToDisplayString();

        public Mapping(ITypeSymbol from, ITypeSymbol to)
        {
            TypeFrom = from;
            TypeTo = to;
        }

        public override bool Equals(object obj)
        {
            return obj is Mapping mapping &&
                TypeFrom.Equals(mapping.TypeFrom, SymbolEqualityComparer.IncludeNullability) &&
                TypeTo.Equals(mapping.TypeTo, SymbolEqualityComparer.IncludeNullability);
        }

        public override int GetHashCode()
        {
            int hashCode = -1308899859;
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(TypeFrom);
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(TypeTo);
            return hashCode;
        }
    }
}
