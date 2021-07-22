using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;


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
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);
            var objCreationExpression = method.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                throw new ArgumentException($"Error when create mapping for method \"{method}\", object creation expression was not found. Partial methods must end with object creation like \"return new Class()\"");
            }
            var constructor = _semanticModel.GetMethodSymbol(objCreationExpression);
            var byConstructor = constructor.GetParametersNames();
            var byInitialyzer = objCreationExpression.GetInitializersLeft();
            var byUser = byConstructor.Union(byInitialyzer);
            var sourceParameter = _semanticModel.GetDeclaredSymbol(method).Parameters.Single();

            var newArguments = new List<ArgumentSyntax>();
            foreach (var argument in objCreationExpression?.ArgumentList?.Arguments)
            {
                if (argument.IsDefaultLiteralExpression())
                {
                    var parameter = _semanticModel.GetConstructorParameter(argument);
                    var fromProperty = from.FindProperty(parameter.Name);
                    newArguments.Add(GenerateMeberAccessArgument(sourceParameter.Name, fromProperty.Name));
                }
                else
                {
                    newArguments.Add(argument);
                }
            }

            var newInitializers = new List<ExpressionSyntax>();
            newInitializers.AddRange(objCreationExpression.Initializer?.Expressions);
            if (UseInitializer)
            {
                var forInitializer = to.GetSettableProperties().Where(x => !byUser.Contains(x.Name));
                foreach (var property in forInitializer)
                {
                    if (from.FindProperty(property.Name) is { } fromProperty)
                    {
                        var initExpr = property.Type.Equals(fromProperty.Type, SymbolEqualityComparer.IncludeNullability)
                            ? GenerateInitializerAssignmentExpression(property.Name, sourceParameter.Name, fromProperty.Name)
                            : GenerateInitializerAssignmentExpressionWithMap(property.Name, property.Type.ToDisplayString(), sourceParameter.Name, fromProperty.Name);
                        newInitializers.Add(initExpr);
                    }
                }
            }

            var newObjCreationExpression = GenerateObjectCreationExpression(to.Name, newArguments, newInitializers);

            var newMethod = GenerateMethod(to.Name, sourceParameter.Name, from.Name);
            if (method.ExpressionBody != null)
            {
                newMethod = newMethod
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(newObjCreationExpression))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                var returnStatement = method.GetReturnStatement();
                var newStatements = method.Body.Statements.Remove(returnStatement);
                newStatements = newStatements.Add(SyntaxFactory.ReturnStatement(newObjCreationExpression));
                newMethod = newMethod.WithBody(SyntaxFactory.Block(newStatements));
            }
            newMethod = newMethod.NormalizeWhitespace();

            _planner.AddMapping(TypeMapping.CreatePartialConstructor(from, to, newMethod), method.GetUsings());
        }

        private ArgumentSyntax GenerateMeberAccessArgument(string member, string property) => 
            SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(member),
                SyntaxFactory.IdentifierName(property)));

        private ExpressionSyntax GenerateInitializerAssignmentExpression(string leftProperty, string rightMember, string rightProperty) =>
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(leftProperty),
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(rightMember),
                    SyntaxFactory.IdentifierName(rightProperty)));

        private ExpressionSyntax GenerateInitializerAssignmentExpressionWithMap(string leftProperty, string leftPropertyType, string rightMember, string rightProperty) =>
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(leftProperty),
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(rightMember),
                            SyntaxFactory.IdentifierName(rightProperty)),
                        SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("Map"))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(leftPropertyType)))))));

        private ObjectCreationExpressionSyntax GenerateObjectCreationExpression(
            string createdType, List<ArgumentSyntax> arguments, List<ExpressionSyntax> initializerExpressions) =>
            SyntaxFactory
                .ObjectCreationExpression(SyntaxFactory.IdentifierName(createdType))
                .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)))
                .WithInitializer(SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList(SyntaxFactory.SeparatedList(initializerExpressions))));

        private MethodDeclarationSyntax GenerateMethod(string returnType, string parameterName, string parameterType) =>
            SyntaxFactory.MethodDeclaration(
                SyntaxFactory.IdentifierName(returnType),
                SyntaxFactory.Identifier("Map"))
            .WithModifiers(
                SyntaxFactory.TokenList(new[]{
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)}))
            .WithTypeParameterList(
                SyntaxFactory.TypeParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.TypeParameter(SyntaxFactory.Identifier("To")))))
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                            .WithType(SyntaxFactory.IdentifierName(parameterType))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword))))));
    }
}
