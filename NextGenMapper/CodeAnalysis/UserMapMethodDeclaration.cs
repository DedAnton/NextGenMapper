using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;
public class UserMapMethodDeclaration
{
    public UserMapMethodDeclaration(MethodDeclarationSyntax node, SemanticModel semanticModel)
    {
        Node = node;
        SemanticModel = semanticModel;
    }

    public MethodDeclarationSyntax Node { get; }
    public SemanticModel SemanticModel { get; }
}
