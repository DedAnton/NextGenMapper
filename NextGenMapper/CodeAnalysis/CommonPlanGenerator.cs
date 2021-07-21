using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis
{
    public class CommonPlanGenerator
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MappingPlanner _planner;

        public CommonPlanGenerator(SemanticModel semanticModel, MappingPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void GenerateMappingsForPlanner(InvocationExpressionSyntax invocation)
        {
            var method = _semanticModel.GetMethodSymbol(invocation.Expression);
            var member = _semanticModel.GetSymbol(invocation.Expression.As<MemberAccessExpressionSyntax>().Expression).As<ILocalSymbol>();

            CreateMapping(member.Type, method.ReturnType);
        }

        public void GenerateMappingsForPlanner(ITypeSymbol from, ITypeSymbol to) => CreateMapping(from, to);

        private void CreateMapping(ITypeSymbol from, ITypeSymbol to)
        {
            var constructor = from.GetOptimalConstructor(to);

            var propertyMappings = new List<PropertyMapping>();
            foreach(var fromProperty in from.GetProperties())
            {
                var toConstructor = constructor.FindParameter(fromProperty.Name);
                var toInitializer = to.FindSettableProperty(fromProperty.Name);

                var propertyMapping = (toConstructor, toInitializer, UseInitializer) switch
                {
                    ({ }, _, _) => new PropertyMapping(fromProperty, toConstructor),
                    (_, { }, true) => new PropertyMapping(fromProperty, toInitializer),
                    _ => null
                };
                propertyMappings.AddIfNotNull(propertyMapping);

                if (propertyMapping is { IsSameTypes: false}
                    && !propertyMapping.IsPrimitveTypesMapping())
                {
                    CreateMapping(propertyMapping.TypeFrom, propertyMapping.TypeTo);
                }
            }
            _planner.AddMapping(TypeMapping.CreateCommon(from, to, propertyMappings));
        }
    }
}
