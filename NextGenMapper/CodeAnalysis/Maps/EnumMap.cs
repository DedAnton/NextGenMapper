using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class EnumMap : TypeMap
    {
        public List<MemberMap> Fields { get; }

        public EnumMap(ITypeSymbol from, ITypeSymbol to, List<MemberMap> fields)
            : base(from, to)
        {
            Fields = fields;
        }
    }
}
