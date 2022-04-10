using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public static class MapDesignersHelper
    {
        public static ObjectCreationExpressionSyntax? GetObjectCreateionExpression(this BaseMethodDeclarationSyntax method)
        {
            if (method.ExpressionBody != null)
            {
                return method.ExpressionBody?.Expression as ObjectCreationExpressionSyntax;
            }
            else if (method.Body != null)
            {
                for(var i = method.Body.Statements.Count - 1; i >= 0; i--)
                {
                    if (method.Body.Statements[i] is ReturnStatementSyntax returnStatement)
                    {
                        return returnStatement.Expression as ObjectCreationExpressionSyntax;
                    }
                }
            }
            //TODO: add diagnostic
            throw new ArgumentException($"Return statement not found for method {method}");
        }

        public static ImmutableArray<ISymbol> GetPropertiesInitializedByConstructorAndInitializer(this IMethodSymbol constructor)
        {
            var constructorParametersNames = constructor.GetParametersNames().ToArray().ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);
            var members = ImmutableArray.Create<ISymbol>();
            foreach (var parameter in constructor.Parameters)
            {
                members = members.Add(parameter);
            }
            foreach (var constructorTypeProperty in constructor.ContainingType.GetPublicProperties())
            {
                if (!constructorTypeProperty.IsReadOnly && !constructorParametersNames.Contains(constructorTypeProperty.Name))
                {
                    members = members.Add(constructorTypeProperty);
                }
            }

            return members;
        }
    }
}
