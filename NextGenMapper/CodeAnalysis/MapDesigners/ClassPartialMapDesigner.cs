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
        private readonly ClassMapDesigner _classMapDesigner;

        public ClassPartialMapDesigner(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            _classMapDesigner = new();
        }

        public List<ClassMap> DesignMapsForPlanner(MethodDeclarationSyntax method)
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

            var maps = new List<ClassMap>();
            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.GetConstructorInitializerMembers();
            foreach (var member in toMembers)
            {
                var isProvidedByUser = byUser.Contains(member.Name, StringComparer.InvariantCultureIgnoreCase);
                MemberMap? memberMap = (member, isProvidedByUser) switch
                {
                    (IParameterSymbol parameter, false) => _classMapDesigner.DesignConstructorParameterMap(from, parameter),
                    (IPropertySymbol property, false) => _classMapDesigner.DesignInitializerPropertyMap(from, property),
                    (IParameterSymbol parameter, true) => MemberMap.User(to.FindProperty(parameter.Name)!, parameter),//TODO: parameter must have same name as property
                    (IPropertySymbol property, true) => MemberMap.User(property),
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

            var customStatements = method.Body != null 
                ? method.Body.Statements.ToList() 
                : new() { SyntaxFactory.ReturnStatement(objCreationExpression).NormalizeWhitespace() };
            var customParameterName = method.ParameterList.Parameters.First().Identifier.Text;

            maps.Add(new ClassPartialMap(from, to, membersMaps, customStatements, customParameterName));

            return maps;
        }
    }
}
