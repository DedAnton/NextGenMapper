using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Linq;

namespace NextGenMapper.CodeGeneration
{
    public class CommonMapperGenerator
    {
        public string Generate(MapGroup commonMapGroup) =>
$@"
{commonMapGroup.Usings.Join()}

namespace NextGenMapper
{{
    public static partial class Mapper
    {{
{commonMapGroup.Maps.InterpolateAndJoin(x => GenerateMappingFunction(x), intend: 2)}
    }}
}}";

        private string GenerateMappingFunction(TypeMap map)
            => map switch
            {
                CollectionMap collectionMap => GenerateCollectionMapFunction(collectionMap),
                EnumMap enumMap => GenerateEnumMapFunction(enumMap),
                ClassMap classMap => GenerateClassMapFunction(classMap),
                _ => throw new ArgumentOutOfRangeException()
            };

        private string GenerateCollectionMapFunction(CollectionMap map) => 
$@"public static {map.To.ToDisplayString()} Map<To>(this {map.From.ToDisplayString()} sources)
    => sources.Select(x => x.Map<{map.ItemTo.ToDisplayString()}>()){(map.CollectionType == CollectionType.List ? ".ToList()" : ".ToArray()")};";


        private string GenerateEnumMapFunction(EnumMap map) =>
$@"public static {map.To.ToDisplayString()} Map<To>(this {map.From.ToDisplayString()} source) => source switch
{{
    {map.Fields.InterpolateAndJoin(x => $"{x.TypeFrom}.{x.NameFrom} => {x.TypeTo}.{x.NameTo},")}
    _ => throw new System.ArgumentOutOfRangeException(""Error when mapping { map.From.ToDisplayString()} to { map.To.ToDisplayString()}"")
}};
";


        private string GenerateClassMapFunction(ClassMap map) =>
$@"public static {map.To.ToDisplayString()} Map<To>(this {map.From.ToDisplayString()} source) => new {map.To.ToDisplayString()}
(
{map.ConstructorProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.HasImplicitConversion,
    x => $"source.{x.NameFrom}",
    x => $"source.{x.NameFrom}.Map<{x.TypeTo}>()", 
    separator: ",\r\n")}
)
{{
{map.InitializerProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.HasImplicitConversion,
    x => $"{x.NameTo} = source.{x.NameFrom}",
    x => $"{x.NameTo} = source.{x.NameFrom}.Map<{x.TypeTo}>()",
    separator: ",\r\n")}
}};
";
    }
}
