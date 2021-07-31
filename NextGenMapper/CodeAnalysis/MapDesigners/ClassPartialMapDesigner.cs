using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class ClassPartialMapDesigner
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MapPlanner _planner;

        public ClassPartialMapDesigner(SemanticModel semanticModel, MapPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void DesignMapsForPlanner(MethodDeclarationSyntax method)
        {
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);
            var objCreationExpression = method.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                throw new ArgumentException($"Error when create mapping for method \"{method}\", object creation expression was not found. Partial methods must end with object creation like \"return new Class()\"");
            }
            var byConstructor = _semanticModel.GetMethodSymbol(objCreationExpression)?.GetParametersNames();
            var byInitialyzer = objCreationExpression.GetInitializersLeft();
            var byUser = byConstructor?.Union(byInitialyzer);
            var constructor = from.GetOptimalConstructor(to, byUser);
            if (constructor == null)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} does not have a suitable constructor");
            }

            var classMapDesigner = new ClassMapDesigner(_semanticModel, _planner);
            var membersMaps = new List<IMemberMap>();
            foreach (var toProperty in to.GetProperties())
            {
                var isProvidedByUser = byUser.Contains(toProperty.Name, StringComparer.InvariantCultureIgnoreCase);
                var toConstructor = constructor.FindParameter(toProperty.Name);
                var toInitializer = to.FindSettableProperty(toProperty.Name);
                var fromProperty = from.FindProperty(toProperty.Name);

                IMemberMap? map = (fromProperty, toConstructor, toInitializer, isProvidedByUser, UseInitializer) switch
                {
                    (_, { }, _, true, _) => new ParameterMap(toProperty, toConstructor, isProvidedByUser),
                    (_, _, { }, true, true) => new PropertyMap(toInitializer, toInitializer, isProvidedByUser),
                    ({ }, { }, _, false, _) => new ParameterMap(fromProperty, toConstructor, isProvidedByUser),
                    ({ }, _, { }, false, true) => new PropertyMap(fromProperty, toInitializer, isProvidedByUser),
                    _ => null
                };
                membersMaps.AddIfNotNull(map);

                if (map is { IsSameTypes: false }
                    && !map.IsProvidedByUser)
                {
                    classMapDesigner.DesignMapsForPlanner(map.TypeFrom, map.TypeTo);
                }
            }

            _planner.AddCustomMap(new ClassPartialMap(from, to, membersMaps, method), method.GetUsingsAndNamespace());
        }
    }
}
