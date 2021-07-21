using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper.CodeAnalysis
{
    public class UserPartialMethod
    {
        public MethodDeclarationSyntax Method { get; }
        public ObjectCreationExpressionSyntax ReturnTypeCreationExpression { get; }

        public UserPartialMethod(MethodDeclarationSyntax method)
        {
            ObjectCreationExpressionSyntax objCreationExpression =
                method.ExpressionBody != null
                ? method.ExpressionBody.Expression as ObjectCreationExpressionSyntax
                : (method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax).Expression as ObjectCreationExpressionSyntax;
            var hasDefault = objCreationExpression.ArgumentList.Arguments.Where(x => x.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression);
        }
    }
}
