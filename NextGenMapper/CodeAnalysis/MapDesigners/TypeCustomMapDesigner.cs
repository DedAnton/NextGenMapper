using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class TypeCustomMapDesigner
    {
        private readonly SemanticModel _semanticModel;

        public TypeCustomMapDesigner(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public TypeCustomMap DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, MethodDeclarationSyntax method)
        {
            return new TypeCustomMap(from, to, method);
        }
    }
}
