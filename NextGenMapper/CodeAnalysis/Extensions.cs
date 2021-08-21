using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using StringComparer = System.StringComparer;
using StringComparison = System.StringComparison;

namespace NextGenMapper.CodeAnalysis
{
    public static class Extensions
    {
        public static (ITypeSymbol ReturnType, ITypeSymbol SingleParameterType) GetReturnAndParameterType(this SemanticModel semanticModel, MethodDeclarationSyntax method)
        {
            if (method.ParameterList.Parameters.Count() != 1)
            {
                throw new System.ArgumentException("method must contains one parameter");
            }
            var parameter = method.ParameterList.Parameters.Single();
            var returnType = semanticModel.GetTypeSymbol(method.ReturnType);
            var parameterType = semanticModel.GetTypeSymbol(parameter.Type);
            if (returnType is null || parameterType is null)
            {
                throw new System.ArgumentException("return type and single parameter type must be not null");
            }

            return (returnType, parameterType);
        }

        public static string? GetPropertyNameInitializedBy(this IMethodSymbol constructor, string parameterName)
        {
            if (constructor == null || constructor.MethodKind != MethodKind.Constructor)
            {
                throw new System.ArgumentException($"method \"{constructor}\" is not constructor");
            }

            var constructorDeclaration = constructor.GetFirstDeclaration() as ConstructorDeclarationSyntax;
            string? propertyName = null;
            if (constructorDeclaration?.Body != null)
            {
                propertyName = constructorDeclaration
                    .GetStatements()
                    .OfType<ExpressionStatementSyntax>()
                    .Select(x => x.Expression)
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(x => x.GetRightAssigmentIdentifierName()?.ToUpperInvariant() == parameterName.ToUpperInvariant())
                    .Select(x => x.GetLeftAssigmentIdentifierName())
                    .FirstOrDefault();
            }
            else if (constructorDeclaration?.GetExpression<AssignmentExpressionSyntax>()?
                .GetRightAssigmentIdentifierName()?.ToUpperInvariant() == parameterName.ToUpperInvariant())
            {
                propertyName = constructorDeclaration.ExpressionBody?.Expression.As<AssignmentExpressionSyntax>()?.GetLeftAssigmentIdentifierName();
            }

            return propertyName;
        }

        //TODO: переделать эти методы (2), потому что нихуя не очевидно
        public static Constructor? GetOptimalConstructor(this Type from, Type to, IEnumerable<string> byUser)
        {
            var constructors = to.Constructors.OrderByDescending(x => x.Parameters.Count());
            if (constructors.Count() == 0)
            {
                throw new System.ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Complement(byUser)
                .Complement(from.GetPropertiesNames())
                .Complement(from.GetFlattenPropertiesNames())
                .IsEmpty());

            var unflattenConstructor = constructors.FirstOrDefault(x => x
                    .GetFlattenParametersNames()
                    .Complement(byUser)
                    .Complement(from.GetPropertiesNames())
                    .IsEmpty());

            return constructor.Parameters.Count() > unflattenConstructor.Parameters.Count()
                ? constructor
                : unflattenConstructor;
        }
        public static Constructor? GetOptimalUnflatteningConstructor(this Type from, Type to, string unflattingPropertyName)
        {
            var constructors = to.Constructors.OrderByDescending(x => x.Parameters.Count());
            if (constructors.Count() == 0)
            {
                throw new System.ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Select(y => $"{unflattingPropertyName}{y}")
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            return constructor;
        }

        public static ImmutableArray<string> GetPropertiesNames(this Type type) => type.Properties.Select(x => x.Name).ToImmutableArray();
        public static ImmutableArray<string> GetParametersNames(this Constructor constructor) => constructor.Parameters.Select(x => x.Name).ToImmutableArray();

        //public static ImmutableArray<string> GetInitializerExpressionsLeft(this Initializer initializer) => initializer.Expressions.Select(x => x.Left.Name).ToImmutableArray();

        public static ImmutableArray<string> GetFlattenPropertiesNames(this Type type)
            => type.Properties.SelectMany(x => x.Type.Properties.Select(y => $"{x.Name}{y.Name}")).ToImmutableArray();
        public static ImmutableArray<string> GetFlattenParametersNames(this Constructor constructor)
            => constructor.Parameters.SelectMany(x => x.Type.Properties.Select(y => $"{x.Name}{y.Name}")).ToImmutableArray();

        public static ImmutableArray<Property> GetInitializerProperties(this Constructor constructor)
            => constructor.ConstructedType.Properties.Where(x => !x.IsReadOnly)
            .Where(x => !constructor.GetParametersNames().Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)).ToImmutableArray();

