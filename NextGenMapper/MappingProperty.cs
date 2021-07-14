using Microsoft.CodeAnalysis;

namespace NextGenMapper
{
    public class MappingProperty
    {
        public string NameFrom { get; }
        public string NameTo { get; }
        public string TypeFrom { get; }
        public string TypeTo { get; }
        public bool IsSameTypes { get; }

        public MappingProperty(IPropertySymbol from, IPropertySymbol to)
        {
            NameFrom = from.Name;
            NameTo = to.Name;
            TypeFrom = from.Type.ToDisplayString();
            TypeTo = to.Type.ToDisplayString();
            IsSameTypes = from.Type.Equals(to.Type, SymbolEqualityComparer.IncludeNullability);
        }
    }
}
