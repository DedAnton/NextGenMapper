using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public class MapGroup
    {
        public List<TypeMap> Maps { get; } = new();
        public List<string> Usings { get; } = new();
        public MapPriority Priority { get; }

        public MapGroup(TypeMap map, IEnumerable<string> usings, MapPriority priority) 
        {
            Maps.Add(map);
            Usings = usings.ToList();
            Priority = priority;
        }

        public void Add(TypeMap map) => Maps.Add(map);
        public bool Remove(TypeMap map) => Maps.Remove(map);
    }
}
