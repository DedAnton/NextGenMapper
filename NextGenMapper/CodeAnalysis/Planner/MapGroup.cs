using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis
{
    public class MapGroup
    {
        public List<TypeMap> Maps { get; } = new();
        public HashSet<string> Usings { get; } = new();
        public MapPriority Priority { get; }

        public MapGroup(TypeMap map, HashSet<string> usings, MapPriority priority)
        {
            Maps.Add(map);
            Usings = usings;
            Priority = priority;
        }

        public void Add(TypeMap map) => Maps.Add(map);
        public bool Remove(TypeMap map) => Maps.Remove(map);
    }
}
