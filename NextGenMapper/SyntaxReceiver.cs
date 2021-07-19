using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<TypeMapping> CommonMappings = new();
        public List<(List<TypeMapping> Mappings, List<UsingDirectiveSyntax> Usings)> CustomMappings = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classNode
                && context.GetDeclaredSymbol(classNode).HasAttribute(Annotations.MapperAttributeName))
            {
                var mappings = HandleCustomMapperClass(context, classNode);

                var currentCustom = CustomMappings.SelectMany(x => x.Mappings);
                var custom = mappings.Where(x => x.Type is MappingType.Custom or MappingType.Partial).Distinct();
                var newCustom = custom.Except(currentCustom.Intersect(custom)).ToList();
                CustomMappings.Add((newCustom, classNode.GetUsings()));
                CommonMappings.RemoveAll(x => newCustom.Contains(x));

                var common = mappings.Where(x => x.Type == MappingType.Common);
                var newCommonMappings = common.Except(common.Intersect(CommonMappings)).Except(currentCustom);
                CommonMappings.AddRange(newCommonMappings);
            }
            else if (context.Node is InvocationExpressionSyntax invocationNode
                && context.GetSymbol(invocationNode.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom.ToDisplayString() == StartMapperSource.FunctionFullName)
            {
                var mappings = HandleStartMapperInvocation(context, invocationNode, method);

                var currentCustom = CustomMappings.SelectMany(x => x.Mappings);
                var newMappings = mappings.Except(mappings.Intersect(CommonMappings)).Except(currentCustom);
                CommonMappings.AddRange(newMappings);
            }
        }

        private List<TypeMapping> HandleStartMapperInvocation(
            GeneratorSyntaxContext context, InvocationExpressionSyntax node, IMethodSymbol method)
        {
            var member = (MemberAccessExpressionSyntax)node.Expression;
            var memberSymbol = context.GetSymbol(member.Expression) as ILocalSymbol;

            return CreateCommonMappings(memberSymbol.Type, method.ReturnType);
        }

        private List<TypeMapping> HandleCustomMapperClass(GeneratorSyntaxContext context, ClassDeclarationSyntax node)
        {
            var mappings = new List<TypeMapping>();
            foreach (var method in node.GetMethodsDeclarations())
            {
                if (method.HasOneParameter())
                {
                    if (context.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName))
                    {
                        mappings.AddRange(CreatePartialMappings(context, method));
                    }
                    else
                    {
                        mappings.Add(CreateCustomMapping(context, method));
                    }
                }
            }

            return mappings;
        }

        private List<TypeMapping> CreateCommonMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var constructorProperties = GetConstructorPropertiesMappings(from, to);
            var isConstructorMapping = constructorProperties.Count > 0;
            var propertiesMappings = isConstructorMapping
                ? constructorProperties
                : CreatePropertiesMappings(from, to);
            var mappings = CreateCommonMappingsForProperties(propertiesMappings);
            var mapping = TypeMapping.CreateCommon(from, to, propertiesMappings, isConstructorMapping);
            mappings.Add(mapping);

            return mappings;
        }

        private List<PropertyMapping> GetConstructorPropertiesMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var constructors = (to as INamedTypeSymbol)
                .Constructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .OrderByDescending(x => x.Parameters.Count());
            if (constructors.Count() == 0)
            {
                throw new ArgumentException($"Error when create mapping from {from} to {to}, {to} must declare at least one public constructor");
            }

            foreach (var constructor in constructors)
            {
                var propertiesMappings = new List<PropertyMapping>();

                foreach (var parameter in constructor.Parameters)
                {
                    var fromProperty = from.GetProperties().FirstOrDefault(x => x.Name.ToUpperInvariant() == parameter.Name.ToUpperInvariant());
                    if (fromProperty != null)
                    {
                        propertiesMappings.Add(new PropertyMapping(fromProperty, parameter));
                    }
                }

                if (propertiesMappings.Count() == constructor.Parameters.Count())
                {
                    return propertiesMappings;
                }
            }

            return new List<PropertyMapping>();
        }

        private List<PropertyMapping> CreatePropertiesMappings(ITypeSymbol from, ITypeSymbol to)
        {
            var propertiesMappings = new List<PropertyMapping>();

            foreach (var fromProperty in from.GetProperties())
            {
                var toProperty = to.GetProperties().FirstOrDefault(x => x.Name == fromProperty.Name);
                if (toProperty != null)
                {
                    propertiesMappings.Add(new PropertyMapping(fromProperty, toProperty));
                }
            }

            return propertiesMappings;
        }

        private List<TypeMapping> CreateCommonMappingsForProperties(List<PropertyMapping> propertiesMappings)
        {
            var mappings = new List<TypeMapping>();
            foreach (var propertyMapping in propertiesMappings)
            {
                if (!propertyMapping.IsSameTypes)
                {
                    mappings.AddRange(CreateCommonMappings(propertyMapping.TypeFrom, propertyMapping.TypeTo));
                }
            }

            return mappings;
        }

        private TypeMapping CreateCustomMapping(GeneratorSyntaxContext context, MethodDeclarationSyntax method)
        {
            var parameter = method.GetSingleParameter();
            var from = context.GetTypeSymbol(parameter.Type);
            var to = context.GetTypeSymbol(method.ReturnType);
            var mapping = TypeMapping.CreateCustom(from, to, method);

            return mapping;
        }

        private List<TypeMapping> CreatePartialMappings(GeneratorSyntaxContext context, MethodDeclarationSyntax method)
        {
            ObjectCreationExpressionSyntax objCreationExpression =
                method.ExpressionBody != null
                ? method.ExpressionBody.Expression as ObjectCreationExpressionSyntax
                : (method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax).Expression as ObjectCreationExpressionSyntax;
            var byConstructor = (context.GetSymbol(objCreationExpression) as IMethodSymbol).Parameters.Select(x => x.Name.ToUpperInvariant());
            var byInitialyzer = objCreationExpression.GetInitializersLeft();

            var parameter = method.GetSingleParameter();
            var from = context.GetTypeSymbol(parameter.Type);
            var to = context.GetTypeSymbol(method.ReturnType);

            var propertiesMappings =  CreatePropertiesMappings(from, to);
            propertiesMappings.RemoveAll(x => byInitialyzer.Contains(x.NameFrom));
            propertiesMappings.RemoveAll(x => byConstructor.Contains(x.NameFrom.ToUpperInvariant()));
            var mappings = CreateCommonMappingsForProperties(propertiesMappings);
            var mapping = TypeMapping.CreatePartial(from, to, propertiesMappings, method, isConstructorMapping: false);
            mappings.Add(mapping);

            return mappings;
        }
    }
}
