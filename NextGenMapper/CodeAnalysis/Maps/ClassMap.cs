using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class ClassMap : TypeMap
    {
        public List<MemberMap> InitializerProperties { get; } = new List<MemberMap>();
        public List<MemberMap> ConstructorProperties { get; } = new List<MemberMap>();
        public bool IsUnflattening { get; }

        public ClassMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, bool isUnflattening = false)
            : base(from, to)
        {
            foreach (var property in properties)
            {
                if (property.MapType is MemberMapType.Initializer or MemberMapType.UnflattenInitializer)
                {
                    InitializerProperties.Add(property);
                }
                if (property.MapType is MemberMapType.Constructor or MemberMapType.UnflattenConstructor)
                {
                    ConstructorProperties.Add(property);
                }
            }

            IsUnflattening = isUnflattening;
        }
    }
}
