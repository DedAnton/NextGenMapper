using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NextGenMapper.Extensions;

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
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif

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
            foreach (var mapGroup in receiver.Planner.MapGroups.Where(x => x.Priority == CodeAnalysis.MapPriority.Custom))
            {
                var sourceBuilder = new StringBuilder();
                sourceBuilder.Append(GenerateUsings(mapGroup.Usings));
                sourceBuilder.Append(MAPPER_BEGIN);
                foreach (var map in mapGroup.Maps)
                {
                    var mapFunction = map switch
                    {
                        TypeCustomMap { MethodType: MethodType.Block } customBlockMap  => GenerateCustomMapBlockFunction(customBlockMap),
                        TypeCustomMap { MethodType: MethodType.Expression } customExpressionMap => GenerateCustomMapExpressionFunction(customExpressionMap),
                        ClassPartialMap partialMap => GeneratePartialMapFunction(partialMap),
                        ClassPartialConstructorMap partialConstructorMap => GeneratePartialConstuctorMapFunction(partialConstructorMap),
                        _ => throw new ArgumentOutOfRangeException(map.GetType().ToString())
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
            receiver.Planner.MapGroups.FirstOrDefault(x => x.Priority == CodeAnalysis.MapPriority.Common)?.Maps.OfType<ClassMap>().ForEach(x => 
                sourceBuilder.AppendLine(GenerateClassMapFunction(x).LeadingSpace(TAB2)));
            receiver.Planner.MapGroups.FirstOrDefault(x => x.Priority == CodeAnalysis.MapPriority.Common)?.Maps.OfType<EnumMap>().ForEach(x =>
                sourceBuilder.AppendLine(GenerateEnumMapFunction(x).LeadingSpace(TAB2)));
            sourceBuilder.Append(MAPPER_END);

            return sourceBuilder.ToString();
        }

        private string GenerateEnumMapFunction(EnumMap map)
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine($"public static {map.To.ToDisplayString()} Map<To>(this {map.From.ToDisplayString()} source)");
            sourceBuilder.AppendLine($"=> source switch".LeadingSpace(TAB1));
            sourceBuilder.AppendLine("{".LeadingSpace(TAB1));
            foreach(var field in map.Fields)
            {
                sourceBuilder.AppendLine($"{field.TypeFrom}.{field.NameFrom} => {field.TypeTo}.{field.NameTo},".LeadingSpace(TAB2));
            }
            sourceBuilder.AppendLine($"_ => throw new System.ArgumentOutOfRangeException(\"Error when mapping {map.From.ToDisplayString()} to {map.To.ToDisplayString()}\")".LeadingSpace(TAB2));
            sourceBuilder.AppendLine("};".LeadingSpace(TAB1));

            return sourceBuilder.ToString();
        }

        private string GenerateClassMapFunction(ClassMap map)
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.Append($"public static {map.To.ToDisplayString()} Map<To>(this {map.From.ToDisplayString()} source) => new {map.To.ToDisplayString()}(");
            foreach (var property in map.ConstructorProperties)
            {
                if (property.IsSameTypes || property.HasImplicitConversion)
                {
                    sourceBuilder.Append($"source.{property.NameFrom}");
                }
                else
                {
                    sourceBuilder.Append($"source.{property.NameFrom}.Map<{property.TypeTo}>()");
                }
                if (property != map.ConstructorProperties.Last())
                {
                    sourceBuilder.Append(", ");
                }
            }
            sourceBuilder.Append(") { ");
            foreach (var property in map.InitializerProperties)
            {
                if (property.IsSameTypes || property.HasImplicitConversion)
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

        private string GenerateCustomMapBlockFunction(TypeCustomMap map)
            => $"public static {map.To} Map<To>(this {map.From} {map.ParameterName})\r\n{map.Body?.ToString().RemoveLeadingSpace(TAB2)}";

        private string GenerateCustomMapExpressionFunction(TypeCustomMap map)
            => $"public static {map.To} Map<To>(this {map.From} {map.ParameterName}) {map.ExpressionBody};\r\n";

        private string GeneratePartialConstuctorMapFunction(ClassPartialConstructorMap map) => map.Method.ToString();

        private string GeneratePartialMapFunction(ClassPartialMap map)
        {
            var userFunction = GenerateUserFunction(map).LeadingSpace(TAB1);
            var sourceBuilder = new StringBuilder();

            sourceBuilder.Append($"public static {map.To} Map<To>(this {map.From} _a___source)\r\n{{\r\n");
            sourceBuilder.Append(userFunction);
            sourceBuilder.AppendLine("var result = UserFunction(_a___source);".LeadingSpace(TAB1));
            sourceBuilder.Append($"return new {map.To.ToDisplayString()}(".LeadingSpace(TAB1));
            foreach (var property in map.ConstructorProperties)
            {
                var source = property.IsProvidedByUser ? "result" : "_a___source";
                if (property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion)
                {
                    sourceBuilder.Append($"{source}.{property.NameFrom}");
                }
                else
                {
                    sourceBuilder.Append($"{source}.{property.NameFrom}.Map<{property.TypeTo}>()");
                }
                if (property != map.ConstructorProperties.Last())
                {
                    sourceBuilder.Append(", ");
                }
            }
            sourceBuilder.Append(") { ");
            foreach (var property in map.InitializerProperties)
            {
                var source = property.IsProvidedByUser ? "result" : "_a___source";

                if (property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion)
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

        private string GenerateUserFunction(ClassPartialMap map)
        {
            var body = map.MethodType == MethodType.Block 
                ? $"\r\n{map.Body?.ToString().RemoveLeadingSpace(TAB2)}" 
                : $"{map.ExpressionBody};\r\n";
            return $"{map.To} UserFunction({map.From} {map.ParameterName}) {body}";
        }

        private string GenerateUsings(List<string> usings)
        {
            var sourceBuilder = new StringBuilder();
            foreach (var @using in usings)
            {
                sourceBuilder.AppendLine(@using);
            }

            return sourceBuilder.AppendLine().ToString();
        }
    }
}
