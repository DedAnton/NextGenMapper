using Microsoft.CodeAnalysis;

namespace NextGenMapper
{
    public class PropertyMapping
    {
        public ITypeSymbol TypeFrom { get; }
        public ITypeSymbol TypeTo { get; }

        public string NameFrom { get; }
        public string NameTo { get; }
        //public string TypeFrom => From.Type.ToDisplayString();
        //public string TypeTo => To.Type.ToDisplayString();
        public bool IsSameTypes => TypeFrom.Equals(TypeTo, SymbolEqualityComparer.IncludeNullability);
        public bool IsParameterMapping { get; }

        public PropertyMapping(IPropertySymbol from, IPropertySymbol to)
        {
            TypeFrom = from.Type;
            TypeTo = to.Type;
            NameFrom = from.Name;
            NameTo = to.Name;
        }

        public PropertyMapping(IPropertySymbol from, IParameterSymbol to)
        {
            TypeFrom = from.Type;
            TypeTo = to.Type;
            NameFrom = from.Name;
            NameTo = to.Name;
            IsParameterMapping = true;
        }
    }
}
