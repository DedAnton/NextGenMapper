using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis
{
    public class ReferencesEqualityComparer : IEqualityComparer<(ITypeSymbol, ITypeSymbol)>
    {
        public bool Equals((ITypeSymbol, ITypeSymbol) x, (ITypeSymbol, ITypeSymbol) y)
            => SymbolEqualityComparer.IncludeNullability.Equals(x.Item1, y.Item1)
            && SymbolEqualityComparer.IncludeNullability.Equals(x.Item2, y.Item2);

        public int GetHashCode((ITypeSymbol, ITypeSymbol) obj)
            => SymbolEqualityComparer.IncludeNullability.GetHashCode(obj.Item1)
            ^ SymbolEqualityComparer.IncludeNullability.GetHashCode(obj.Item2);
    }
}
