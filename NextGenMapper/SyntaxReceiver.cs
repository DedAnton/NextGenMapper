using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.Extensions;
using System.Linq;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        public MapPlanner Planner = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var semanticModel = context.SemanticModel;
            if (context.Node is ClassDeclarationSyntax classNode
                && semanticModel.GetDeclaredSymbol(classNode).HasAttribute(Annotations.MapperAttributeName))
            {
                HandleCustomMapperClass(context.SemanticModel, classNode);
            }
            else if (context.Node is InvocationExpressionSyntax invocationNode
                && semanticModel.GetSymbol(invocationNode.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom?.ToDisplayString() == StartMapperSource.FunctionFullName
                && invocationNode.Expression is MemberAccessExpressionSyntax memberAccess
                && semanticModel.GetSymbol(memberAccess.Expression) is ILocalSymbol invocatingVariable)
            {
                MapInvocation(semanticModel, invocatingVariable.Type, method.ReturnType);
            }
        }

        private void MapInvocation(SemanticModel semanticModel, ITypeSymbol from, ITypeSymbol to)
        {
            if (from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum)
            {
                var designer = new EnumMapDesigner(semanticModel, Planner);
                designer.DesignMapsForPlanner(from, to);
            }
            else if (from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class)
            {
                var designer = new ClassMapDesigner(semanticModel, Planner);
                designer.DesignMapsForPlanner(from, to);
            }
        }

        private void HandleCustomMapperClass(SemanticModel semanticModelt, ClassDeclarationSyntax node)
        {
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasSingleParameterWithType()))
            {
                if (semanticModelt.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName) is var isPartial
                    && isPartial == true
                    && method.GetObjectCreateionExpression() is { ArgumentList: { Arguments: var arguments } }
                    && arguments.Any(x => x.IsDefaultLiteralExpression()))
                {
                    var partialOneConstructorPlanGenerator = new ClassPartialConstructorMapDesigner(semanticModelt, Planner);
                    partialOneConstructorPlanGenerator.DesignMapsForPlanner(method);
                }
                else if (isPartial)
                {
                    var partialPlanGenerator = new ClassPartialMapDesigner(semanticModelt, Planner);
                    partialPlanGenerator.DesignMapsForPlanner(method);
                }
                else
                {
                    var customPlanGenerator = new TypeCustomMapDesigner(semanticModelt, Planner);
                    customPlanGenerator.DesignMapsForPlanner(method);
                }
            }
        }
    }
}
