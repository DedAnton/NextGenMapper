using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis
{
    public class MapMethodInvocation
    {
        public InvocationExpressionSyntax Node { get; }
        public SemanticModel SemanticModel { get; }

        public MapMethodInvocation(InvocationExpressionSyntax node, SemanticModel semanticModel)
        {
            Node = node;
            SemanticModel = semanticModel;
        }
    }
}
