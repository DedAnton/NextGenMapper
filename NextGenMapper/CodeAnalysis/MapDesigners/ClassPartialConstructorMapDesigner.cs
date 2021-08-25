using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialConstructorMapDesigner
    {
        private readonly ClassMapDesigner _classMapDesigner;

        public ClassPartialConstructorMapDesigner()
        {
            _classMapDesigner = new();
        }

        public List<ClassMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, IMethodSymbol constructor, MethodDeclarationSyntax methodSyntax)
        {
            var objCreationExpression = methodSyntax.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                throw new ArgumentException($"Error when create mapping for method \"{methodSyntax}\", object creation expression was not found. Partial methods must end with object creation like \"return new Class()\"");
            }

            var argumentByParameterName = objCreationExpression.ArgumentList?.Arguments
                .Where(x => !x.IsDefaultLiteralExpression())
                .Select(x => new { Argument = x, ParameterName = constructor.GetConstructorParameter(x).Name })
                .ToDictionary(x => x.ParameterName, x => x.Argument, StringComparer.InvariantCultureIgnoreCase) ?? new();

            var initializerByPropertyName = objCreationExpression.Initializer?.Expressions
                .OfType<InitializerExpressionSyntax>()
                .Select(x => new { Initializer = x, PropertyName = x.GetInitializerLeft() })
                .Where(x => x.PropertyName != null)
                .ToDictionary(x => x.PropertyName, x => x.Initializer) ?? new();

            var maps = new List<ClassMap>();
            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.GetConstructorInitializerMembers();
            foreach (var member in toMembers)
            {
                MemberMap? memberMap = (member) switch
                {
                    IParameterSymbol parameter when argumentByParameterName.TryGetValue(member.Name, out var argument) => MemberMap.Argument(parameter, argument),
                    IPropertySymbol property when initializerByPropertyName.TryGetValue(member.Name, out var initializer) => MemberMap.InitializerExpression(property, initializer),
                    IParameterSymbol parameter => _classMapDesigner.DesignConstructorParameterMap(from, parameter),
                    IPropertySymbol property => _classMapDesigner.DesignInitializerPropertyMap(from, property),
                    _ => null
                };

                if (memberMap == null)
                {
                    continue;
                }
                membersMaps.Add(memberMap);

                if (memberMap.MapType is MemberMapType.UnflattenConstructor or MemberMapType.UnflattenInitializer)
                {
                    maps.AddRange(_classMapDesigner.DesignUnflattingClassMap(from, memberMap.ToName, memberMap.ToType));
                }

                if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
                {
                    maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType));
                }
            }

            var customParameterName = methodSyntax.ParameterList.Parameters.First().Identifier.Text;
            maps.Add(new ClassPartialConstructorMap(from, to, membersMaps, customParameterName));

            return maps;
        }
    }
}
