using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialMapDesigner
    {
        private readonly ClassMapDesigner _classMapDesigner;

        public ClassPartialMapDesigner()
        {
            _classMapDesigner = new();
        }

        public ImmutableArray<TypeMap> DesignClassPartialMaps(PartialMapMethod partialMapMethod) => DesignMaps(partialMapMethod).ToImmutableArray();

        private List<TypeMap> DesignMaps(PartialMapMethod partialMapMethod)
        {
            var maps = new List<TypeMap>();
            var (from, to) = (partialMapMethod.ReturnType, partialMapMethod.Parameter.Type);

            var byConstructor = partialMapMethod.ObjectCreationExpression.Constructor.GetParametersNames();
            var byInitialyzer = partialMapMethod.ObjectCreationExpression.InitializerPropertiesNames;
            var byUser = byConstructor.Union(byInitialyzer);

            var constructor = from.GetOptimalConstructor(to, byUser);
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
                var isProvidedByUser = byUser.Contains(toMember.Name, System.StringComparer.InvariantCultureIgnoreCase);
                var memberMap = (toMember, isProvidedByUser) switch
                {
                    (Parameter, false) => _classMapDesigner.DesignMemberMap(from, toMember, MemberMapType.Constructor),
                    (Property, false) => _classMapDesigner.DesignMemberMap(from, toMember, MemberMapType.Initializer),
                    (Parameter parameter, true) when to.FindProperty(toMember.Name) is Property property => CustomMap.Constructor(property, parameter),
                    (Property property, true) => CustomMap.Initializer(property),
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
                    var unflattingClassMaps = _classMapDesigner.DesignUnflattingClassMaps(from, memberMap.ToType, memberMap.ToName);
                    maps.AddRange(unflattingClassMaps);
                }

                if (memberMap is not CustomMap and { IsSameTypes: false })
                {
                    maps.AddRange(_classMapDesigner.DesignClassMaps(memberMap.FromType, memberMap.ToType));
                }
            }

            maps.Add(new ClassPartialMap(from, to, membersMaps, partialMapMethod));

            return maps;
        }
    }
}
