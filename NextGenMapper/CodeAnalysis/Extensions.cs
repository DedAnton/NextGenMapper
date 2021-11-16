using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public static class Extensions
    {
        public static IMethodSymbol? GetOptimalConstructor(
            this ITypeSymbol from, ITypeSymbol to, IEnumerable<string>? byUser = null)
        {
            byUser ??= new List<string>();
            var constructors = to.GetPublicConstructors().OrderByParametersDesc();
            if (constructors.Count == 0)
            {
                return null;
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Complement(byUser)
                .Complement(from.GetPropertiesNames())
                .Complement(from.GetFlattenPropertiesNames())
                .IsEmpty());

            var unflattenConstructor = constructors.FirstOrDefault(x => x
                .Parameters.Where(y => from.GetOptimalUnflatteningConstructor(y.Type, y.Name) == null)
                .Select(x => x.Name)
                .Complement(byUser)
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            return constructor.GetParametersCount() > unflattenConstructor.GetParametersCount()
                ? constructor
                : unflattenConstructor;
        }

        public static IMethodSymbol? GetOptimalUnflatteningConstructor(
            this ITypeSymbol from, ITypeSymbol to, string unflattingPropertyName)
        {
            var constructors = to.GetPublicConstructors().OrderByParametersDesc();
            if (constructors.IsEmpty())
            {
                //TODO: сделать отдельный сервси для нахождения оптимальных кострукторов, куда прокидывать DiagnosticReporter
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            var constructor = constructors.FirstOrDefault(x => x
                .GetParametersNames()
                .Select(y => $"{unflattingPropertyName}{y}")
                .Complement(from.GetPropertiesNames())
                .IsEmpty());

            var flattenProperties = to.GetPropertiesNames().Select(x => $"{unflattingPropertyName}{x}");
            var isUnflattening = from.GetPropertiesNames().Any(x => flattenProperties.Contains(x, StringComparer.InvariantCultureIgnoreCase));
            if (!isUnflattening)
            {
                return null;
            }

            return constructor;
        }

        public static ObjectCreationExpressionSyntax? GetObjectCreateionExpression(this BaseMethodDeclarationSyntax method)
        {
            var objCreationExpression = method.ExpressionBody != null
                ? method.GetExpression<ObjectCreationExpressionSyntax>()
                : method.GetReturnStatement().Expression as ObjectCreationExpressionSyntax;

            return objCreationExpression;
        }

        public static IParameterSymbol GetConstructorParameter(this IMethodSymbol constructor, ArgumentSyntax argument)
        {
            //argument -> argumentList -> method
            if (argument.Parent?.Parent is ObjectCreationExpressionSyntax methodDeclaration
                && methodDeclaration?.ArgumentList?.Arguments.IndexOf(argument) is int index)
            {
                return constructor.Parameters[index];
            }
            else
            {
                //TODO: вынести в класс еоторый будет искать оптимальный коструктор и заменить на диагностику (или вообще убрать)
                throw new Exception($"Parameter for argument {argument} was not found");
            }
        }

        public static List<string> GetUsingsAndNamespace(this SyntaxNode node)
            => node.GetUsings().Append($"using {node.GetNamespace()};").ToList();

        public static (IPropertySymbol flattenProperty, IPropertySymbol mappedProperty) FindFlattenMappedProperty(
            this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type
            .GetProperties()
            .SelectMany(flatten => flatten.Type
                .GetProperties()
                .Where(mapped => $"{flatten.Name}{mapped.Name}".Equals(name, comparision))
                .Select(mapped => (flatten, mapped)))
            .FirstOrDefault();

        public static List<string> GetFlattenPropertiesNames(this ITypeSymbol type)
            => type.GetProperties().SelectMany(x => x.Type.GetProperties().Select(y => $"{x.Name}{y.Name}")).ToList();

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
