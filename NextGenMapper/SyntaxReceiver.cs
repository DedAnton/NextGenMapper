using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper
{
    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<Mapping> MappingList = new List<Mapping>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax node)
            {
                var fromType = context.SemanticModel.GetDeclaredSymbol(node);
                var attribute = fromType.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToDisplayString() == Annotations.mapToAttributeName);
                if (attribute != null)
                {
                    if (attribute
                        .ConstructorArguments
                        .SingleOrDefault()
                        .Value is INamedTypeSymbol toType)
                    {
                        var reverse = toType
                            .GetAttributes()
                            .Any(x => x.AttributeClass.ToDisplayString() == Annotations.mapReverseAttributeName);
                        var targetNames = fromType
                            .GetMembers()
                            .Where(x => x.Kind == SymbolKind.Property)
                            .Select(x => (x.Name, TargetName: x.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToDisplayString() == Annotations.targetNameAttributeName)?.ConstructorArguments.SingleOrDefault().Value as string))
                            .Where(x => x.TargetName != null);
                        
                        if (reverse)
                        {
                            var reverseTargetNames = targetNames
                                .Select(x => (Name: x.TargetName, TargetName: x.Name));
                            targetNames = targetNames.Concat(reverseTargetNames);
                        }

                        var mapping = new Mapping
                        {
                            From = fromType,
                            To = toType,
                            Reverse = reverse,
                            TargetNames = targetNames.ToDictionary(x => x.Name, y => y.TargetName)
                        };
                        MappingList.Add(mapping);
                    }
                }
            }
        }
    }
}
