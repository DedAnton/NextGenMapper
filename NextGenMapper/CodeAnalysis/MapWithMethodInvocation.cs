using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;

public class MapWithMethodInvocation
{
    public InvocationExpressionSyntax Node { get; }
    public SemanticModel SemanticModel { get; }
    public ArgumentSyntax[] Arguments { get; }

    public MapWithMethodInvocation(InvocationExpressionSyntax node, SemanticModel semanticModel, ArgumentSyntax[] arguments)
    {
        Node = node;
        SemanticModel = semanticModel;
        Arguments = arguments;
    }
}
