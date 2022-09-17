using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public abstract class TypeMap
    {
        public ITypeSymbol From { get; }
        public ITypeSymbol To { get; }

        public TypeMap(ITypeSymbol from, ITypeSymbol to)
        {
            From = from;
            To = to;
        }
    }
}
