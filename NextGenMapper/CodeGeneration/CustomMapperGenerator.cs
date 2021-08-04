using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Linq;

namespace NextGenMapper.CodeGeneration
{
    public class CustomMapperGenerator
    {
        public string Generate(MapGroup customMapGroup) =>
$@"
{customMapGroup.Usings.Join()}

namespace NextGenMapper
{{
    public static partial class Mapper
    {{
{customMapGroup.Maps.InterpolateAndJoin(x => GenerateMappingFunction(x), intend: 2)}
    }}
}}";

        public string GenerateMappingFunction(TypeMap map)
            => map switch
            {
                TypeCustomMap { MethodType: MethodType.Block } customBlockMap => GenerateCustomMapBlockFunction(customBlockMap),
                TypeCustomMap { MethodType: MethodType.Expression } customExpressionMap => GenerateCustomMapExpressionFunction(customExpressionMap),
                ClassPartialMap partialMap => GeneratePartialMapFunction(partialMap),
                ClassPartialConstructorMap partialConstructorMap => GeneratePartialConstuctorMapFunction(partialConstructorMap),
                _ => throw new ArgumentOutOfRangeException(map.GetType().ToString())
            };


        private string GenerateCustomMapBlockFunction(TypeCustomMap map) =>
$@"
public static {map.To} Map<To>(this {map.From} {map.ParameterName})
{map.Body?.ToString()}";


        private string GenerateCustomMapExpressionFunction(TypeCustomMap map) =>
$@"
public static {map.To} Map<To>(this {map.From} {map.ParameterName})
    {map.ExpressionBody};";


        private string GeneratePartialConstuctorMapFunction(ClassPartialConstructorMap map) => map.Method.ToString();

        private string GeneratePartialMapFunction(ClassPartialMap map) =>
$@"
public static {map.To} Map<To>(this {map.From} _a___source)
{{
    {map.To} _a__UserFunction({map.From} {map.ParameterName})
    {{
{map.CustomStatements.InterpolateAndJoin(x => x.ToString(), 2)}
    }}
    var _a__result = _a__UserFunction(_a___source);

    return new {map.To.ToDisplayString()}
    (
{map.ConstructorProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.IsProvidedByUser || x.HasImplicitConversion,
    x => $"{GetSource(x)}.{x.NameFrom}",
    x => $"{GetSource(x)}.{x.NameFrom}.Map<{x.TypeTo}>()",
    intend: 2, separator: ",\r\n")}
    )
    {{
{map.InitializerProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.IsProvidedByUser || x.HasImplicitConversion,
    x => $"{x.NameTo} = {GetSource(x)}.{x.NameFrom}",
    x => $"{x.NameTo} = {GetSource(x)}.{x.NameFrom}.Map<{x.TypeTo}>()",
    intend: 2, separator: ",\r\n")}
    }};
}}";

        private string GetSource(IMemberMap member) => member.IsProvidedByUser ? "_a__result" : "_a___source";
    }
}
