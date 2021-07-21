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
        public bool IsProvidedByUser { get; }

        public PropertyMapping(IPropertySymbol from, IPropertySymbol to, bool isProvidedByUser = false)
        {
            TypeFrom = from.Type;
            TypeTo = to.Type;
            NameFrom = from.Name;
            NameTo = to.Name;
            IsProvidedByUser = isProvidedByUser;
        }

        public PropertyMapping(IPropertySymbol from, IParameterSymbol to, bool isProvidedByUser = false)
        {
            TypeFrom = from.Type;
            TypeTo = to.Type;
            NameFrom = from.Name;
            NameTo = to.Name;
            IsProvidedByUser = isProvidedByUser;
            IsParameterMapping = true;
        }
    }
}
