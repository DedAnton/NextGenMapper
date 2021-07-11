using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            #if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            #endif

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
}
