using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public static class MapDesignersHelper
    {
        public static ObjectCreationExpressionSyntax? GetObjectCreateionExpression(this BaseMethodDeclarationSyntax method)
        {
            var objCreationExpression = method.ExpressionBody != null
                ? method.ExpressionBody?.Expression as ObjectCreationExpressionSyntax
                : method.Body?.Statements.OfType<ReturnStatementSyntax>().Last().Expression as ObjectCreationExpressionSyntax;

            return objCreationExpression;
        }

        public static List<ISymbol> GetPropertiesInitializedByConstructorAndInitializer(this IMethodSymbol constructor)
        {
            IEnumerable<ISymbol> constructorParameters = constructor.GetParameters();
            var initializerProperties = constructor.ContainingType
                .GetProperties()
                .Where(x => !x.IsReadOnly && !constructor.GetParametersNames().Contains(x.Name, StringComparer.InvariantCultureIgnoreCase));
            var members = constructorParameters.Concat(initializerProperties).ToList();

            return members;
        }
    }
}
