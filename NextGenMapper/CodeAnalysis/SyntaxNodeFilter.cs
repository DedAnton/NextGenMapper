using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;

internal static class SyntaxNodeFilter
{
    public static bool IsUserMapMethodDeclaration(SyntaxNode node)
    => node is MethodDeclarationSyntax
    {
        Identifier.ValueText: "Map",
        ParameterList.Parameters.Count: 1,
        Parent: ClassDeclarationSyntax
        {
            Arity: 0,
            Identifier.ValueText: "Mapper",
            Parent: NamespaceDeclarationSyntax
            {
                Name: IdentifierNameSyntax
                {
                    Identifier.ValueText: "NextGenMapper"
                }
            }
                or FileScopedNamespaceDeclarationSyntax
            {
                Name: IdentifierNameSyntax
                {
                    Identifier.ValueText: "NextGenMapper"
                }
            }
        }
    };

    public static bool IsMapMethodInvocation(SyntaxNode node) => IsGenericMethodInvocation(node, "Map");

    public static bool IsConfiguredMapMethodInvocation(SyntaxNode node) => IsGenericMethodInvocation(node, "MapWith");

    public static bool IsProjectionMethodInvocation(SyntaxNode node) => IsGenericMethodInvocation(node, "Project");

    public static bool IsConfiguredProjectionMethodInvocation(SyntaxNode node) => IsGenericMethodInvocation(node, "ProjectWith");

    private static bool IsGenericMethodInvocation(SyntaxNode node, string mapMethodName)
    => node is InvocationExpressionSyntax
    {
        Expression: MemberAccessExpressionSyntax
        {
            Name: GenericNameSyntax genericNameSyntax
        }
    }
    && genericNameSyntax.Identifier.ValueText == mapMethodName;
}
