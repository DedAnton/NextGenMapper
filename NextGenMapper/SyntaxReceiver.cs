using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;
using System.Linq;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        public MappingPlanner Planner = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classNode
                && context.GetDeclaredSymbol(classNode).HasAttribute(Annotations.MapperAttributeName))
            {
                HandleCustomMapperClass(context.SemanticModel, classNode);
  
            }
            else if (context.Node is InvocationExpressionSyntax invocationNode
                && context.GetSymbol(invocationNode.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom.ToDisplayString() == StartMapperSource.FunctionFullName)
            {
                HandleStartMapperInvocation(context.SemanticModel, invocationNode);
            }
        }

        private void HandleStartMapperInvocation(SemanticModel semanticModel, InvocationExpressionSyntax node)
        {
            var commonPlanGenerator = new CommonPlanGenerator(semanticModel, Planner);
            commonPlanGenerator.GenerateMappingsForPlanner(node);
        }

        private void HandleCustomMapperClass(SemanticModel semanticModelt, ClassDeclarationSyntax node)
        {
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasOneParameter()))
            {
                if (semanticModelt.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName) is var isPartial
                    && method.GetObjectCreateionExpression() is { ArgumentList: { Arguments: var arguments } }
                    && arguments.Any(x => x.IsDefaultLiteralExpression()))
                {
                    var partialOneConstructorPlanGenerator = new PartialOneConstructorPlanGenerator(semanticModelt, Planner);
                    partialOneConstructorPlanGenerator.GenerateMappingsForPlanner(method);
                }
                else if (isPartial)
                {
                    var partialPlanGenerator = new PartialPlanGenerator(semanticModelt, Planner);
                    partialPlanGenerator.GenerateMappingsForPlanner(method);
                }
                else
                {
                    var customPlanGenerator = new CustomPlanGenerator(semanticModelt, Planner);
                    customPlanGenerator.GenerateMappingsForPlanner(method);
                }
            }
        }
    }
}
