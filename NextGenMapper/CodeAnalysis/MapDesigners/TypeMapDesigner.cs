using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class TypeMapDesigner
    {
        private readonly HashSet<(ITypeSymbol from, ITypeSymbol to)> _referencesHistory;
        private readonly DiagnosticReporter _diagnosticReporter;
        private readonly ConstructorFinder _constructorFinder;
        private readonly EnumMapDesigner _enumMapDesigner;
        private readonly CollectionMapDesigner _collectionMapDesigner;

        public TypeMapDesigner(DiagnosticReporter diagnosticReporter)
        {
            _referencesHistory = new(new ReferencesEqualityComparer());
            _diagnosticReporter = diagnosticReporter;
            _constructorFinder = new();
            _enumMapDesigner = new(diagnosticReporter);
            _collectionMapDesigner = new(diagnosticReporter, this);
        }

        public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            var mapTypes = new Stack<(ITypeSymbol From, ITypeSymbol To)>();
            mapTypes.Push((from, to));

            var maps = new List<TypeMap>();
            while (mapTypes.Count > 0)
            {
                (from, to) = mapTypes.Pop();

                if (from.IsPrimitive() || to.IsPrimitive())
                {
                    continue;
                }

                if (SymbolEqualityComparer.IncludeNullability.Equals(from, to))
                {
                    continue;
                }

                if (MapDesignersHelper.IsEnumMapping(from, to))
                {
                    maps.Add(_enumMapDesigner.DesignMapsForPlanner(from, to));
                    continue;
                }

                if (MapDesignersHelper.IsCollectionMapping(from, to))
                {
                    maps.AddRange(_collectionMapDesigner.DesignMapsForPlanner(from, to));
                    continue;
                }

                if (_referencesHistory.Contains((from, to)))
                {
                    _diagnosticReporter.ReportCircularReferenceError(to.Locations, _referencesHistory.Select(x => x.from).Append(from));
                    return maps;
                }
                _referencesHistory.Add((from, to));



                var constructor = _constructorFinder.GetOptimalConstructor(from, to, new HashSet<string>());
                if (constructor == null)
                {
                    _diagnosticReporter.ReportConstructorNotFoundError(to.Locations, from, to);
                    continue;
                }

                var membersMaps = new List<MemberMap>();
                var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
                foreach (var member in toMembers)
                {
                    MemberMap? memberMap = member switch
                    {
                        IParameterSymbol parameter => DesignConstructorParameterMap(from, parameter),
                        IPropertySymbol property => DesignInitializerPropertyMap(from, property),
                        _ => null
                    };

                    if (memberMap == null)
                    {
                        continue;
                    }
                    membersMaps.Add(memberMap);

                    mapTypes.Push((memberMap.FromType, memberMap.ToType));
                }

                maps.Add(new ClassMap(from, to, membersMaps));
            }

            return maps;
        }

        public MemberMap? DesignConstructorParameterMap(ITypeSymbol from, IParameterSymbol constructorParameter)
        {
            var fromProperty = from.FindPublicProperty(constructorParameter.Name);
            if (fromProperty != null)
            {
                return MemberMap.Counstructor(fromProperty, constructorParameter);
            }

            return null;
        }

        public MemberMap? DesignInitializerPropertyMap(ITypeSymbol from, IPropertySymbol initializerProperty)
        {
            var fromProperty = from.FindPublicProperty(initializerProperty.Name);
            if (fromProperty != null)
            {
                return MemberMap.Initializer(fromProperty, initializerProperty);
            }

            return null;
        }
    }
}
