﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    class PartialPlanGenerator
    {
        private const bool UseInitializer = true;

        private readonly SemanticModel _semanticModel;
        private readonly MappingPlanner _planner;

        public PartialPlanGenerator(SemanticModel semanticModel, MappingPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void GenerateMappingsForPlanner(MethodDeclarationSyntax method)
        { 
            var (to, from) = _semanticModel.GetReturnAndParameterType(method);
            var constructor = from.GetOptimalConstructor(to);
            var objCreationExpression = method.GetObjectCreateionExpression();
            if (objCreationExpression == null)
            {
                throw new ArgumentException($"Error when create mapping for method \"{method}\", object creation expression was not found. Partial methods must end with object creation like \"return new Class()\"");
            }
            var byConstructor = _semanticModel.GetMethodSymbol(objCreationExpression).GetParametersNames();
            var byInitialyzer = objCreationExpression.GetInitializersLeft();
            var byUser = byConstructor.Union(byInitialyzer);

            var commonPlanGenerator = new CommonPlanGenerator(_semanticModel, _planner);
            var propertyMappings = new List<PropertyMapping>();
            //foreach (var fromProperty in from.GetProperties())
            //{
            //    var isProvidedByUser = byUser.Contains(fromProperty.Name, StringComparer.InvariantCultureIgnoreCase);
            //    var toConstructor = constructor.FindParameter(fromProperty.Name);
            //    var toInitializer = to.FindSettableProperty(fromProperty.Name);

            //    var propertyMapping = (toConstructor, toInitializer, isProvidedByUser, UseInitializer) switch
            //    {
            //        ({ }, _, true, _) => new PropertyMapping(fromProperty, toConstructor, isProvidedByUser),
            //        (_, { }, true, true) => new PropertyMapping(toInitializer, toInitializer, isProvidedByUser),
            //        ({ }, _, false, _) => new PropertyMapping(fromProperty, toConstructor, isProvidedByUser),
            //        (_, { }, false, true) => new PropertyMapping(fromProperty, toInitializer, isProvidedByUser),
            //        _ => null
            //    };
            //    propertyMappings.AddIfNotNull(propertyMapping);

            //    if (propertyMapping is { IsSameTypes: false }
            //        && !propertyMapping.IsPrimitveTypesMapping()
            //        && !propertyMapping.IsProvidedByUser)
            //    {
            //        commonPlanGenerator.GenerateMappingsForPlanner(propertyMapping.TypeFrom, propertyMapping.TypeTo);
            //    }
            //}
            foreach(var toProperty in to.GetProperties())
            {
                var isProvidedByUser = byUser.Contains(toProperty.Name, StringComparer.InvariantCultureIgnoreCase);
                var toConstructor = constructor.FindParameter(toProperty.Name);
                var toInitializer = to.FindSettableProperty(toProperty.Name);
                var fromProperty = from.FindProperty(toProperty.Name);

                var propertyMapping = (fromProperty, toConstructor, toInitializer, isProvidedByUser, UseInitializer) switch
                {
                    (_, { }, _, true, _) => new PropertyMapping(toProperty, toConstructor, isProvidedByUser),
                    (_, _, { }, true, true) => new PropertyMapping(toInitializer, toInitializer, isProvidedByUser),
                    ({ }, { }, _, false, _) => new PropertyMapping(fromProperty, toConstructor, isProvidedByUser),
                    ({ }, _, { }, false, true) => new PropertyMapping(fromProperty, toInitializer, isProvidedByUser),
                    _ => null
                };
                propertyMappings.AddIfNotNull(propertyMapping);

                if (propertyMapping is { IsSameTypes: false }
                    && !propertyMapping.IsPrimitveTypesMapping()
                    && !propertyMapping.IsProvidedByUser)
                {
                    commonPlanGenerator.GenerateMappingsForPlanner(propertyMapping.TypeFrom, propertyMapping.TypeTo);
                }
            }

            _planner.AddMapping(TypeMapping.CreatePartial(from, to, propertyMappings, method), method.GetUsings());
        }
    }
}