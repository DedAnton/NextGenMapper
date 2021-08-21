using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class ClassMap : TypeMap
    {
        public List<MemberMap> InitializerProperties { get; }
        public List<MemberMap> ConstructorProperties { get; }
        public bool IsUnflattening { get; }

        public ClassMap(Type from, Type to, IEnumerable<MemberMap> properties, bool isUnflattening = false)
            : base(from, to)
        {
            ConstructorProperties = properties.Where(x => x.Type == MemberMapType.Constructor).ToList();
            InitializerProperties = properties.Where(x => x.Type == MemberMapType.Initializer).ToList();
            IsUnflattening = isUnflattening;
        }
    }
}
