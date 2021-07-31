using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassMapDesigner
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MapPlanner _planner;

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
