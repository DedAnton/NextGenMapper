using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public class MapPlanner
    {
        private readonly List<string> _commonGroupUsings = new() { "using NextGenMapper.Extensions;" };
        private readonly HashSet<TypeMap> _typeMaps = new();
        private readonly HashSet<TypeMap> _customTypeMap = new();

        public List<MapGroup> MapGroups { get; } = new();


        public void AddCommonMap(TypeMap map)
        {
            if (_typeMaps.Contains(map))
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
                MapGroups.Add(new MapGroup(map, _commonGroupUsings, MapPriority.Common));
            }
            _typeMaps.Add(map);
        }

        public void AddCustomMap(TypeMap map, List<string> usings)
        {
            if (_customTypeMap.Contains(map))
            {
                //add diagnostic
                return;
            }

            var commonGroup = MapGroups.FirstOrDefault(x => x.Priority == MapPriority.Custom && x.Usings.SequenceEqual(usings));
            if (commonGroup is not null)
            {
                commonGroup.Add(map);
            }
            else
            {
                MapGroups.Add(new MapGroup(map, usings, MapPriority.Custom));
            }
            MapGroups.FirstOrDefault(x => x.Priority == MapPriority.Common)?.Remove(map);
            _typeMaps.Add(map);
            _customTypeMap.Add(map);
        }

        public bool IsTypesMapAlreadyPlanned(ITypeSymbol from, ITypeSymbol to)
            => MapGroups.SelectMany(x => x.Maps)
            .Any(x => x.From.Equals(from, SymbolEqualityComparer.IncludeNullability)
                && x.To.Equals(to, SymbolEqualityComparer.IncludeNullability));
    }
}
