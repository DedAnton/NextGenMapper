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

        public TypeCustomMap DesignMapsForPlanner(MethodDeclarationSyntax method)
        {
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);

            return new TypeCustomMap(from, to, method);
        }
    }
}
