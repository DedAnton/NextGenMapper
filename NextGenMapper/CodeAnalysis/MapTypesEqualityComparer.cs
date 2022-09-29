using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis;

public class MapTypesEqualityComparer : IEqualityComparer<(ITypeSymbol From, ITypeSymbol To)>
{
    public bool Equals((ITypeSymbol From, ITypeSymbol To) x, (ITypeSymbol From, ITypeSymbol To) y)
        => SymbolEqualityComparer.IncludeNullability.Equals(x.From, y.From)
        && SymbolEqualityComparer.IncludeNullability.Equals(x.To, y.To);

    public int GetHashCode((ITypeSymbol From, ITypeSymbol To) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.IncludeNullability.GetHashCode(obj.From);
            hash = hash * 23 + SymbolEqualityComparer.IncludeNullability.GetHashCode(obj.To);
            return hash;
        }
    }
}