using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialMapDesigner
    {
        private readonly ClassMapDesigner _classMapDesigner;
        private readonly DiagnosticReporter _diagnosticReporter;
        private readonly ConstructorFinder _constructorFinder;

        public ClassPartialMapDesigner(DiagnosticReporter diagnosticReporter)
        {
            _classMapDesigner = new(diagnosticReporter);
            _diagnosticReporter = diagnosticReporter;
            _constructorFinder = new();
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
            var byInitialyzer = new List<string>();
            if (objCreationExpression.Initializer != null)
            {
                foreach (var expression in objCreationExpression.Initializer.Expressions)
                {
                    if (expression is AssignmentExpressionSyntax assignmentExpression
                        && assignmentExpression.Left is IdentifierNameSyntax identifierNameSyntax)
                    {
                        byInitialyzer.Add(identifierNameSyntax.Identifier.ValueText);
                    }
                }
            }
            var byUser = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach(var parameter in byConstructor)
            {
                byUser.Add(parameter);
            }
            foreach (var initializer in byInitialyzer)
            {
                byUser.Add(initializer);
            }
            var constructor = _constructorFinder.GetOptimalConstructor(from, to, byUser);
            if (constructor == null)
            {
                _diagnosticReporter.ReportConstructorNotFoundError(to.Locations, from, to);
                return new();
            }

            var maps = new List<ClassMap>();
            var membersMaps = new List<MemberMap>();
            var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
            foreach (var member in toMembers)
            {
                var isProvidedByUser = byUser.Contains(member.Name);
                MemberMap? memberMap = (member, isProvidedByUser) switch
                {
                    (IParameterSymbol parameter, false) => _classMapDesigner.DesignConstructorParameterMap(from, parameter),
                    (IPropertySymbol property, false) => _classMapDesigner.DesignInitializerPropertyMap(from, property),
                    (IParameterSymbol parameter, true) => FindPropertyForParameterAndCreateMemberMap(to, parameter),
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
                else if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
                {
                    maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType));
                }
            }

            var customStatements = new List<StatementSyntax>();
            if (userMethod.Body != null)
            {
                customStatements.AddRange(userMethod.Body.Statements);
            }
            else
            {
                customStatements.Add(SyntaxFactory.ReturnStatement(objCreationExpression).NormalizeWhitespace());
            }
            var customParameterName = userMethod.ParameterList.Parameters.First().Identifier.Text;

            maps.Add(new ClassPartialMap(from, to, membersMaps, customStatements, customParameterName));

            return maps;
        }

        private MemberMap? FindPropertyForParameterAndCreateMemberMap(ITypeSymbol to, IParameterSymbol parameter)
        {
            if (to.FindProperty(parameter.Name) is IPropertySymbol property)
            {
                return MemberMap.User(property, parameter);
            }
            else
            {
                _diagnosticReporter.ReportPropertyAndParameterHasDifferentNamesError(parameter.Locations, to, parameter.Name);
                return null;
            }
        }
    }
}
