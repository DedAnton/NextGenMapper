using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialMapDesigner
    {
        private readonly SemanticModel _semanticModel;
        private readonly MapPlanner _planner;

        public ClassPartialMapDesigner(SemanticModel semanticModel, MapPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void DesignMapsForPlanner(MethodDeclarationSyntax method)
        {
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);
            var objCreationExpression = method.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                throw new ArgumentException($"Error when create mapping for method \"{method}\", object creation expression was not found. Partial methods must end with object creation like \"return new Class()\"");
            }
            var byConstructor = _semanticModel.GetMethodSymbol(objCreationExpression)?.GetParametersNames() ?? new();
            var byInitialyzer = objCreationExpression.GetInitializersLeft();
            var byUser = byConstructor.Union(byInitialyzer);
            var constructor = from.GetOptimalConstructor(to, byUser);
            if (constructor == null)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} does not have a suitable constructor");
            }

            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.GetConstructorInitializerMembers();
            foreach (var member in toMembers)
            {
                MemberMap? map = member switch
                {
                    IParameterSymbol parameter => DesignConstructorParameterMap(from, to, parameter, byUser),
                    IPropertySymbol property => DesignInitializerPropertyMap(from, property, byUser),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);
                if (map?.IsProvidedByUser == false)
                {
                    DesignMapForDifferentTypes(map);
                }
            }

            var customStatements = method.Body != null 
                ? method.Body.Statements.ToList() 
                : new() { SyntaxFactory.ReturnStatement(objCreationExpression).NormalizeWhitespace() };
            var customParameterName = method.ParameterList.Parameters.First().Identifier.Text;

            _planner.AddCustomMap(new ClassPartialMap(from, to, membersMaps, customStatements, customParameterName), method.GetUsingsAndNamespace());
        }

        private MemberMap? DesignConstructorParameterMap(ITypeSymbol from, ITypeSymbol to, IParameterSymbol constructorParameter, IEnumerable<string> byUser)
        {
            var isProvidedByUser = byUser.Contains(constructorParameter.Name, StringComparer.InvariantCultureIgnoreCase);
            if (isProvidedByUser)
            {
                var property = to.FindProperty(constructorParameter.Name);
                if (property == null)
                {
                    throw new ArgumentException($"Error when map {from} to {to}. Property {constructorParameter.Name} was not found. Constructor parameter must have Property with same name");
                }
                return MemberMap.User(property, constructorParameter);
            }

            var fromProperty = from.FindProperty(constructorParameter.Name);
            if (fromProperty != null)
            {
                return MemberMap.Counstructor(fromProperty, constructorParameter);
            }

            var (flattenProperty, mappedProperty) = from.FindFlattenMappedProperty(constructorParameter.Name);
            if (flattenProperty != null && mappedProperty != null)
            {
                return MemberMap.Counstructor(mappedProperty, constructorParameter, flattenPropertyName: flattenProperty.Name);
            }

            var unflattingClassMap = DesignUnflattingClassMap(from, constructorParameter.Name, constructorParameter.Type);
            if (unflattingClassMap != null)
            {
                _planner.AddCommonMap(unflattingClassMap);
                return MemberMap.CounstructorUnflatten(from, constructorParameter);
            }

            return null;
        }

        private MemberMap? DesignInitializerPropertyMap(ITypeSymbol from, IPropertySymbol initializerProperty, IEnumerable<string> byUser)
        {
            var isProvidedByUser = byUser.Contains(initializerProperty.Name, StringComparer.InvariantCultureIgnoreCase);
            if (isProvidedByUser)
            {
                return MemberMap.User(initializerProperty);
            }

            var fromProperty = from.FindProperty(initializerProperty.Name);
            if (fromProperty != null)
            {
                return MemberMap.Initializer(fromProperty, initializerProperty);
            }

            var (flattenProperty, mappedProperty) = from.FindFlattenMappedProperty(initializerProperty.Name);
            if (flattenProperty != null && mappedProperty != null)
            {
                return MemberMap.Initializer(mappedProperty, initializerProperty, flattenPropertyName: flattenProperty.Name);
            }

            var unflattingClassMap = DesignUnflattingClassMap(from, initializerProperty.Name, initializerProperty.Type);
            if (unflattingClassMap != null)
            {
                _planner.AddCommonMap(unflattingClassMap);
                return MemberMap.InitializerUnflatten(from, initializerProperty);
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
            foreach (var member in toMembers)
            {
                var fromProperty = from.FindProperty($"{unflattingPropertyName}{member.Name}");
                MemberMap? map = (fromProperty, member) switch
                {
                    ({ }, IParameterSymbol parameter) => MemberMap.Counstructor(fromProperty, parameter),
                    ({ }, IPropertySymbol property) => MemberMap.Initializer(fromProperty, property),
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
                var designer = new ClassMapDesigner(_planner);
                designer.DesignMapsForPlanner(map.FromType, map.ToType);
            }
        }
    }
}
