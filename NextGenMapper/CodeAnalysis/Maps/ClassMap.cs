using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassMap : TypeMap
    {
        public List<PropertyMap> InitializerProperties { get; }
        public List<ParameterMap> ConstructorProperties { get; }

        public ClassMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<IMemberMap> properties)
            : base(from, to)
        {
            InitializerProperties = properties.OfType<PropertyMap>().ToList();
            ConstructorProperties = properties.OfType<ParameterMap>().ToList();
        }
    }
}
