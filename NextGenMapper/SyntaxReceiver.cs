using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        private List<(ITypeSymbol from, ITypeSymbol to)> typesWithoutDefaultConstructor = new();
        
        public List<Mapping> Mappings = new();
        public List<UsingDirectiveSyntax> CustomMappingsUsings = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax node)
            {
                if (context.GetSymbolInfo(node.Expression).Symbol is IMethodSymbol symbol
                    && symbol.Kind == SymbolKind.Method 
                    && symbol.ContainingType.ToDisplayString() == "NextGenMapper.Mapper" 
                    && symbol.MethodKind == MethodKind.ReducedExtension)
                {
                    var member = (MemberAccessExpressionSyntax)node.Expression;
                    var memberSymbol = context.GetSymbolInfo(member.Expression).Symbol as ILocalSymbol;

                    CreateCommonMapping(memberSymbol.Type, symbol.ReturnType);
                }
            }
            else if (context.Node is ClassDeclarationSyntax cNode)
            {
                if (context.GetSymbol(cNode).HasAttribute(Annotations.MapperAttributeName))
                {
                    var methodsDeclarations = cNode.Members.Where(x => x.Kind() == SyntaxKind.MethodDeclaration);
                    foreach (MethodDeclarationSyntax method in methodsDeclarations)
                    {
                        if (context.GetSymbol(method).HasAttribute(Annotations.PartialAttributeName))
                        {
                            CreatePartialCustomMapping(context, method);
                        }
                        else
                        {
                            CreateCustomMapping(context, method);
                        }
                    }

                    //class -> namespace -> comilation unit
                    var usings = (cNode.Parent.Parent as CompilationUnitSyntax).Usings;
                    var newUsings = usings.Except(usings.Intersect(CustomMappingsUsings));
                    CustomMappingsUsings.AddRange(newUsings);
                }
            }
        }

        private void CreatePartialCustomMapping(GeneratorSyntaxContext context, MethodDeclarationSyntax method)
        {
            var singleParameter = method.ParameterList.Parameters.SingleOrDefault();
            if (singleParameter != null)
            {
                var mappedProperties = new List<string>();
                var arrowExp = method.ExpressionBody;
                if (arrowExp != null)
                {
                    var objCreationExp = arrowExp.Expression as ObjectCreationExpressionSyntax;
                    var initializers = objCreationExp.Initializer.Expressions;
                    foreach(AssignmentExpressionSyntax assigment in initializers)
                    {
                        mappedProperties.Add((assigment.Left as IdentifierNameSyntax).Identifier.ValueText);
                    }
                }
                else if (method.Body != null)
                {
                    var returnStatemant = method.Body.Statements.SingleOrDefault(x => x is ReturnStatementSyntax) as ReturnStatementSyntax;
                    var objCreationExp = returnStatemant.Expression as ObjectCreationExpressionSyntax;
                    var initializers = objCreationExp.Initializer.Expressions;
                    foreach (AssignmentExpressionSyntax assigment in initializers)
                    {
                        mappedProperties.Add((assigment.Left as IdentifierNameSyntax).Identifier.ValueText);
                    }
                }
                else
                {
                    return;
                }

                var from = context.SemanticModel.GetSymbolInfo(singleParameter.Type).Symbol as ITypeSymbol;
                var to = context.SemanticModel.GetSymbolInfo(method.ReturnType).Symbol as ITypeSymbol;
                var fromProps = from.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);
                var toProps = to.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);

                var mapping = new PartialMapping(from, to, method);
                foreach (var fromProp in fromProps)
                {
                    var toProp = toProps.FirstOrDefault(x => x.Name == fromProp.Name);
                    if (toProp != null)
                    {
                        var mappingProp = new MappingProperty(fromProp, toProp);
                        mapping.Properties.Add(mappingProp);

                        if (!fromProp.Type.Equals(toProp.Type, SymbolEqualityComparer.IncludeNullability))
                        {
                            var hasDefaultConstructor = (toProp.Type as INamedTypeSymbol)
                                .Constructors
                                .Any(x => x.DeclaredAccessibility == Accessibility.Public && x.Parameters.Count() == 0);
                            if (hasDefaultConstructor)
                            {
                                CreateCommonMapping(fromProp.Type, toProp.Type);
                            }
                            else
                            {
                                typesWithoutDefaultConstructor.Add((fromProp.Type, toProp.Type));
                            }
                        }
                    }
                }
                mapping.Properties.RemoveAll(x => mappedProperties.Contains(x.NameFrom));

                //remove same common mapping
                Mappings.Remove(mapping);
                Mappings.Add(mapping);
            }
        }

        private void CreateCustomMapping(GeneratorSyntaxContext context, MethodDeclarationSyntax method)
        {
            var singleParameter = method.ParameterList.Parameters.SingleOrDefault();
            if (singleParameter != null)
            {
                var from = context.SemanticModel.GetSymbolInfo(singleParameter.Type).Symbol as ITypeSymbol;
                var to = context.SemanticModel.GetSymbolInfo(method.ReturnType).Symbol as ITypeSymbol;
                var mapping = new CustomMapping(from, to, method);

                //remove same common mapping
                Mappings.Remove(mapping);
                Mappings.Add(mapping);
            }
        }

        private void CreateCommonMapping(ITypeSymbol from, ITypeSymbol to)
        {
            var fromProps = from.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);
            var toProps = to.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);

            var mapping = new Mapping(from, to);
            foreach(var fromProp in fromProps)
            {
                var toProp = toProps.FirstOrDefault(x => x.Name == fromProp.Name);
                if (toProp != null)
                {
                    var mappingProp = new MappingProperty(fromProp, toProp);
                    mapping.Properties.Add(mappingProp);

                    if (!fromProp.Type.Equals(toProp.Type, SymbolEqualityComparer.IncludeNullability))
                    {
                        var hasDefaultConstructor = (toProp.Type as INamedTypeSymbol)
                            .Constructors
                            .Any(x => x.DeclaredAccessibility == Accessibility.Public && x.Parameters.Count() == 0);
                        if (hasDefaultConstructor)
                        {
                            CreateCommonMapping(fromProp.Type, toProp.Type);
                        }
                        else
                        {
                            typesWithoutDefaultConstructor.Add((fromProp.Type, toProp.Type));
                        }
                    }
                }
            }

            if (!Mappings.Contains(mapping))
            {
                Mappings.Add(mapping);
            }
        }
    }
}
