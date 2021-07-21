using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;

namespace NextGenMapper.CodeAnalysis
{
    public class CustomPlanGenerator
    {
        private readonly SemanticModel _semanticModel;
        private readonly MappingPlanner _planner;

        public CustomPlanGenerator(SemanticModel semanticModel, MappingPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void GenerateMappingsForPlanner(MethodDeclarationSyntax method)
        {
            CreateMapping(method);
        }

        private void CreateMapping(MethodDeclarationSyntax method)
        {
            var parameter = method.GetSingleParameter();
            var from = _semanticModel.GetTypeSymbol(parameter.Type);
            var to = _semanticModel.GetTypeSymbol(method.ReturnType);

            var mapping = TypeMapping.CreateCustom(from, to, method);
            _planner.AddMapping(mapping, method.GetUsings());
        }
    }
}
