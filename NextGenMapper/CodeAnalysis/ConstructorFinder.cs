using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using NextGenMapper.Extensions;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public readonly ref struct ConstructorForMapping
    {
        public IMethodSymbol? ConstructorSymbol { get; }
        public ReadOnlySpan<Assigment> Assigments { get; }

        public ConstructorForMapping()
        {
            ConstructorSymbol = null;
            Assigments = Array.Empty<Assigment>();
        }

        public ConstructorForMapping(IMethodSymbol constructorMethodSymbol)
        {
            ConstructorSymbol = constructorMethodSymbol;
            Assigments = Array.Empty<Assigment>();
        }

        public ConstructorForMapping(IMethodSymbol constructorMethodSymbol, ReadOnlySpan<Assigment> assigments)
        {
            ConstructorSymbol = constructorMethodSymbol;
            Assigments = assigments;
        }

        public void Deconstruct(out IMethodSymbol? constructorSymbol, out ReadOnlySpan<Assigment> assigments)
        {
            constructorSymbol = ConstructorSymbol;
            assigments = Assigments;
        }
    }

    //TODO: refactoring
    public class ConstructorFinder
    {
        private readonly SemanticModel _semanticModel;
        private readonly ConstructorComparer _constructorComparer = new();

        public ConstructorFinder(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public ConstructorForMapping GetOptimalConstructor(
            Dictionary<string, IPropertySymbol> sourcePropertiesDictionary,
            ITypeSymbol destination,
            HashSet<string>? userArguments = null)
        {
            userArguments ??= new HashSet<string>();

            var constructors = destination.GetPublicConstructors();
            BubbleSort.Sort(ref constructors, _constructorComparer);
            if (constructors.Length == 0)
            {
                return new ConstructorForMapping();
            }

            bool ValidateCommonCostructor(IMethodSymbol constructor, ReadOnlySpan<Assigment> constructorAssigments)
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
                        else if (sourcePropertiesDictionary.ContainsKey(assigment.Property))
                        {
                            assigmentNotFound = false;
                        }
                        else if (userArguments.Contains(assigment.Parameter))
                        {
                            assigmentNotFound = false;
                        }
                    }
                    if (assigmentNotFound && !parameter.IsOptional)
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var constructor in constructors)
            {
                if (constructor.IsImplicitlyDeclared)
                {
                    return new ConstructorForMapping(constructor);
                }
                var location = constructor.Locations.FirstOrDefault();
                if (location?.SourceTree is null)
                {
                    var constructorAssigments = GetAssigmentsBySemantic(constructor);
                    if (ValidateCommonCostructor(constructor, constructorAssigments))
                    {
                        return new ConstructorForMapping(constructor, constructorAssigments);
                    }
                }
                else
                {
                    var syntax = location.SourceTree.GetCompilationUnitRoot().FindNode(location.SourceSpan);
                    var constructorAssigments = syntax switch
                    {
                        ConstructorDeclarationSyntax constructorSyntax => GetConstructorAssigments(constructorSyntax),
                        RecordDeclarationSyntax => GetRecordAssigments(constructor),
                        _ => new()
                    };
                    if (ValidateCommonCostructor(constructor, constructorAssigments))
                    {
                        return new ConstructorForMapping(constructor, constructorAssigments);
                    }
                }
            }

            return new ConstructorForMapping();
        }

        public ReadOnlySpan<Assigment> GetAssigments(IMethodSymbol constructor)
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

        private ReadOnlySpan<Assigment> GetAssigmentsBySemantic(IMethodSymbol constructor)
        {
            var assigments = new ValueListBuilder<Assigment>(constructor.Parameters.Length);
            var properties = constructor.ContainingType.GetPublicPropertiesNames();
            foreach (var parameter in constructor.Parameters)
            {
                foreach (var property in properties)
                {
                    if (parameter.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase))
                    {
                        assigments.Append(new Assigment(property, parameter.Name));
                        break;
                    }
                }
            }

            return assigments.AsSpan();
        }

        private ReadOnlySpan<Assigment> GetRecordAssigments(IMethodSymbol methodSymbol)
        {
            var assigments = new ValueListBuilder<Assigment>(methodSymbol.Parameters.Length);
            foreach (var parameter in methodSymbol.Parameters)
            {
                assigments.Append(new Assigment(parameter.Name, parameter.Name));
            }

            return assigments.AsSpan();
        }

        private ReadOnlySpan<Assigment> GetConstructorAssigments(ConstructorDeclarationSyntax constructorSyntax)
        {
            var inheritedAssigments = new ValueListBuilder<Assigment>(8);

            if (constructorSyntax.Initializer?.Kind() == SyntaxKind.ThisConstructorInitializer
                && _semanticModel.GetOperation(constructorSyntax.Initializer) is IInvocationOperation thisInvocationOperation)
            {
                var argumentParameterPairs = new List<(string argument, string parameter)>(thisInvocationOperation.Arguments.Length);
                foreach (var argument in thisInvocationOperation.Arguments)
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
                var assigments = GetAssigments(thisInvocationOperation.TargetMethod);
                foreach (var assigment in assigments)
                {
                    var argument = argumentParameterPairs
                        .Where(x => x.parameter == assigment.Parameter)
                        .Select(x => x.argument)
                        .FirstOrDefault();
                    if (argument is not null)
                    {
                        inheritedAssigments.Append(new Assigment(assigment.Property, argument));
                    }
                }
            }

            if (constructorSyntax.Body is not null)
            {
                var assigments = new ValueListBuilder<Assigment>(constructorSyntax.Body.Statements.Count + inheritedAssigments.Length);
                for (int i = 0; i < constructorSyntax.Body.Statements.Count; i++)
                {
                    if (constructorSyntax.Body.Statements[i] is ExpressionStatementSyntax
                        { Expression: AssignmentExpressionSyntax assignmentExpression })
                    {
                        var assigment = GetAssigment(assignmentExpression);
                        if (assigment is not null)
                        {
                            assigments.Append(assigment);
                        }
                    }
                }
                foreach(var assigment in inheritedAssigments.AsSpan())
                {
                    assigments.Append(assigment);
                }

                return assigments.AsSpan();
            }
            else
            {
                if (constructorSyntax.ExpressionBody is ArrowExpressionClauseSyntax { Expression: AssignmentExpressionSyntax assignmentExpression })
                {
                    var assigment = GetAssigment(assignmentExpression);

                    if (assigment is not null)
                    {
                        inheritedAssigments.Append(assigment);

                        return inheritedAssigments.AsSpan();
                    }
                }
            }

            return inheritedAssigments.AsSpan();
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