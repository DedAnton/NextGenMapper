using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public interface IMemberMap
    {
        public ITypeSymbol TypeFrom { get; }
        public ITypeSymbol TypeTo { get; }
        public string NameFrom { get; }
        public string NameTo { get; }
        public bool IsSameTypes { get; }
        public bool IsProvidedByUser { get; }
    }
}
