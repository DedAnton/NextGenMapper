using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis
{
    public class MapPlanner
    {
        private readonly HashSet<string> _commonGroupUsings = new() { "using NextGenMapper.Extensions;" };
        private readonly HashSet<TypeMap> _typeMaps = new();
        private readonly HashSet<TypeMap> _customTypeMap = new();
        private readonly HashSet<(ITypeSymbol from, ITypeSymbol to)> _allMapsTypes = new(new ReferencesEqualityComparer());

        private MapGroup? commonMapGroup;

        public List<MapGroup> MapGroups { get; } = new();


        public void AddCommonMap(TypeMap map)
        {
            if (_typeMaps.Contains(map))
            {
                //add diagnostic
                return;
            }

            if (commonMapGroup != null)
            {
                commonMapGroup.Add(map);
            }
            else
            {
                commonMapGroup = new MapGroup(map, _commonGroupUsings, MapPriority.Common);
                MapGroups.Add(commonMapGroup);
            }

            _typeMaps.Add(map);
            _allMapsTypes.Add((map.From, map.To));
        }

        public void AddCustomMap(TypeMap map, HashSet<string> usings)
        {
            if (_customTypeMap.Contains(map))
            {
                //add diagnostic
                return;
            }

            MapGroup? customGroup = null;
            foreach (var group in MapGroups)
            {
                if (group.Priority == MapPriority.Custom
                    && group.Usings.SetEquals(usings))
                {
                    customGroup = group;
                }
            }
            if (customGroup is not null)
            {
                customGroup.Add(map);
            }
            else
            {
                MapGroups.Add(new MapGroup(map, usings, MapPriority.Custom));
            }
            commonMapGroup?.Remove(map);

            _typeMaps.Add(map);
            _customTypeMap.Add(map);
            _allMapsTypes.Add((map.From, map.To));
        }

        public bool IsTypesMapAlreadyPlanned(ITypeSymbol from, ITypeSymbol to) => _allMapsTypes.Contains((from, to));
    }
}
