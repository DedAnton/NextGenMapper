using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class EnumMap : TypeMap
    {
        public List<FieldMap> Fields { get; }

        public EnumMap(ITypeSymbol from, ITypeSymbol to, List<FieldMap> fields)
            : base(from, to)
        {
            Fields = fields;
        }
    }

    public enum EnumMapType
    {
        ByNameAndValue,
        ByName,
        ByValue
    }
}
