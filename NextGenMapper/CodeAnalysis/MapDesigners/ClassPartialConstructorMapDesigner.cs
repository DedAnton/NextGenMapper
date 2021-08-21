using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialConstructorMapDesigner
    {
        private readonly string _defaultKeyword;
        private readonly ClassMapDesigner _classMapDesigner;

        public ClassPartialConstructorMapDesigner()
        {
            _defaultKeyword = SyntaxFactory.Token(SyntaxKind.DefaultKeyword).ValueText;
            _classMapDesigner = new();
        }

        public ImmutableArray<TypeMap> DesignClassPartialConstructorMaps(PartialConstructorMapMethod partialConstructorMapMethod) 
            => DesignMaps(partialConstructorMapMethod).ToImmutableArray();

        private List<TypeMap> DesignMaps(PartialConstructorMapMethod partialConstructorMapMethod)
        {
            var maps = new List<TypeMap>();
            var (from, to) = (partialConstructorMapMethod.ReturnType, partialConstructorMapMethod.Parameter.Type);

            var byConstructor = partialConstructorMapMethod.ObjectCreationExpression.Constructor.GetParametersNames().Where(x => x != _defaultKeyword);
            var byInitialyzer = partialConstructorMapMethod.ObjectCreationExpression.InitializerPropertiesNames;
            var byUser = byConstructor.Union(byInitialyzer);

            var constructor = from.GetOptimalConstructor(to, byUser);
            if (constructor == null)
            {
                //TODO: diagnostics
                throw new System.ArgumentException($"Error when create mapping from {from} to {to}, {to} does not have a suitable constructor");
            }

            var argumentByParameterName = partialConstructorMapMethod.ObjectCreationExpression.Constructor.GetParametersNames()
                .Zip(partialConstructorMapMethod.Arguments, (p, a) => (p, a)).ToDictionary(x => x.p, x => x.a, System.StringComparer.InvariantCultureIgnoreCase);
            var initializerByPropertyName = partialConstructorMapMethod.ObjectCreationExpression.InitializerPropertiesNames
                .Zip(partialConstructorMapMethod.InitializerExpressions, (p, i) => (p, i)).ToDictionary(x => x.p, x => x.i, System.StringComparer.InvariantCultureIgnoreCase);

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
                    (Parameter parameter, true) => new CustomArgumentMap(parameter, argumentByParameterName[parameter.Name]),
                    (Property property, true) => new CustomInitializerExpressionMap(property, initializerByPropertyName[property.Name]),
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

            maps.Add(new ClassPartialConstructorMap(from, to, membersMaps, partialConstructorMapMethod.Parameter.Name));

            return maps;
        }
    }
}
