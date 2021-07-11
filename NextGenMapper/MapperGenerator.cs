using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapToAttribute", Annotations.mapToAttributeText);
                i.AddSource("MapReverseAttribute", Annotations.mapReverseAttributeText);
                i.AddSource("TargetNameAttribute", Annotations.targetNameAttributeText);
            });

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            StringBuilder sourceBuilder = new StringBuilder(@"
using System;
namespace NextGenMapper
{
    public static class Mapper
    {
");


            foreach (var mapping in receiver.MappingList)
            {
                var fromProps = mapping.From.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);
                var toProps = mapping.To.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol);

                AddMapFunction(sourceBuilder, mapping.From.ToDisplayString(), mapping.To.ToDisplayString(), fromProps, toProps, mapping.TargetNames);

                if (mapping.Reverse)
                {
                    AddMapFunction(sourceBuilder, mapping.To.ToDisplayString(), mapping.From.ToDisplayString(), toProps, fromProps, mapping.TargetNames);
                }
            }

            // finish creating the source to inject
            sourceBuilder.Append(@"
    }
}");

            // inject the created source into the users compilation
            context.AddSource("Mapper", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private void AddMapFunction(
            StringBuilder sourceBuilder,
            string from, 
            string to, 
            IEnumerable<IPropertySymbol> fromProps, 
            IEnumerable<IPropertySymbol> toProps, 
            Dictionary<string, string> targetNames)
        {
            //public static User2 Map(User1 source) => new User2 { Name = source.Name, Email = source.Email };
            sourceBuilder.Append($"        public static {to} Map({from} source) => new {to} {{ ");
            foreach (IPropertySymbol fromProperty in fromProps)
            {
                targetNames.TryGetValue(fromProperty.Name, out var targetName);
                targetName ??= fromProperty.Name;

                var toProperty = toProps.FirstOrDefault(x => x.Name == targetName && x.Type.ToDisplayString() == fromProperty.Type.ToDisplayString());
                if (toProperty != null)
                {
                    sourceBuilder.Append($"{toProperty.Name} = source.{fromProperty.Name}, ");
                }
            }
            sourceBuilder.AppendLine("};");
        }
    }

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

    class Mapping
    {
        public INamedTypeSymbol From { get; set; }
        public INamedTypeSymbol To { get; set; }
        public bool Reverse { get; set; }
        public Dictionary<string, string> TargetNames { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, bool> Ignore { get; set; } = new Dictionary<string, bool>();
    }
}
