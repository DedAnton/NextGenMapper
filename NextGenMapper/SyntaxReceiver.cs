using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.PostInitialization;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        private const string MAP_METHOD_NAME = "Map";
        public List<MapMethodInvocation> MapMethodInvocations { get; } = new();
        public List<MapperClassDeclaration> MapperClassDeclarations { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classNode
                && classNode.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == Annotations.MapperAttributeShortName || x.Name.ToString() == Annotations.MapperAttributeName))
            {
                var mapperClasDeclaration = new MapperClassDeclaration(classNode, context.SemanticModel);
                MapperClassDeclarations.Add(mapperClasDeclaration);
            }
            else if (context.Node is InvocationExpressionSyntax invocationNode
                && invocationNode.Expression is MemberAccessExpressionSyntax memberAccessExpression
                && memberAccessExpression.Name is GenericNameSyntax
                && memberAccessExpression.Name.Identifier.ToString() == MAP_METHOD_NAME)
            {
                var mapMethodInvocation = new MapMethodInvocation(invocationNode, context.SemanticModel);
                MapMethodInvocations.Add(mapMethodInvocation);
            }
        }
    }
}
