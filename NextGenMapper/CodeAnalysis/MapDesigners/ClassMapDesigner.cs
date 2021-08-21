using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassMapDesigner
    {
        private readonly List<string> _referencesHistory = new();

        public ClassMapDesigner()
        { }

        private string MapHistoryEntry(Type from, Type to) => $"{from}-{to}";

        public ImmutableArray<TypeMap> DesignClassMaps(Type from, Type to)
            => ImmutableArray.CreateRange(DesignMaps(from, to));

        private List<TypeMap> DesignMaps(Type from, Type to)
        {
            var maps = new List<TypeMap>();
            if (from.IsPrimitive || to.IsPrimitive)
            {
                return new();
            }
            if (_referencesHistory.Contains(MapHistoryEntry(from, to), System.StringComparer.InvariantCulture))
            {
                //add diagnostics
                throw new System.ArgumentException("Circular reference was found." + string.Join(" => ", _referencesHistory.Select(x => $"{x}")));
            }
            _referencesHistory.Add(MapHistoryEntry(from, to));

            var constructor = from.GetOptimalConstructor(to, new List<string>());
            if (constructor == null)
            {
                //TODO: diagnostics
                throw new System.ArgumentException($"Error when create mapping from {from} to {to}, {to} does not have a suitable constructor");
            }

            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.Parameters.OfType<IMember>()
                .Concat(constructor.GetInitializerProperties());
            foreach (var toMember in toMembers)
            {
                var memberMap = toMember switch
                {
                    Parameter => DesignMemberMap(from, toMember, MemberMapType.Constructor),
                    Property => DesignMemberMap(from, toMember, MemberMapType.Initializer),
                    _ => null
                };

                if (memberMap == null)
                {
                    //TODO: add diagnostics that not all properties was mapped
                    continue;
                }
                membersMaps.AddIfNotNull(memberMap);

                if (memberMap is UnflattenedMap)
                {
                    var unflattingClassMaps = DesignUnflattingClassMaps(from, memberMap.ToType, memberMap.ToName);
                    maps.AddRange(unflattingClassMaps);
                }

                if (memberMap is { IsSameTypes: false })
                {
                    maps.AddRange(DesignMaps(memberMap.FromType, memberMap.ToType));
                }
            }

            maps.Add(new ClassMap(from, to, membersMaps));

            return maps;
        }

        public MemberMap? DesignMemberMap(Type from, IMember to, MemberMapType mapType)
        {
            var fromProperty = from.FindProperty(to.Name);
            if (fromProperty != null)
            {
                return new MemberMap(fromProperty, to, mapType);
            }

            var (flattenProperty, mappedProperty) = from.FindFlattenMappedProperty(to.Name);
            if (flattenProperty != null && mappedProperty != null)
            {
                return new FlattenedMap(mappedProperty, to, mapType, flattenProperty.Name);
            }

            if (IsUnflattening(from, to))
            {
                return new UnflattenedMap(from, to, mapType);
            }

            return null;
        }

        private bool IsUnflattening(Type from, IMember to)
        {
            var constructor = from.GetOptimalUnflatteningConstructor(to.Type, to.Name);
            if (constructor == null)
            {
                return false;
            }

            var flattenProperties = to.Type.Properties.Select(x => $"{to.Name}{x.Name}");
            var isUnflattening = from.GetPropertiesNames().Any(x => flattenProperties.Contains(x));

            return isUnflattening;
        }

        public ImmutableArray<TypeMap> DesignUnflattingClassMaps(Type from, Type unflatteningMemberType, string unflatteningMemberName) 
            => DesignUnflatteningMaps(from, unflatteningMemberType, unflatteningMemberName).ToImmutableArray();

        private List<TypeMap> DesignUnflatteningMaps(Type from, Type unflatteningMemberType, string unflatteningMemberName)
        {
            var maps = new List<TypeMap>();
            var constructor = from.GetOptimalUnflatteningConstructor(unflatteningMemberType, unflatteningMemberName);
            if (constructor == null)
            {
                return new();
            }
            var toMembers = constructor.Parameters.OfType<IMember>()
                .Concat(constructor.GetInitializerProperties());

            var membersMaps = new List<MemberMap>();
            foreach(var member in toMembers)
            {
                var fromProperty = from.FindProperty($"{unflatteningMemberName}{member.Name}");
                MemberMap? map = (fromProperty, member) switch
                {
                    ({ }, Parameter parameter) => new MemberMap(fromProperty, parameter, MemberMapType.Constructor),
                    ({ }, Property property) => new MemberMap(fromProperty, property, MemberMapType.Initializer),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);

                if (map is { IsSameTypes: false })
                {
                    maps.AddRange(DesignMaps(map.FromType, map.ToType));
                }
            }

            maps.Add(new ClassMap(from, unflatteningMemberType, membersMaps, isUnflattening: true));
            return maps;
        }
    }
}
