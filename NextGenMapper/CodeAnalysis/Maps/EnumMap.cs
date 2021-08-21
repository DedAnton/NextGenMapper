using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class EnumMap : TypeMap
    {
        public List<MemberMap> Fields { get; }

        public EnumMap(Enum from, Enum to, List<MemberMap> fields)
            : base(from, to)
        {
            Fields = fields;
        }
    }
}
