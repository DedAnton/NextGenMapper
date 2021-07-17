using Microsoft.CodeAnalysis;

namespace NextGenMapper
{
    public class PropertyMapping
    {
        public IPropertySymbol From { get; }
        public IPropertySymbol To { get; }

        public string NameFrom => From.Name;
        public string NameTo => To.Name;
        public string TypeFrom => From.Type.ToDisplayString();
        public string TypeTo => To.Type.ToDisplayString();
        public bool IsSameTypes => From.Type.Equals(To.Type, SymbolEqualityComparer.IncludeNullability);

        public PropertyMapping(IPropertySymbol from, IPropertySymbol to)
        {
            From = from;
            To = to;
        }
    }
}
