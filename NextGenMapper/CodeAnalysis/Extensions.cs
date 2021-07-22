using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public static class Extensions
    {
        public static (ITypeSymbol ReturnType, ITypeSymbol SingleParameterType) GetReturnAndParameterType(this SemanticModel semanticModel, MethodDeclarationSyntax method)
        {
            var parameter = method.ParameterList.Parameters.SingleOrDefault();
            if (parameter == null)
            {
                throw new ArgumentException("method must contains one parameter");
            }
            var returnType = semanticModel.GetTypeSymbol(method.ReturnType);
            var parameterType = semanticModel.GetTypeSymbol(parameter.Type);

            return (returnType, parameterType);
        }

        public static string GetPropertyNameInitializedBy(this IMethodSymbol constructor, string parameterName)
        {
            if (constructor == null || constructor.MethodKind != MethodKind.Constructor)
            {
                throw new ArgumentException($"method \"{constructor}\" is not constructor");
            }

            var constructorDeclaration = constructor.GetFirstDeclaration<ConstructorDeclarationSyntax>();
            string propertyName = null;
            if (constructorDeclaration.Body != null)
            {
                propertyName = constructorDeclaration.GetStatements().Select(x => x.As<ExpressionStatementSyntax>().Expression.As<AssignmentExpressionSyntax>())
                    .Where(x => x.GetRightAssigmentIdentifierName().ToUpperInvariant() == parameterName.ToUpperInvariant())
                    .Select(x => x.GetLeftAssigmentIdentifierName())
                    .FirstOrDefault();
            }
            else if (constructorDeclaration.GetExpression<AssignmentExpressionSyntax>()
                .GetRightAssigmentIdentifierName().ToUpperInvariant() == parameterName.ToUpperInvariant())
            {
                propertyName = constructorDeclaration.ExpressionBody.Expression.As<AssignmentExpressionSyntax>().GetLeftAssigmentIdentifierName();
            }

            return propertyName;
        }

        public static IMethodSymbol GetOptimalConstructor(this ITypeSymbol from, ITypeSymbol to)
        {
            var constructors = to.GetPublicConstructors().OrderByParametersDesc();
            if (constructors.Count() == 0)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }
            var constructor = constructors.FirstOrDefault(x =>
                x.GetParametersNames().ToUpperInvariant()
                .Complement(from.GetPropertiesNames().ToUpperInvariant())
                .IsEmpty());
            if (constructor == null)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} does not have a suitable constructor");
            } 

            return constructor;
        }

        public static ObjectCreationExpressionSyntax GetObjectCreateionExpression(this BaseMethodDeclarationSyntax method)
        {
            var objCreationExpression = method.ExpressionBody != null
                ? method.GetExpression<ObjectCreationExpressionSyntax>()
                : method.GetReturnStatement().Expression as ObjectCreationExpressionSyntax;

            return objCreationExpression;
        }

        public static bool IsPrimitveTypesMapping(this PropertyMapping mapping) => mapping.TypeFrom.IsPrivitive() || mapping.TypeTo.IsPrivitive();

        public static IParameterSymbol GetConstructorParameter(this SemanticModel semanticModel, ArgumentSyntax argument)
        {
            //argument -> argumentList -> method
            var methodDeclaration = argument?.Parent?.Parent as ObjectCreationExpressionSyntax;
            var method = semanticModel.GetSymbol(methodDeclaration) as IMethodSymbol;
            var index = methodDeclaration.ArgumentList.Arguments.IndexOf(argument);
            var parameter = method.Parameters[index];

            return parameter;
        }
    }
}
