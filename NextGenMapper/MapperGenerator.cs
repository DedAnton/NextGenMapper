using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private const int tab = 4;
        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif

            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapToAttribute", Annotations.MapToAttributeText);
                i.AddSource("MapReverseAttribute", Annotations.MapReverseAttributeText);
                i.AddSource("TargetNameAttribute", Annotations.TargetNameAttributeText);
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("PartialAttribute", Annotations.PartialAttributeText);
                i.AddSource("StartMapper", MappersText.StartMapper);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            var commonMapper = BuildCommonMapper(receiver);
            var customMapper = BuildCustomMapper(receiver);

            context.AddSource("CommonMapper", SourceText.From(commonMapper, Encoding.UTF8));
            context.AddSource("CustomMapper", SourceText.From(customMapper, Encoding.UTF8));
        }

        private string BuildCommonMapper(SyntaxReceiver receiver)
        {
            StringBuilder sourceBuilder = new(MappersText.MapperBegin);
            var mappings = receiver.Mappings.Where(x => x is not CustomMapping);
            foreach (var mapping in mappings)
            {
                AddMapFunction(sourceBuilder, mapping);
            }
            sourceBuilder.Append(MappersText.MapperEnd);

            return sourceBuilder.ToString();
        }

        private string BuildCustomMapper(SyntaxReceiver receiver)
        {
            var sourceBuilder = new StringBuilder();
            foreach(var @using in receiver.CustomMappingsUsings)
            {
                sourceBuilder.AppendLine(@using.ToString());
            }

            sourceBuilder.Append(MappersText.MapperBegin);
            var mappings = receiver.Mappings.Where(x => x is CustomMapping);
            foreach (CustomMapping customMapping in mappings)
            {
                if (customMapping is PartialMapping partialMapping)
                {
                    sourceBuilder.Append(GeneratePartialMapFunction(partialMapping).LeadingSpace(tab * 2));
                }
                else
                {
                    AddCustomMapFunction(sourceBuilder, customMapping);
                }
            }
            sourceBuilder.Append(MappersText.MapperEnd);

            return sourceBuilder.ToString();
        }

        private void AddMapFunction(StringBuilder sourceBuilder, Mapping mapping)
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

        private void AddCustomMapFunction(StringBuilder sourceBuilder, CustomMapping mapping)
        {
            var body = mapping.Body != null ? $"\r\n        {mapping.Body}\r\n" : $"{mapping.ExpressionBody};\r\n";
            sourceBuilder.Append($"        public static {mapping.TypeTo} Map<TTo>(this {mapping.TypeFrom} {mapping.ParameterName}) {body}");
        }

        private string GeneratePartialMapFunction(PartialMapping mapping)
        {
            var userFunction = GenerateUserFunction(mapping).LeadingSpace(tab);
            var sourceBuilder = new StringBuilder();

            sourceBuilder.Append($"public static {mapping.TypeTo} Map<TTo>(this {mapping.TypeFrom} _a__source)\r\n{{\r\n");
            sourceBuilder.Append(userFunction);
            sourceBuilder.AppendLine("var result = UserFunction(_a__source);".LeadingSpace(tab));
            foreach (var property in mapping.Properties)
            {
                if (property.IsSameTypes)
                {
                    sourceBuilder.AppendLine($"result.{property.NameTo} = _a__source.{property.NameFrom};".LeadingSpace(tab));
                }
                else
                {
                    sourceBuilder.AppendLine($"result.{property.NameTo} = _a__source.{property.NameFrom}.Map<{property.TypeTo}>();".LeadingSpace(tab));
                }
            }
            sourceBuilder.AppendLine("return result;".LeadingSpace(tab));
            sourceBuilder.AppendLine("}");

            return sourceBuilder.ToString();
        }

        private string GenerateUserFunction(PartialMapping mapping)
        {
            var body = mapping.Body != null ? $"\r\n{mapping.Body.ToString().LeadingSpace(tab * -2)}\r\n" : $"{mapping.ExpressionBody};\r\n";
            return $"{mapping.TypeTo} UserFunction({mapping.TypeFrom} {mapping.ParameterName}) {body}";
        }
    }
}
