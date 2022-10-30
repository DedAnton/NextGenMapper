using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis;

public class MapWithStubsEqualityComparer : IEqualityComparer<(ITypeSymbol From, ITypeSymbol To, ParameterDescriptor[] Parameters)>
{
    public bool Equals((ITypeSymbol From, ITypeSymbol To, ParameterDescriptor[] Parameters) x, (ITypeSymbol From, ITypeSymbol To, ParameterDescriptor[] Parameters) y)
    {
        var isEqual = SymbolEqualityComparer.Default.Equals(x.From, y.From)
            && SymbolEqualityComparer.Default.Equals(x.To, y.To)
            && x.Parameters.Length == y.Parameters.Length;

        var spanX = x.Parameters;
        var spanY = y.Parameters;
        for (var i = 0; i < x.Parameters.Length; i++)
        {
            isEqual = isEqual && SymbolEqualityComparer.Default.Equals(spanX[i].Type, spanY[i].Type);
        }

        return isEqual;
    }

    public int GetHashCode((ITypeSymbol From, ITypeSymbol To, ParameterDescriptor[] Parameters) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.From);
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.To);
            foreach (var parameter in obj.Parameters)
            {
                hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(parameter.Type);
            }
            hash = hash * 23 + obj.Parameters.Length.GetHashCode();

            return hash;
        }
    }
}
