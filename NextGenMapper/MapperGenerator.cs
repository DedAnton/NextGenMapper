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
        private const string _startMapperText = @"
using System;

namespace NextGenMapper
{
    public static partial class Mapper
    {
        public static TTo Map<TTo>(this object source) => throw new InvalidOperationException();
    }
}";

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
                i.AddSource("MapToAttribute", Annotations.MapToAttributeText);
                i.AddSource("MapReverseAttribute", Annotations.MapReverseAttributeText);
                i.AddSource("TargetNameAttribute", Annotations.TargetNameAttributeText);
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("StartMapper", _startMapperText);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            StringBuilder sourceBuilder = new(@"
using System;

namespace NextGenMapper
{
    public static partial class Mapper
    {
");


            foreach (var mapping in receiver.MappingList)
            {
                AddMapFunction(sourceBuilder, mapping);
            }

            sourceBuilder.Append(@"
    }
}");

            context.AddSource("Mapper", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private void AddMapFunction(
            StringBuilder sourceBuilder,
            Mapping mapping)
        {
            sourceBuilder.Append($"        public static {mapping.TypeTo} Map<TTo>(this {mapping.TypeFrom} source) => new {mapping.TypeTo} {{ ");
            foreach (var property in mapping.Properties)
            {
                if (property.IsSameTypes)
                {
                    sourceBuilder.Append($"{property.NameTo} = source.{property.NameFrom}, ");
                }
                else
                {
                    sourceBuilder.Append($"{property.NameTo} = source.{property.NameFrom}.Map<{property.TypeTo}>()");
                }
            }
            sourceBuilder.AppendLine("};");
        }
    }
}