        public static Property? FindProperty(this Type type, string propertyName, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type.Properties.FirstOrDefault(x => x.Name.Equals(propertyName, comparision));

        public static (Property flattenProperty, Property mappedProperty) FindFlattenMappedProperty(
            this Type type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type
            .Properties
            .SelectMany(flatten => flatten.Type
                .Properties
                .Where(mapped => $"{flatten.Name}{mapped.Name}".Equals(name, comparision))
                .Select(mapped => (flatten, mapped)))
            .FirstOrDefault();
        public static (Property unflattenProperty, Property mappedProperty) FindUnflattenMappedProperties(
            this Type type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type
            .Properties
            .SelectMany(flatten => flatten.Type
                .Properties
                .Where(mapped => $"{flatten.Name}{mapped.Name}".Equals(name, comparision))
                .Select(mapped => (flatten, mapped)))
            .FirstOrDefault();

        public static IMethodSymbol? GetOptimalUnflattingConstructor(this ITypeSymbol from, ITypeSymbol to, string unflattingPropertyName)
        {
            var constructors = to.GetPublicConstructors().OrderByParametersDesc();
            if (constructors.Count() == 0)
            {
                throw new System.ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Select(y => $"{unflattingPropertyName}{y}")
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            return constructor;
        }

        public static ObjectCreationExpressionSyntax? GetObjectCreateionExpression(this BaseMethodDeclarationSyntax method)
        {
            var objCreationExpression = method.ExpressionBody != null
                ? method.GetExpression<ObjectCreationExpressionSyntax>()
                : method.GetReturnStatement().Expression as ObjectCreationExpressionSyntax;

            return objCreationExpression;
        }

        public static IParameterSymbol GetConstructorParameter(this SemanticModel semanticModel, ArgumentSyntax argument)
        {
            //argument -> argumentList -> method
            if (argument.Parent?.Parent is ObjectCreationExpressionSyntax methodDeclaration
                && semanticModel.GetSymbol(methodDeclaration) is IMethodSymbol method
                && methodDeclaration?.ArgumentList?.Arguments.IndexOf(argument) is int index)
            {
                return method.Parameters[index];
            }
            else
            {
                throw new System.Exception($"Parameter for argument {argument} was not found");
            }
        }

        public static List<string> GetUsingsAndNamespace(this SyntaxNode node)
            => node.GetUsings().Append($"using {node.GetNamespace()};").ToList();

        public static (IPropertySymbol flattenProperty, IPropertySymbol mappedProperty) FindFlattenMappedProperty(
            this ITypeSymbol type, string name, System.StringComparison comparision = System.StringComparison.InvariantCultureIgnoreCase) 
            => type
            .GetProperties()
            .SelectMany(flatten => flatten.Type
                .GetProperties()
                .Where(mapped => $"{flatten.Name}{mapped.Name}".Equals(name, comparision))
                .Select(mapped => (flatten, mapped)))
            .FirstOrDefault();

        public static (IPropertySymbol unflattenProperty, IPropertySymbol mappedProperty) FindUnflattenMappedProperties(
            this ITypeSymbol type, string name, System.StringComparison comparision = System.StringComparison.InvariantCultureIgnoreCase)
            => type
            .GetProperties()
            .SelectMany(flatten => flatten.Type
                .GetProperties()
                .Where(mapped => $"{flatten.Name}{mapped.Name}".Equals(name, comparision))
                .Select(mapped => (flatten, mapped)))
            .FirstOrDefault();

        public static List<string> GetFlattenPropertiesNames(this ITypeSymbol type)
            => type.GetProperties().SelectMany(x => x.Type.GetProperties().Select(y => $"{x.Name}{y.Name}")).ToList();

        public static List<string> GetFlattenParametersNames(this IMethodSymbol method)
            => method.GetParameters().SelectMany(x => x.Type.GetProperties().Select(y => $"{x.Name}{y.Name}")).ToList();

        public static IParameterSymbol? FindUnflattenParameter(this IMethodSymbol constructor, string name, IPropertySymbol unflattingProperty, System.StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => constructor?.Parameters.FirstOrDefault(x => $"{unflattingProperty.Name}{x.Name}".Equals(name, comparision));

        public static IPropertySymbol? FindSettableUnflattenProperty(this ITypeSymbol type, string name, IPropertySymbol unflattingProperty, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetSettableProperties().FirstOrDefault(x => $"{unflattingProperty.Name}{x.Name}".Equals(name, comparision));
        public static IPropertySymbol? FindUnflattenProperty(this ITypeSymbol type, string name, IPropertySymbol unflattingProperty, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetProperties().FirstOrDefault(x => $"{unflattingProperty.Name}{x.Name}".Equals(name, comparision));

        public static List<ISymbol> GetConstructorInitializerMembers(this IMethodSymbol constructor)
        {
            IEnumerable<ISymbol> constructorParameters = constructor.GetParameters();
            var initializerProperties = constructor.ContainingType
                .GetSettableProperties()
                .Where(x => !constructor.GetParametersNames().Contains(x.Name, StringComparer.InvariantCultureIgnoreCase));
            var members = constructorParameters.Concat(initializerProperties).ToList();

            return members;
        }
    }
}
