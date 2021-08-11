using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis
{
    public class MapperClassDeclaration
    {
        public ClassDeclarationSyntax Node { get; }
        public SemanticModel SemanticModel { get; }

        public MapperClassDeclaration(ClassDeclarationSyntax node, SemanticModel semanticModel)
        {
            Node = node;
            SemanticModel = semanticModel;
        }
    }
}