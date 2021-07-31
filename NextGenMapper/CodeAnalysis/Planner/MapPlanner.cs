using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public class MapPlanner
    {
        public List<MapGroup> MapGroups { get; } = new();

        public void AddCommonMap(TypeMap map)
        {
            if (MapGroups.SelectMany(x => x.Maps).Contains(map))
            {
                //add diagnostic
                return;
            }

            var commonGroup = MapGroups.FirstOrDefault(x => x.Priority == MapPriority.Common);
            if (commonGroup is not null)
            {
                commonGroup.Add(map);
            }
            else
            {
                MapGroups.Add(new MapGroup(map, new(), MapPriority.Common));
            }
        }

        public void AddCustomMap(TypeMap map, List<string> usings)
        {
            if (MapGroups.Where(x => x.Priority == MapPriority.Custom).SelectMany(x => x.Maps).Contains(map))
            {
                //add diagnostic
                return;
            }

            var commonGroup = MapGroups.FirstOrDefault(x => x.Usings.SequenceEqual(usings));
            if (commonGroup is not null)
            {
                commonGroup.Add(map);
            }
            else
            {
                MapGroups.Add(new MapGroup(map, usings, MapPriority.Custom));
            }
            MapGroups.FirstOrDefault(x => x.Priority == MapPriority.Common)?.Remove(map);
        }
    }
}
