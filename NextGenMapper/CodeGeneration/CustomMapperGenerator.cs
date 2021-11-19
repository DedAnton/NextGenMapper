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

$@"{customMapGroup.Usings.Join()}

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
                TypeCustomMap { MethodType: MethodBodyType.Block } customBlockMap => GenerateCustomMapBlockFunction(customBlockMap),
                TypeCustomMap { MethodType: MethodBodyType.Expression } customExpressionMap => GenerateCustomMapExpressionFunction(customExpressionMap),
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


        private string GeneratePartialConstuctorMapFunction(ClassPartialConstructorMap map) =>
$@"
public static {map.To} Map<To>(this {map.From} {map.ParameterName}) => new {map.To}
(
{map.ConstructorProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.IsProvidedByUser || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenConstructor,
    one => $"{one.ArgumentSyntax?.ToString() ?? $"{map.ParameterName}.{one.FromName}"}",
    two => $"UnflatteningMap_{two.ToType.ToString().RemoveDots()}({map.ParameterName})",
    @default => $"{map.ParameterName}.{@default.FromName}.Map<{@default.ToType}>()",
    separator: ",\r\n")}
)
{{
{map.InitializerProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.IsProvidedByUser || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenInitializer,
    one => $"{one.InitializerExpressionSyntax?.ToString() ?? $"{one.ToName} = {map.ParameterName}.{one.FromName}"}",
    two => $"{two.ToName} = UnflatteningMap_{two.ToType.ToString().RemoveDots()}({map.ParameterName})",
    @default => $"{@default.ToName} = {map.ParameterName}.{@default.FromName}.Map<{@default.ToType}>()",
    separator: ",\r\n")}
}};";

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
{map.ConstructorProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.IsProvidedByUser || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenConstructor,
    one => $"{GetSource(one)}.{one.FromName}",
    two => $"UnflatteningMap_{two.ToType.ToString().RemoveDots()}(_a___source)",
    @default => $"{GetSource(@default)}.{@default.FromName}.Map<{@default.ToType}>()",
    intend: 2, separator: ",\r\n")}
    )
    {{
{map.InitializerProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.IsProvidedByUser || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenInitializer,
    one => $"{one.ToName} = {GetSource(one)}.{one.FromName}",
    two => $"{two.ToName} = UnflatteningMap_{two.ToType.ToString().RemoveDots()}(_a___source)",
    @default => $"{@default.ToName} = {GetSource(@default)}.{@default.FromName}.Map<{@default.ToType}>()",
    intend: 2, separator: ",\r\n")}
    }};
}}";

        private string GetSource(MemberMap member) => member.IsProvidedByUser ? "_a__result" : "_a___source";
    }
}
