using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private const int TAB1 = 4;
        private const int TAB2 = 8;

        #region mapper text

        private const string MAPPER_BEGIN =
@"namespace NextGenMapper
{
    public static partial class Mapper
    {
";

        private const string MAPPER_END =
@"    }
}";

    #endregion

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
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("PartialAttribute", Annotations.PartialAttributeText);
                i.AddSource("StartMapper", StartMapperSource.StartMapper);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            var commonMapper = GenerateCommonMapper(receiver);
            context.AddSource("CommonMapper", SourceText.From(commonMapper, Encoding.UTF8));

            var customMappers = GenerateCustomMappers(receiver);
            var mapperIndex = 1;
            foreach(var mapper in customMappers)
            {
                context.AddSource($"{mapperIndex++}_CustomMapper", SourceText.From(mapper, Encoding.UTF8));
            }
        }

        private IEnumerable<string> GenerateCustomMappers(SyntaxReceiver receiver)
        {
            foreach(var plan in receiver.Planner.CustomMappingPlans)
            {
                var sourceBuilder = new StringBuilder();
                sourceBuilder.Append(GenerateUsings(plan.Usings));
                sourceBuilder.Append(MAPPER_BEGIN);
                foreach (var mapping in plan.Mappings)
                {
                    var mapFunction = mapping switch
                    {
                        { Type: MappingType.Custom, MethodType: MethodType.Block } => GenerateCustomMapBlockFunction(mapping),
                        { Type: MappingType.Custom, MethodType: MethodType.Expression } => GenerateCustomMapExpressionFunction(mapping),
                        { Type: MappingType.Partial } => GeneratePartialMapFunction(mapping),
                        _ => throw new ArgumentOutOfRangeException(nameof(mapping.Type))
                    };
                    sourceBuilder.AppendLine(mapFunction.LeadingSpace(TAB2));
                }
                sourceBuilder.Append(MAPPER_END);

                yield return sourceBuilder.ToString();
            }
        }

        private string GenerateCommonMapper(SyntaxReceiver receiver)
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.Append(MAPPER_BEGIN);
            receiver.Planner.CommonMappingPlan?.Mappings.ForEach(x => 
                sourceBuilder.AppendLine(GenerateCommonMapFunction(x).LeadingSpace(TAB2)));
            sourceBuilder.Append(MAPPER_END);

            return sourceBuilder.ToString();
        }

        private string GenerateCommonMapFunction(TypeMapping mapping)
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.Append($"public static {mapping.ToType} Map<To>(this {mapping.FromType} source) => new {mapping.ToType}(");
            foreach (var property in mapping.ConstructorProperties)
            {
                if (property.IsSameTypes)
                {
                    sourceBuilder.Append($"source.{property.NameFrom}");
                }
                else
                {
                    sourceBuilder.Append($"source.{property.NameFrom}.Map<{property.TypeTo}>()");
                }
                if (property != mapping.ConstructorProperties.Last())
                {
                    sourceBuilder.Append(", ");
                }
            }
            sourceBuilder.Append(") { ");
            foreach (var property in mapping.InitializatorPropererties)
            {
                if (property.IsSameTypes)
                {
                    sourceBuilder.Append($"{property.NameTo} = source.{property.NameFrom}, ");
                }
                else
                {
                    sourceBuilder.Append($"{property.NameTo} = source.{property.NameFrom}.Map<{property.TypeTo}>(), ");
                }
            }
            sourceBuilder.AppendLine("};");

            return sourceBuilder.ToString();
        }

        private string GenerateCustomMapBlockFunction(TypeMapping mapping)
            => $"public static {mapping.To} Map<To>(this {mapping.From} {mapping.ParameterName})\r\n{mapping.Body.ToString().RemoveLeadingSpace(TAB2)}";

        private string GenerateCustomMapExpressionFunction(TypeMapping mapping)
            => $"public static {mapping.To} Map<To>(this {mapping.From} {mapping.ParameterName}) {mapping.ExpressionBody};\r\n";

        private string GeneratePartialMapFunction(TypeMapping mapping)
        {
            var userFunction = GenerateUserFunction(mapping).LeadingSpace(TAB1);
            var sourceBuilder = new StringBuilder();

            sourceBuilder.Append($"public static {mapping.To} Map<To>(this {mapping.From} _a___source)\r\n{{\r\n");
            sourceBuilder.Append(userFunction);
            sourceBuilder.AppendLine("var result = UserFunction(_a___source);".LeadingSpace(TAB1));
            sourceBuilder.Append($"return new {mapping.ToType}(".LeadingSpace(TAB1));
            foreach (var property in mapping.ConstructorProperties)
            {
                var source = property.IsProvidedByUser ? "result" : "_a___source";
                if (property.IsSameTypes || property.IsProvidedByUser)
                {
                    sourceBuilder.Append($"{source}.{property.NameFrom}");
                }
                else
                {
                    sourceBuilder.Append($"{source}.{property.NameFrom}.Map<{property.TypeTo}>()");
                }
                if (property != mapping.ConstructorProperties.Last())
                {
                    sourceBuilder.Append(", ");
                }
            }
            sourceBuilder.Append(") { ");
            foreach (var property in mapping.InitializatorPropererties)
            {
                var source = property.IsProvidedByUser ? "result" : "_a___source";

                if (property.IsSameTypes || property.IsProvidedByUser)
                {
                    sourceBuilder.Append($"{property.NameTo} = {source}.{property.NameFrom}, ");
                }
                else
                {
                    sourceBuilder.Append($"{property.NameTo} = {source}.{property.NameFrom}.Map<{property.TypeTo}>(), ");
                }
            }
            sourceBuilder.AppendLine("};");
            sourceBuilder.AppendLine("}");

            return sourceBuilder.ToString();
        }

        private string GenerateUserFunction(TypeMapping mapping)
        {
            var body = mapping.MethodType == MethodType.Block 
                ? $"\r\n{mapping.Body.ToString().RemoveLeadingSpace(TAB2)}" 
                : $"{mapping.ExpressionBody};\r\n";
            return $"{mapping.To} UserFunction({mapping.From} {mapping.ParameterName}) {body}";
        }

        private string GenerateUsings(List<string> usings)
        {
            var sourceBuilder = new StringBuilder();
            foreach (var @using in usings)
            {
                sourceBuilder.AppendLine(@using.ToString());
            }

            return sourceBuilder.AppendLine().ToString();
        }
    }
}
