using System.Linq;

namespace NextGenMapperTests
{
    public static class TestTypeSourceGenerator
    {
        public static string GenerateEnumMapPair(int fieldsCount, int byValueCount)
        {
            var fields = Enumerable.Range(0, fieldsCount).Select(
                x => x < byValueCount ? $"Field{x} = {x}" : $"Field{x}");

            var enumsSource = $@"
public enum SourceEnum
{{
    {string.Join(",\r\n    ", fields)}
}}

public enum DestinationEnum
{{
    {string.Join(",\r\n    ", fields)}
}}
";

            return enumsSource;
        }

        public static string GenerateClassMapPair(
            int propertiesCount, 
            int byConstructorCount, 
            int nestedTypesCount, 
            int nestedTypesDepth, 
            string nestedTypesPrefix = "Nested", 
            string nameFrom = "Source", 
            string nameTo = "Destination")
        {
            var properties = Enumerable.Range(0, propertiesCount).Select(
                x => $"public int Property{x} {{ get; set; }}");
            var constructorParameters = Enumerable.Range(0, byConstructorCount).Select(
                x => $"int property{x}");
            var constructorAssignments = Enumerable.Range(0, byConstructorCount).Select(
                x => $"Property{x} = property{x};");
            var nextDepth = nestedTypesDepth - 1;
            var nestedTypes = nestedTypesDepth > 0
                ? Enumerable.Range(0, nestedTypesCount).Select(
                    x => GenerateClassMapPair(
                        propertiesCount,
                        0,
                        nestedTypesCount,
                        nextDepth,
                        $"D{nestedTypesDepth}N{x}{nestedTypesPrefix}",
                        nestedTypesPrefix + "Source" + x,
                        nestedTypesPrefix + "Destination" + x))
                : System.Array.Empty<string>();
            var nestedPropertiesFrom = nestedTypesDepth > 0 ? Enumerable.Range(0, nestedTypesCount).Select(
                x => $"public {nestedTypesPrefix}Source{x} PropertyNested{x} {{ get; set; }}") : System.Array.Empty<string>();
            var nestedPropertiesTo = nestedTypesDepth > 0 ? Enumerable.Range(0, nestedTypesCount).Select(
                x => $"public {nestedTypesPrefix}Destination{x} PropertyNested{x} {{ get; set; }}") : System.Array.Empty<string>();

            var classes = $@"
public class {nameFrom}
{{
    {string.Join("\r\n    ", properties)}

    {string.Join("\r\n    ", nestedPropertiesFrom)}
}}
public class {nameTo}
{{
    {string.Join("\r\n    ", properties)}

    public {nameTo}({string.Join(", ", constructorParameters)})
    {{
        {string.Join("\r\n        ", constructorAssignments)}
    }}

    {string.Join("\r\n    ", nestedPropertiesTo)}
}}

//-------------------------------------

{string.Join("\r\n", nestedTypes)}

";

            return classes;
        }

        public static string GenerateClassesSource(string classes)
        {
            var source =
@"
namespace Test
{
"
    + classes +
@"

}
";
            return source;
        }
    }
}
