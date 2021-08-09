using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class ClassMap : TypeMap
    {
        public List<MemberMap> InitializerProperties { get; }
        public List<MemberMap> ConstructorProperties { get; }
        public bool IsUnflattening { get; }

        public ClassMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, bool isUnflattening = false)
            : base(from, to)
        {
            InitializerProperties = properties.Where(x => x.MapType is MemberMapType.Initializer or MemberMapType.UnflattenInitializer).ToList();
            ConstructorProperties = properties.Where(x => x.MapType is MemberMapType.Constructor or MemberMapType.UnflattenConstructor).ToList();
            IsUnflattening = isUnflattening;
        }
    }
}
