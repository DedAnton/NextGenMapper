using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialConstructorMap : ClassMap
    {
        public string ParameterName { get; }

        public ClassPartialConstructorMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, string parameterName)
            : base(from, to, properties)
        {
            ParameterName = parameterName;
        }
    }
}
