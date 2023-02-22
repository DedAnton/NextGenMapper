using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.Mapping.Comparers;

internal class ConstructorsComparer : IComparer<IMethodSymbol>
{
    public int Compare(IMethodSymbol x, IMethodSymbol y) => x.Parameters.Length.CompareTo(y.Parameters.Length) * -1;
}