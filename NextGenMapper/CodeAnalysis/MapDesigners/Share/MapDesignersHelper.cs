using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public static class MapDesignersHelper
    {
        public static bool IsEnumMapping(ITypeSymbol from, ITypeSymbol to)
            => from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum;

        public static bool IsCollectionMapping(ITypeSymbol from, ITypeSymbol to)
            => from.IsGenericEnumerable() && to.IsGenericEnumerable();

        public static bool IsClassMapping(ITypeSymbol from, ITypeSymbol to)
            => from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class;

        public static ObjectCreationExpressionSyntax? GetObjectCreationExpression(this BaseMethodDeclarationSyntax method)
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

        public static List<ISymbol> GetPropertiesInitializedByConstructorAndInitializer(this IMethodSymbol constructor)
        {
            var constructorParametersNames = new HashSet<string>(constructor.GetParametersNames().ToArray(), StringComparer.InvariantCultureIgnoreCase);
            var members = new List<ISymbol>();
            foreach (var parameter in constructor.Parameters)
            {
                members.Add(parameter);
            }
            foreach (var constructorTypeProperty in constructor.ContainingType.GetPublicProperties())
            {
                if (!constructorTypeProperty.IsReadOnly && !constructorParametersNames.Contains(constructorTypeProperty.Name))
                {
                    members.Add(constructorTypeProperty);
                }
            }

            return members;
        }
    }
}
