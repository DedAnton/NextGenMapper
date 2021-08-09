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

            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.GetConstructorInitializerMembers();
            foreach (var member in toMembers)
            {
                MemberMap? map = member switch
                {
                    IParameterSymbol parameter => DesignConstructorParameterMap(from, parameter),
                    IPropertySymbol property => DesignInitializerPropertyMap(from, property),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);
                DesignMapForDifferentTypes(map);
            }

            _planner.AddCommonMap(new ClassMap(from, to, membersMaps));
        }

        private MemberMap? DesignConstructorParameterMap(ITypeSymbol from, IParameterSymbol constructorParameter)
        {
            var fromProperty = from.FindProperty(constructorParameter.Name);
            if (fromProperty != null)
            {
                return new MemberMap(fromProperty, constructorParameter);
            }

            var (flattenProperty, mappedProperty) = from.FindFlattenMappedProperty(constructorParameter.Name);
            if (flattenProperty != null && mappedProperty != null)
            {
                return new MemberMap(mappedProperty, constructorParameter, flattenPropertyName: flattenProperty.Name);
            }

            var unflattingClassMap = DesignUnflattingClassMap(from, constructorParameter.Name, constructorParameter.Type);
            if (unflattingClassMap != null)
            {
                _planner.AddCommonMap(unflattingClassMap);
                return new MemberMap(from, constructorParameter);
            }

            return null;
        }

        private MemberMap? DesignInitializerPropertyMap(ITypeSymbol from, IPropertySymbol initializerProperty)
        {
            var fromProperty = from.FindProperty(initializerProperty.Name);
            if (fromProperty != null)
            {
                return new MemberMap(fromProperty, initializerProperty);
            }

            var (flattenProperty, mappedProperty) = from.FindFlattenMappedProperty(initializerProperty.Name);
            if (flattenProperty != null && mappedProperty != null)
            {
                return new MemberMap(mappedProperty, initializerProperty, flattenPropertyName: flattenProperty.Name);
            }

            var unflattingClassMap = DesignUnflattingClassMap(from, initializerProperty.Name, initializerProperty.Type);
            if (unflattingClassMap != null)
            {
                _planner.AddCommonMap(unflattingClassMap);
                return new MemberMap(from, initializerProperty);
            }

            return null;
        }

        private ClassMap? DesignUnflattingClassMap(ITypeSymbol from, string unflattingPropertyName, ITypeSymbol unflattingPropertyType)
        {
            var constructor = from.GetOptimalUnflattingConstructor(unflattingPropertyType, unflattingPropertyName);
            if (constructor == null)
            {
                return null;
            }
            var toMembers = constructor.GetConstructorInitializerMembers();

            var membersMaps = new List<MemberMap>();
            foreach(var member in toMembers)
            {
                var fromProperty = from.FindProperty($"{unflattingPropertyName}{member.Name}");
                MemberMap? map = (fromProperty, member) switch
                {
                    ({ }, IParameterSymbol parameter) => new MemberMap(fromProperty, parameter),
                    ({ }, IPropertySymbol property) => new MemberMap(fromProperty, property),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);
                DesignMapForDifferentTypes(map);
            }
            if (membersMaps.IsEmpty())
            {
                return null;
            }

            return new ClassMap(from, unflattingPropertyType, membersMaps, isUnflattening: true);
        }

        private void DesignMapForDifferentTypes(MemberMap? map)
        {
            if (map is { IsSameTypes: false })
            {
                DesignMapsForPlanner(map.FromType, map.ToType);
            }
        }
    }
}
