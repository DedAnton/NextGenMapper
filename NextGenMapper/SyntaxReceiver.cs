using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.PostInitialization;
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

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
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
