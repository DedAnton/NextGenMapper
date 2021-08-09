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

$@"{commonMapGroup.Usings.Join()}

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
                ClassMap classMap when !classMap.IsUnflattening => GenerateClassMapFunction(classMap),
                ClassMap unflattenClassMap when unflattenClassMap.IsUnflattening => GenerateUnflattenClassMapFunction(unflattenClassMap),
                _ => throw new ArgumentOutOfRangeException()
            };

        private string GenerateCollectionMapFunction(CollectionMap map) => 
$@"public static {map.To} Map<To>(this {map.From} sources)
    => sources.Select(x => x.Map<{map.ItemTo}>()){(map.CollectionType == CollectionType.List ? ".ToList()" : ".ToArray()")};";


        private string GenerateEnumMapFunction(EnumMap map) =>
$@"public static {map.To} Map<To>(this {map.From} source) => source switch
{{
    {map.Fields.InterpolateAndJoin(x => $"{x.FromType}.{x.FromName} => {x.ToType}.{x.ToName},")}
    _ => throw new System.ArgumentOutOfRangeException(""Error when mapping { map.From} to { map.To}"")
}};
";


        private string GenerateClassMapFunction(ClassMap map) =>
$@"public static {map.To} Map<To>(this {map.From} source) => new {map.To}
(
{map.ConstructorProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenConstructor,
    one => $"source.{one.FromName}",
    two => $" UnflatteningMap_{two.ToType.ToString().RemoveDots()}(source)",
    @default => $"source.{@default.FromName}.Map<{@default.ToType}>()",
    separator: ",\r\n")}
)
{{
{map.InitializerProperties.TwoTernarInterpolateAndJoin(
    one => one.IsSameTypes || one.HasImplicitConversion,
    two => two.MapType == MemberMapType.UnflattenInitializer,
    one => $"{one.ToName} = source.{one.FromName}",
    two => $"{two.ToName} = UnflatteningMap_{two.ToType.ToString().RemoveDots()}(source)",
    @default => $"{@default.ToName} = source.{@default.FromName}.Map<{@default.ToType}>()",
    separator: ",\r\n")}
}};
";


        private string GenerateUnflattenClassMapFunction(ClassMap map) =>
$@"private static {map.To} UnflatteningMap_{map.To.ToString().RemoveDots()}({map.From} source) => new {map.To}
(
{map.ConstructorProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.HasImplicitConversion,
    x => $"source.{x.FromName}",
    x => $"source.{x.FromName}.Map<{x.ToType}>()",
    separator: ",\r\n")}
)
{{
{map.InitializerProperties.TernarInterpolateAndJoin(
    x => x.IsSameTypes || x.HasImplicitConversion,
    x => $"{x.ToName} = source.{x.FromName}",
    x => $"{x.ToName} = source.{x.FromName}.Map<{x.ToType}>()",
    separator: ",\r\n")}
}};
";
    }
}
