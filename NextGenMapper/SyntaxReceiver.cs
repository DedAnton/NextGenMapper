using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    internal partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        private const string MAP_METHOD_NAME = "Map";
        private const string MAP_WITH_METHOD_NAME = "MapWith";
        public List<MapMethodInvocation> MapMethodInvocations { get; } = new();
        public List<MapWithMethodInvocation> MapWithMethodInvocations { get; } = new();
        public List<UserMapMethodDeclaration> UserMapMethods { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is MethodDeclarationSyntax
                {
                    Arity: 1,
                    Identifier.ValueText: "Map",
                    ParameterList.Parameters.Count: 1,
                    Parent: ClassDeclarationSyntax
                    {
                        Arity: 0,
                        Identifier.ValueText: "Mapper",
                        Modifiers.Count: 3,
                        Parent: NamespaceDeclarationSyntax
                        {
                            Name: IdentifierNameSyntax
                            {
                                Identifier.ValueText: "NextGenMapper"
                            }
                        }
                    } classDeclarationSyntax,
                } methodDeclarationSyntax
                && classDeclarationSyntax.Modifiers[0].ValueText == "internal"
                && classDeclarationSyntax.Modifiers[1].ValueText == "static"
                && classDeclarationSyntax.Modifiers[2].ValueText == "partial"
                && methodDeclarationSyntax.ParameterList.Parameters[0].Modifiers.Count == 1
                && methodDeclarationSyntax.ParameterList.Parameters[0].Modifiers[0].ValueText == "this")
            {
                var userMapMethodDeclaration = new UserMapMethodDeclaration(methodDeclarationSyntax, context.SemanticModel);
                UserMapMethods.Add(userMapMethodDeclaration);
            }

            if (context.Node is InvocationExpressionSyntax invocationNode
                && invocationNode.Expression is MemberAccessExpressionSyntax memberAccessExpression
                && memberAccessExpression.Name is GenericNameSyntax)
            {
                if (memberAccessExpression.Name.Identifier.ToString() == MAP_METHOD_NAME)
                {
                    var mapMethodInvocation = new MapMethodInvocation(invocationNode, context.SemanticModel);
                    MapMethodInvocations.Add(mapMethodInvocation);
                }
                else if (memberAccessExpression.Name.Identifier.ToString() == MAP_WITH_METHOD_NAME)
                {
                    var mapWithMethodInvocation = new MapWithMethodInvocation(invocationNode, context.SemanticModel, invocationNode.ArgumentList.Arguments.ToArray());
                    MapWithMethodInvocations.Add(mapWithMethodInvocation);
                }
            }
        }
    }
}
