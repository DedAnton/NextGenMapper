using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class FieldMap : IMemberMap
    {
        public IFieldSymbol From { get; }
        public IFieldSymbol To { get; }

        public FieldMap(IFieldSymbol from, IFieldSymbol to)
        {
            From = from;
            To = to;
        }

        public ITypeSymbol TypeFrom => From.Type;
        public ITypeSymbol TypeTo => To.Type;
        public string NameFrom => From.Name;
        public string NameTo => To.Name;
        public bool IsSameTypes => true;
        public bool IsProvidedByUser => false;
    }
}
