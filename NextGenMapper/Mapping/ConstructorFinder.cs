using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using NextGenMapper.Errors;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Comparers;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NextGenMapper.Mapping
{
    internal static class ConstructorFinder
    {
        private static readonly ConstructorsComparer _constructorComparer = new();

        public static ConstructorForMapping GetOptimalConstructor(
            Dictionary<string, IPropertySymbol> sourceProperties,
            ITypeSymbol destination,
            SemanticModel semanticModel,
            CancellationToken cancellationToken) 
            => GetOptimalConstructor(sourceProperties, destination, new HashSet<string>(), semanticModel, cancellationToken);

        public static ConstructorForMapping GetOptimalConstructor(
            Dictionary<string, IPropertySymbol> sourceProperties,
            ITypeSymbol destination,
            HashSet<string> userArguments,
            SemanticModel semanticModel, 
            CancellationToken cancellationToken)
        {
            var constructors = GetSortedPublicConstructors(destination);

            foreach (var constructor in constructors)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var constructorAssignments = GetAssignments(constructor, semanticModel, cancellationToken);
                if (IsConstructorValid(constructor, constructorAssignments, sourceProperties, userArguments, out var error))
                {
                    return new ConstructorForMapping(constructor, constructorAssignments);
                }
                else if (error is not null)
                {
                    return new ConstructorForMapping(error);
                }
            }

            return new ConstructorForMapping();
        }

        public static ConstructorForMapping GetPublicDefaultConstructor(ITypeSymbol type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                if (constructor.Parameters.Length == 0 && constructor.DeclaredAccessibility
                    is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
                {
                    return new ConstructorForMapping(constructor);
                }
            }

            return new ConstructorForMapping();
        }

        public static ReadOnlySpan<Assignment> GetAssignments(
            IMethodSymbol constructor, 
            SemanticModel semanticModel, 
            CancellationToken cancellationToken)
        {
            if (constructor.IsImplicitlyDeclared)
            {
                return new();
            }

            if (constructor.Locations.FirstOrDefault() is not Location { SourceTree: not null })
            {
                return GetAssignmentsBySemantic(constructor);
            }

            return constructor.GetFirstDeclarationSyntax() switch
            {
                ConstructorDeclarationSyntax constructorSyntax => GetConstructorAssignments(constructorSyntax, semanticModel, cancellationToken),
                RecordDeclarationSyntax => GetRecordAssignments(constructor),
                _ => new()
            };
        }

        private static bool IsConstructorValid(
            IMethodSymbol constructor,
            ReadOnlySpan<Assignment> constructorAssignments,
            Dictionary<string, IPropertySymbol> sourceProperties,
            HashSet<string> userArguments,
            out MappingError?  error)
        {
            error = null;
            var assignmentsByParameter = new Dictionary<string, Assignment>(StringComparer.InvariantCulture);
            foreach (var assignment in constructorAssignments)
            {
                if (assignmentsByParameter.TryGetValue(assignment.Parameter, out var existAssigment))
                {
                    error = new MultipleInitializationError(assignment.Parameter, new[] { existAssigment.Property, assignment.Property });
                    return false;
                }

                assignmentsByParameter.Add(assignment.Parameter, assignment);
            }

            foreach (var parameter in constructor.Parameters)
            {
                if (assignmentsByParameter.TryGetValue(parameter.Name, out var assignment))
                {
                    if (!sourceProperties.ContainsKey(assignment.Property)
                        && !userArguments.Contains(assignment.Parameter))
                    {
                        return false;
                    }
                }
                else if (!parameter.IsOptional)
                {
                    return false;
                }
            }

            return true;
        }

        private static ReadOnlySpan<IMethodSymbol> GetSortedPublicConstructors(ITypeSymbol type)
        {
            var constructors = type.GetConstructors();

            Span<IMethodSymbol> publicConstructors = new IMethodSymbol[constructors.Length];
            var count = 0;
            foreach (var constructor in constructors)
            {
                if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
                {
                    publicConstructors[count] = constructor;
                    count++;
                }
            }
            publicConstructors = publicConstructors.Slice(0, count);

            BubbleSort.Sort(ref publicConstructors, _constructorComparer);

            return publicConstructors;
        }

        private static ReadOnlySpan<Assignment> GetAssignmentsBySemantic(IMethodSymbol constructor)
        {
            var assignments = new ValueListBuilder<Assignment>(constructor.Parameters.Length);
            var members = constructor.ContainingType.GetMembers().AsSpan();
            var parameters = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var parameter in constructor.Parameters.AsSpan())
            {
                parameters.Add(parameter.Name, parameter.Name);
            }
            foreach (var member in members)
            {
                if (member is not IPropertySymbol
                    {
                        CanBeReferencedByName: true,
                        DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                    } property)
                {
                    continue;
                }
                if (parameters.TryGetValue(property.Name, out var parameter))
                {
                    assignments.Append(new Assignment(property.Name, parameter));
                }
            }

            return assignments.AsSpan();
        }

        private static ReadOnlySpan<Assignment> GetRecordAssignments(IMethodSymbol methodSymbol)
        {
            var assignments = new ValueListBuilder<Assignment>(methodSymbol.Parameters.Length);
            foreach (var parameter in methodSymbol.Parameters)
            {
                assignments.Append(new Assignment(parameter.Name, parameter.Name));
            }

            return assignments.AsSpan();
        }

        private static ReadOnlySpan<Assignment> GetConstructorAssignments(
            ConstructorDeclarationSyntax constructorSyntax,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var chainedConstructorsAssignments = GetChainedConstructorsAssignments(constructorSyntax, semanticModel, cancellationToken);

            var assignments = new ValueListBuilder<Assignment>(
                constructorSyntax.Body?.Statements.Count ?? 1 + chainedConstructorsAssignments.Length);
            foreach (var assignment in chainedConstructorsAssignments)
            {
                assignments.Append(assignment);
            }

            if (constructorSyntax.Body is not null)
            {
                for (int i = 0; i < constructorSyntax.Body.Statements.Count; i++)
                {
                    if (constructorSyntax.Body.Statements[i] is ExpressionStatementSyntax
                        { Expression: AssignmentExpressionSyntax assignmentExpression })
                    {
                        var assignment = GetAssignment(assignmentExpression);
                        if (assignment is not null)
                        {
                            assignments.Append(assignment.Value);
                        }
                    }
                }
            }
            else if (constructorSyntax.ExpressionBody is ArrowExpressionClauseSyntax
            {
                Expression: AssignmentExpressionSyntax assignmentExpression
            })
            {
                var assignment = GetAssignment(assignmentExpression);

                if (assignment is not null)
                {
                    assignments.Append(assignment.Value);
                }
            }

            return assignments.AsSpan();
        }

        private static ReadOnlySpan<Assignment> GetChainedConstructorsAssignments(
            ConstructorDeclarationSyntax constructorSyntax,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (constructorSyntax.Initializer?.Kind() is SyntaxKind.ThisConstructorInitializer
                && semanticModel.GetOperation(constructorSyntax.Initializer, cancellationToken) is IInvocationOperation thisInvocationOperation)
            {
                var argumentsByParameters = new Dictionary<string, string>(StringComparer.InvariantCulture);
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
                        argumentsByParameters.Add(parameterName, argumentName);
                    }
                }

                var assignments = GetAssignments(thisInvocationOperation.TargetMethod, semanticModel, cancellationToken);
                var chainedConstructorsAssignments = new ValueListBuilder<Assignment>(assignments.Length);
                foreach (var assignment in assignments)
                {
                    if (argumentsByParameters.TryGetValue(assignment.Parameter, out var argument))
                    {
                        chainedConstructorsAssignments.Append(new Assignment(assignment.Property, argument));
                    }
                }

                return chainedConstructorsAssignments.AsSpan();
            }

            return Array.Empty<Assignment>();
        }

        private static Assignment? GetAssignment(AssignmentExpressionSyntax assignmentExpression)
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

            return new Assignment(property, parameter);
        }
    }
}