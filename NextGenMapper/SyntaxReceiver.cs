using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;

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
                HandleCustomMapperClass(context, classNode);
  
            }
            else if (context.Node is InvocationExpressionSyntax invocationNode
                && context.GetSymbol(invocationNode.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom.ToDisplayString() == StartMapperSource.FunctionFullName)
            {
                HandleStartMapperInvocation(context, invocationNode, method);
            }
        }

        private void HandleStartMapperInvocation(
            GeneratorSyntaxContext context, InvocationExpressionSyntax node, IMethodSymbol method)
        {
            var commonPlanGenerator = new CommonPlanGenerator(context.SemanticModel, Planner);
            commonPlanGenerator.GenerateMappingsForPlanner(node);
        }

        private void HandleCustomMapperClass(GeneratorSyntaxContext context, ClassDeclarationSyntax node)
        {
            foreach (var method in node.GetMethodsDeclarations())
            {
                if (method.HasOneParameter())
                {
                    if (context.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName))
                    {
                        var partialPlanGenerator = new PartialPlanGenerator(context.SemanticModel, Planner);
                        partialPlanGenerator.GenerateMappingsForPlanner(method);
                    }
                    else
                    {
                        var customPlanGenerator = new CustomPlanGenerator(context.SemanticModel, Planner);
                        customPlanGenerator.GenerateMappingsForPlanner(method);
                    }
                }
            }
        }
    }
}
