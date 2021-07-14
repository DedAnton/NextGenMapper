using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper
{
    public class Mapping
    {
        protected readonly ITypeSymbol _typeFrom;
        protected readonly ITypeSymbol _typeTo;

        public string TypeFrom { get; }
        public string TypeTo { get; }
        public List<MappingProperty> Properties { get; set; } = new List<MappingProperty>();

        public Mapping(ITypeSymbol from, ITypeSymbol to)
        {
            _typeFrom = from;
            _typeTo = to;

            TypeFrom = _typeFrom.ToDisplayString();
            TypeTo = _typeTo.ToDisplayString();
        }

        public override bool Equals(object obj)
        {
            return obj is Mapping mapping &&
                _typeFrom.Equals(mapping._typeFrom, SymbolEqualityComparer.IncludeNullability) &&
                _typeTo.Equals(mapping._typeTo, SymbolEqualityComparer.IncludeNullability);
        }

        public override int GetHashCode()
        {
            int hashCode = -1308899859;
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(_typeFrom);
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSymbol>.Default.GetHashCode(_typeTo);
            return hashCode;
        }
    }
}
