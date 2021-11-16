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
        private readonly ClassMapDesigner _classMapDesigner;
        private readonly DiagnosticReporter _diagnosticReporter;

        public ClassPartialMapDesigner(DiagnosticReporter diagnosticReporter)
        {
            _classMapDesigner = new(diagnosticReporter);
            _diagnosticReporter = diagnosticReporter;
        }

        public List<ClassMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, IMethodSymbol userConstructor, MethodDeclarationSyntax userMethod)
        {
            var objCreationExpression = userMethod.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                _diagnosticReporter.ReportObjectCreationExpressionNotFoundError(userMethod.GetLocation(), from, to);
                return new();
            }
            var byConstructor = userConstructor.GetParametersNames();
            var byInitialyzer = objCreationExpression.GetInitializersLeft();
            var byUser = byConstructor.Union(byInitialyzer);
            var constructor = from.GetOptimalConstructor(to, byUser);
            if (constructor == null)
            {
                _diagnosticReporter.ReportConstructorNotFoundError(to.Locations, from, to);
                return new();
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
                    //TODO: parameter must have same name as property. try this "to.FindProperty(parameter.Name) is IPropertySymbol property ? MemberMap.User(property, parameter) : null"
                    (IParameterSymbol parameter, true) => MemberMap.User(to.FindProperty(parameter.Name)!, parameter),
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

            var customStatements = userMethod.Body != null
                ? userMethod.Body.Statements.ToList()
                : new() { SyntaxFactory.ReturnStatement(objCreationExpression).NormalizeWhitespace() };
            var customParameterName = userMethod.ParameterList.Parameters.First().Identifier.Text;

            maps.Add(new ClassPartialMap(from, to, membersMaps, customStatements, customParameterName));

            return maps;
        }
    }
}
