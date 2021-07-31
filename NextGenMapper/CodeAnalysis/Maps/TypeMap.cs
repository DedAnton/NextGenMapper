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

        public override bool Equals(object? obj)
        {
            return obj is TypeMap map && Equals(map.From, map.To);
        }

        public override int GetHashCode()
        {
            int hashCode = -1781160927;
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.IncludeNullability.GetHashCode(From);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.IncludeNullability.GetHashCode(To);
            return hashCode;
        }

        public bool Equals(ITypeSymbol from, ITypeSymbol to)
            => From.Equals(from, SymbolEqualityComparer.IncludeNullability)
            && To.Equals(to, SymbolEqualityComparer.IncludeNullability);
    }
}
