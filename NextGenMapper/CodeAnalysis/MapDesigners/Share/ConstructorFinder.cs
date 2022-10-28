using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using NextGenMapper.Extensions;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ConstructorFinder
    {
        private readonly SemanticModel _semanticModel;
        private readonly ConstructorComparer _constructorComparer = new();

        public ConstructorFinder(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public (IMethodSymbol? constructor, List<Assigment> assigments) GetOptimalConstructor(ITypeSymbol from, ITypeSymbol to, HashSet<string> byUser)
        {
            var constructors = to.GetPublicConstructors();
            BubbleSort.SortSpan(ref constructors, _constructorComparer);
            if (constructors.Length == 0)
            {
                return (null, new());
            }

            var publicPropertiesNames = from.GetPublicPropertiesNames();
            var fromPropertiesNames = new HashSet<string>(StringComparer.InvariantCulture);
            foreach (var publicProperty in publicPropertiesNames)
            {
                fromPropertiesNames.Add(publicProperty);
            }
            bool ValidateCommonCostructor(IMethodSymbol constructor, List<Assigment> constructorAssigments)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    var assigmentNotFound = true;
                    foreach (var assigment in constructorAssigments)
                    {
                        if (parameter.Name != assigment.Parameter)
                        {
                            continue;
                        }
                        else if (fromPropertiesNames.Contains(assigment.Property))
                        {
                            assigmentNotFound = false;
                        }
                    }
                    if (assigmentNotFound && !parameter.IsOptional && !byUser.Contains(parameter.Name))
                    {
                        return false;
                    }
                }

                return true;
            }

            IMethodSymbol? commonConstructor = null;
            List<Assigment> constructorAssigments = new();

            foreach (var constructor in constructors)
            {
                if (constructor.IsImplicitlyDeclared)
                {
                    return (constructor, new());
                }
                var location = constructor.Locations.FirstOrDefault();
                if (location?.SourceTree is null)
                {
                    constructorAssigments = GetAssigmentsBySemantic(constructor);
                    if (ValidateCommonCostructor(constructor, constructorAssigments))
                    {
                        commonConstructor = constructor;
                        break;
                    }
                }
                else
                {
                    var syntax = location.SourceTree.GetCompilationUnitRoot().FindNode(location.SourceSpan);
                    constructorAssigments = syntax switch
                    {
                        ConstructorDeclarationSyntax constructorSyntax => GetConstructorAssigments(constructorSyntax),
                        RecordDeclarationSyntax => GetRecordAssigments(constructor),
                        _ => new()
                    };
                    if (ValidateCommonCostructor(constructor, constructorAssigments))
                    {
                        commonConstructor = constructor;
                        break;
                    }
                }
            }

            return (commonConstructor, constructorAssigments);
        }

        public List<Assigment> GetAssigments(IMethodSymbol constructor)
        {
            if (constructor.IsImplicitlyDeclared)
            {
                return new();
            }
            var location = constructor.Locations.FirstOrDefault();
            if (location is null || location.SourceTree is null)
            {
                return new();
            }
            var syntax = location.SourceTree.GetCompilationUnitRoot().FindNode(location.SourceSpan);
            return syntax switch
            {
                ConstructorDeclarationSyntax constructorSyntax => GetConstructorAssigments(constructorSyntax),
                RecordDeclarationSyntax => GetRecordAssigments(constructor),
                _ => new()
            };
        }

        private List<Assigment> GetAssigmentsBySemantic(IMethodSymbol constructor)
        {
            var assigments = new List<Assigment>(constructor.Parameters.Length);
            var properties = constructor.ContainingType.GetPublicPropertiesNames();
            foreach(var parameter in constructor.Parameters)
            {
                foreach (var property in properties)
                {
                    if (parameter.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase))
                    {
                        assigments.Add(new Assigment(property, parameter.Name));
                        break;
                    }
                }
            }

            return assigments;
        }

        private List<Assigment> GetRecordAssigments(IMethodSymbol methodSymbol)
        {
            var assigments = new List<Assigment>(methodSymbol.Parameters.Length);
            foreach (var parameter in methodSymbol.Parameters)
            {
                assigments.Add(new Assigment(parameter.Name, parameter.Name));
            }

            return assigments;
        }

        private List<Assigment> GetConstructorAssigments(ConstructorDeclarationSyntax constructorSyntax)
        {
            var inheritedAssigments = new List<Assigment>();

            if (constructorSyntax.Initializer?.Kind() == SyntaxKind.ThisConstructorInitializer)
            {
                var thisOperation = _semanticModel.GetOperation(constructorSyntax.Initializer) as IInvocationOperation;
                var targetConstructor = thisOperation.TargetMethod;
                var argumentParameterPairs = new List<(string argument, string parameter)>(thisOperation.Arguments.Length);
                foreach(var argument in thisOperation.Arguments)
                {
                    if (argument is
                        {
                            Syntax: ArgumentSyntax
                            {
                                Expression: IdentifierNameSyntax
                                {
                                    Identifier.ValueText: string argumentName
                                }
                            },
                            Parameter.Name: string parameterName
                        })
                    {
                        argumentParameterPairs.Add((argumentName, parameterName));
                    }
                }
                var assigments = GetAssigments(targetConstructor);
                foreach(var assigment in assigments)
                {
                    var argument = argumentParameterPairs
                        .Where(x => x.parameter == assigment.Parameter)
                        .Select(x => x.argument)
                        .FirstOrDefault();
                    if (argument is not null)
                    {
                        inheritedAssigments.Add(new Assigment(assigment.Property, argument));
                    }
                }
            }

            if (constructorSyntax.Body is not null)
            {
                List<Assigment> assigments = new(constructorSyntax.Body.Statements.Count);
                for (int i = 0; i < constructorSyntax.Body.Statements.Count; i++)
                {
                    if (constructorSyntax.Body.Statements[i] is ExpressionStatementSyntax
                        { Expression: AssignmentExpressionSyntax assignmentExpression })
                    {
                        var assigment = GetAssigment(assignmentExpression);
                        if (assigment is not null)
                        {
                            assigments.Add(assigment);
                        }
                    }
                }
                assigments.AddRange(inheritedAssigments);

                return assigments;
            }
            else
            {
                if (constructorSyntax.ExpressionBody is ArrowExpressionClauseSyntax { Expression: AssignmentExpressionSyntax assignmentExpression })
                {
                    var assigment = GetAssigment(assignmentExpression);

                    if (assigment is not null)
                    {
                        inheritedAssigments.Add(assigment);

                        return inheritedAssigments;
                    }
                }
            }

            return inheritedAssigments;
        }

        private Assigment? GetAssigment(AssignmentExpressionSyntax assignmentExpression)
        {
            var property = assignmentExpression.Left switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                MemberAccessExpressionSyntax
                {
                    Expression: ThisExpressionSyntax,
                    Name: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            var parameter = assignmentExpression.Right switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                BinaryExpressionSyntax
                {
                    RawKind: (int)SyntaxKind.CoalesceExpression,
                    Left: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            if (property is null || parameter is null)
            {
                return null;
            }

            return new Assigment(property, parameter);
        }

        private class ConstructorComparer : IComparer<IMethodSymbol>
        {
            public int Compare(IMethodSymbol x, IMethodSymbol y) => x.Parameters.Length.CompareTo(y.Parameters.Length) * -1;
        }
    }

    public class Assigment
    {
        public Assigment(string property, string parameter)
        {
            Property = property;
            Parameter = parameter;
        }

        public string Property { get; }
        public string Parameter { get; }
    }
}