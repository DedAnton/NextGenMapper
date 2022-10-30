using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis;

public class MapTypesEqualityComparer : IEqualityComparer<(ITypeSymbol From, ITypeSymbol To)>
{
    public bool Equals((ITypeSymbol From, ITypeSymbol To) x, (ITypeSymbol From, ITypeSymbol To) y)
        => SymbolEqualityComparer.Default.Equals(x.From, y.From)
        && SymbolEqualityComparer.Default.Equals(x.To, y.To);

    public int GetHashCode((ITypeSymbol From, ITypeSymbol To) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.From);
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.To);
            return hash;
        }
    }
}