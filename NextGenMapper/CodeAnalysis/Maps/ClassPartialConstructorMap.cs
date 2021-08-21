using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialConstructorMap : ClassMap
    {
        public string ParameterName { get; }

        public ClassPartialConstructorMap(Type from, Type to, IEnumerable<MemberMap> properties, string parameterName)
            : base(from, to, properties)
        {
            ParameterName = parameterName;
        }
    }
}
