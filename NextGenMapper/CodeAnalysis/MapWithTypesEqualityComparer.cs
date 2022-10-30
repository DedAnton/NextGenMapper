using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis;

public class MapWithTypesEqualityComparer : IEqualityComparer<(ITypeSymbol From, ITypeSymbol To, MapWithInvocationAgrument[] Arguments)>
{
    public bool Equals((ITypeSymbol From, ITypeSymbol To, MapWithInvocationAgrument[] Arguments) x, (ITypeSymbol From, ITypeSymbol To, MapWithInvocationAgrument[] Arguments) y)
    {
        var isEqual = SymbolEqualityComparer.Default.Equals(x.From, y.From)
            && SymbolEqualityComparer.Default.Equals(x.To, y.To)
            && x.Arguments.Length == y.Arguments.Length;

        var spanX = x.Arguments;
        var spanY = y.Arguments;
        for (var i = 0; i < x.Arguments.Length; i++)
        {
            isEqual = isEqual && SymbolEqualityComparer.Default.Equals(spanX[i].Type, spanY[i].Type);
        }

        return isEqual;
    }

    public int GetHashCode((ITypeSymbol From, ITypeSymbol To, MapWithInvocationAgrument[] Arguments) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.From);
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.To);
            foreach (var argument in obj.Arguments)
            {
                hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(argument.Type);
            }
            hash = hash * 23 + obj.Arguments.Length.GetHashCode();

            return hash;
        }
    }
}
