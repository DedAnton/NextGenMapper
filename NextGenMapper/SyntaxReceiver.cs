using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper
{
    partial class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<Mapping> MappingList = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax node)
            {
                var symbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(node.Expression).Symbol;
                if (symbol != null && symbol.Kind == SymbolKind.Method && symbol.ContainingType.ToDisplayString() == "NextGenMapper.Mapper" && symbol.MethodKind == MethodKind.ReducedExtension)
                {
                    var member = (MemberAccessExpressionSyntax)node.Expression;
                    var memberSymbol = (ILocalSymbol)context.SemanticModel.GetSymbolInfo(member.Expression).Symbol;
                    var fromType = (INamedTypeSymbol)memberSymbol.Type;
                    var toType = (INamedTypeSymbol)symbol.ReturnType;

                    CreateMapping(fromType, toType);
                }
            }
        }

        private void CreateMapping(ITypeSymbol from, ITypeSymbol to)
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
                        CreateMapping(fromProp.Type, toProp.Type);
                    }
                }
            }

            if (!MappingList.Contains(mapping))
            {
                MappingList.Add(mapping);
            }
        }
    }
}
