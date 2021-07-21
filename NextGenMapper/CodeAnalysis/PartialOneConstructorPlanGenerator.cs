using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NextGenMapper.CodeAnalysis
{
    public class PartialOneConstructorPlanGenerator
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MappingPlanner _planner;

        public PartialOneConstructorPlanGenerator(SemanticModel semanticModel, MappingPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void GenerateMappingsForPlanner(MethodDeclarationSyntax method)
        {
            Crteasdasd(method);
        }

        private void CreateMapping(ITypeSymbol from, ITypeSymbol to)
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

            var propertyMappings = new List<PropertyMapping>();
            foreach (var fromProperty in from.GetProperties())
            {
                var propertyMapping = fromProperty.Name switch
                {
                    var fromName when
                        constructor.FindParameter(fromName) is var toParameter
                        && toParameter is not null => new PropertyMapping(fromProperty, toParameter),

                    var fromName when UseInitializer
                        && to.FindSettableProperty(fromName) is var toProperty
                        && toProperty is not null => new PropertyMapping(fromProperty, toProperty),

                    _ => null
                };

                switch (propertyMapping?.IsSameTypes)
                {
                    case true: _planner.AddMapping(TypeMapping.CreateCommon(from, to, propertyMappings)); break;
                    case false: CreateMapping(propertyMapping.TypeFrom, propertyMapping.TypeTo); break;
                }
            }
        }

        private void Crteasdasd(MethodDeclarationSyntax method)
        {
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);

            ObjectCreationExpressionSyntax objCreationExpression =
                method.ExpressionBody != null
                ? method.ExpressionBody.Expression as ObjectCreationExpressionSyntax
                : (method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax).Expression as ObjectCreationExpressionSyntax;

            var hasDefault = objCreationExpression.ArgumentList.Arguments.Any(x => x.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression);

            var def = objCreationExpression.ArgumentList.Arguments.FirstOrDefault(x => x.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression);

            //objCreationExpression.ArgumentList.Arguments.Remove(def);

            var arguments = new SyntaxNodeOrToken[1];
            var list = SyntaxFactory.SeparatedList<ArgumentSyntax>();
            foreach (var argument in objCreationExpression.ArgumentList.Arguments)
            {
                if (argument.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression)
                {

                }
                else
                {
                    list = list.Add(argument);
                    arguments[0] = argument;
                }
            }

            var newObjCreationExpression = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("HorseModel"))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(list));

            var returnStatement = (method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax);

            var dfh = newObjCreationExpression.NormalizeWhitespace().ToString();
            var newStatements = method.Body.Statements.Remove(returnStatement);
            newStatements = newStatements.Add(SyntaxFactory.ReturnStatement(newObjCreationExpression));

            var lkj = SyntaxFactory.MethodDeclaration(
    SyntaxFactory.IdentifierName(to.Name),
    SyntaxFactory.Identifier($"Map<{to.Name}>"))
.WithModifiers(
    SyntaxFactory.TokenList(
        SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
.WithParameterList(
    SyntaxFactory.ParameterList(
        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
            SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(method.GetSingleParameter().Identifier.Text))
            .WithType(
                SyntaxFactory.IdentifierName(method.GetSingleParameter().Type.ToString())))))
.WithBody(
    SyntaxFactory.Block(newStatements))
.NormalizeWhitespace().ToString();
            var qwe = lkj;


            var constructors = to.GetPublicConstructors().OrderByParametersDesc();
            if (constructors.Count() == 0)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }
            var constructor = constructors.FirstOrDefault(x =>
                x.GetParametersNames().ToUpperInvariant()
                .Complement(from.GetPropertiesNames().ToUpperInvariant())
                .IsEmpty());


        }

        private List<TypeMapping> CreatePartialMappings(GeneratorSyntaxContext context, MethodDeclarationSyntax method)
        {
            ObjectCreationExpressionSyntax objCreationExpression =
                method.ExpressionBody != null
                ? method.ExpressionBody.Expression as ObjectCreationExpressionSyntax
                : (method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax).Expression as ObjectCreationExpressionSyntax;
            var hasDefault = objCreationExpression.ArgumentList.Arguments.Where(x => x.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression);
            var byConstructor = (context.GetSymbol(objCreationExpression) as IMethodSymbol).Parameters.Select(x => x.Name.ToUpperInvariant());
            var byInitialyzer = objCreationExpression.GetInitializersLeft();

            var parameter = method.GetSingleParameter();
            var from = context.GetTypeSymbol(parameter.Type);
            var to = context.GetTypeSymbol(method.ReturnType);

            var propertiesMappings = CreatePropertiesMappings(from, to);
            propertiesMappings.RemoveAll(x => byInitialyzer.Contains(x.NameFrom));
            propertiesMappings.RemoveAll(x => byConstructor.Contains(x.NameFrom.ToUpperInvariant()));
            var mappings = CreateCommonMappingsForProperties(propertiesMappings);
            var mapping = TypeMapping.CreatePartial(from, to, propertiesMappings, method);
            mappings.Add(mapping);

            return mappings;
        }

        private List<PropertyMapping> CreatePropertiesMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var propertiesMappings = new List<PropertyMapping>();

            foreach (var fromProperty in from.GetProperties())
            {
                var toProperty = to.GetProperties().FirstOrDefault(x => x.Name == fromProperty.Name);
                if (toProperty != null)
                {
                    propertiesMappings.Add(new PropertyMapping(fromProperty, toProperty));
                }
            }

            return propertiesMappings;
        }

        private List<TypeMapping> CreateCommonMappingsForProperties(List<PropertyMapping> propertiesMappings)
        {
            var mappings = new List<TypeMapping>();
            foreach (var propertyMapping in propertiesMappings)
            {
                if (!propertyMapping.IsSameTypes)
                {
                    mappings.AddRange(CreateCommonMappings(propertyMapping.TypeFrom, propertyMapping.TypeTo));
                }
            }

            return mappings;
        }

        private List<TypeMapping> CreateCommonMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var constructorProperties = GetConstructorPropertiesMappings(from, to);
            var isConstructorMapping = constructorProperties.Count > 0;
            var propertiesMappings = isConstructorMapping
                ? constructorProperties
                : CreatePropertiesMappings(from, to);
            var mappings = CreateCommonMappingsForProperties(propertiesMappings);
            var mapping = TypeMapping.CreateCommon(from, to, propertiesMappings);
            mappings.Add(mapping);

            return mappings;
        }

        private List<PropertyMapping> GetConstructorPropertiesMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var constructors = (to as INamedTypeSymbol)
                .Constructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(x => x.Parameters.Count());
            if (constructors.Count() == 0)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            foreach (var constructor in constructors)
            {
                var propertiesMappings = new List<PropertyMapping>();

                foreach (var parameter in constructor.Parameters)
                {
                    var fromProperty = from.GetProperties().FirstOrDefault(x => x.Name.ToUpperInvariant() == parameter.Name.ToUpperInvariant());
                    if (fromProperty != null)
                    {
                        propertiesMappings.Add(new PropertyMapping(fromProperty, parameter));
                    }
                }

                if (propertiesMappings.Count() == constructor.Parameters.Count())
                {
                    return propertiesMappings;
                }
            }

            return new List<PropertyMapping>();
        }
    }
}
