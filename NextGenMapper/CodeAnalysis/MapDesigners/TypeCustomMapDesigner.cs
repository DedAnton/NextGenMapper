using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class TypeCustomMapDesigner
    {
        private readonly SemanticModel _semanticModel;
        private readonly MapPlanner _planner;

        public TypeCustomMapDesigner(SemanticModel semanticModel, MapPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void DesignMapsForPlanner(MethodDeclarationSyntax method)
        {
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);
            var map = new TypeCustomMap(from, to, method);

            _planner.AddCustomMap(map, method.GetUsingsAndNamespace());
        }
    }
}
