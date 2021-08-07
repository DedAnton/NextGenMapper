using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassMapDesigner
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MapPlanner _planner;

        private List<(ITypeSymbol from, ITypeSymbol to)> _referencesHistory = new();

        public ClassMapDesigner(SemanticModel semanticModel, MapPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            if (from.IsPrivitive() || to.IsPrivitive()
                || !(from.GetOptimalConstructor(to) is var constructor and { }))
            {
                return;
            }
            if (_referencesHistory.Contains((from, to), new ReferencesEqualityComparer()))
            {
                //add diagnostics
                throw new ArgumentException("Circular reference was found." + string.Join(" => ", _referencesHistory.Select(x => $"{x.from} to {x.to}")));
            }
            _referencesHistory.Add((from, to));

            var membersMaps = new List<IMemberMap>();
            foreach (var fromProperty in from.GetProperties())
            {
                var toConstructor = constructor.FindParameter(fromProperty.Name);
                var toInitializer = to.FindSettableProperty(fromProperty.Name);

                IMemberMap? map = (toConstructor, toInitializer, UseInitializer) switch
                {
                    ({ }, _, _) => new ParameterMap(fromProperty, toConstructor),
                    (_, { }, true) => new PropertyMap(fromProperty, toInitializer),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);

                if (map is { IsSameTypes: false })
                {
                    DesignMapsForPlanner(map.TypeFrom, map.TypeTo);
                }
            }

            _planner.AddCommonMap(new ClassMap(from, to, membersMaps));
        }
    }
}
