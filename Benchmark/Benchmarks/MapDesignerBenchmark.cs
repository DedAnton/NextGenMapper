using Benchmark.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;

namespace Benchmark.Benchmarks;

[SimpleJob(RuntimeMoniker.Net50)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class MapDesignerBenchmark
{
    [BenchmarkCategory("Properties"), Benchmark]
    [ArgumentsSource(nameof(GenerateCommonClassesMapPairs))]
    public List<ClassMap> Properties(TypesMapPair mapPair) => new ClassMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To);

    [BenchmarkCategory("NestedClasses"), Benchmark]
    [ArgumentsSource(nameof(GenerateNestedClassesMapPairs))]
    public List<ClassMap> Nested(TypesMapPair mapPair) => new ClassMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To);

    [BenchmarkCategory("Enums"), Benchmark]
    [ArgumentsSource(nameof(GenerateEnumsMapPairs))]
    public EnumMap Enums(TypesMapPair mapPair) => new EnumMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To);

    [BenchmarkCategory("Partial"), Benchmark]
    [ArgumentsSource(nameof(GeneratePartialMapPairs))]
    public List<ClassMap> Partial(TypesMapPair mapPair) => new ClassPartialMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To, mapPair.Constructor, mapPair.Method);

    [BenchmarkCategory("PartialConstructor"), Benchmark]
    [ArgumentsSource(nameof(GeneratePartialMapPairs))]
    public List<ClassMap> PartialConstructor(TypesMapPair mapPair) => new ClassPartialConstructorMapDesigner(new()).DesignMapsForPlanner(mapPair.From, mapPair.To, mapPair.Constructor, mapPair.Method);

    public IEnumerable<TypesMapPair> GenerateCommonClassesMapPairs()
    {
        var groups = new List<(string Description, string Classes)>()
        {
            ("props_1_init", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 0, 0)),
            ("props_1_cstr", TestTypeSourceGenerator.GenerateClassMapPair(1, 1, 0, 0)),
            ("props_10_init", TestTypeSourceGenerator.GenerateClassMapPair(10, 0, 0, 0)),
            ("props_10_cstr", TestTypeSourceGenerator.GenerateClassMapPair(10, 10, 0, 0)),
            ("props_100_init", TestTypeSourceGenerator.GenerateClassMapPair(100, 0, 0, 0)),
            ("props_100_cstr", TestTypeSourceGenerator.GenerateClassMapPair(100, 100, 0, 0)),
        };

        foreach (var (Description, Classes) in groups)
        {
            var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
            var compilation = source.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.Source");
            var to = compilation.GetTypeByMetadataName("Test.Destination");

            yield return new TypesMapPair(Description, from, to);
        }
    }

    public IEnumerable<TypesMapPair> GenerateNestedClassesMapPairs()
    {
        var groups = new List<(string Description, string Classes)>()
        {
            ("depth_1", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 1)),
            ("depth_2", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 2)),
            ("depth_3", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 3)),
            ("depth_4", TestTypeSourceGenerator.GenerateClassMapPair(1, 0, 1, 4)),
        };

        foreach (var (Description, Classes) in groups)
        {
            var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
            var compilation = source.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.Source");
            var to = compilation.GetTypeByMetadataName("Test.Destination");

            yield return new TypesMapPair(Description, from, to);
        }
    }

    public IEnumerable<TypesMapPair> GenerateEnumsMapPairs()
    {
        var groups = new List<(string Description, string Classes)>()
        {
            ("fields_1", TestTypeSourceGenerator.GenerateEnumMapPair(1, 0)),
            ("fields_10", TestTypeSourceGenerator.GenerateEnumMapPair(10, 0)),
            ("fields_100", TestTypeSourceGenerator.GenerateEnumMapPair(100, 0))
        };

        foreach (var (Description, Classes) in groups)
        {
            var source = TestTypeSourceGenerator.GenerateClassesSource(Classes);
            var compilation = source.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.SourceEnum");
            var to = compilation.GetTypeByMetadataName("Test.DestinationEnum");

            yield return new TypesMapPair(Description, from, to);
        }
    }

    public IEnumerable<TypesMapPair> GeneratePartialMapPairs()
    {
        var c1 = SyntaxGenerator.GeneratePartialMapMethodAndSourceCode(1);
        var c10 = SyntaxGenerator.GeneratePartialMapMethodAndSourceCode(10);
        var c100 = SyntaxGenerator.GeneratePartialMapMethodAndSourceCode(100);
        var groups = new List<(string Description, string Classes, MethodDeclarationSyntax Method)>()
        {
            ("partial_1", c1.SourceCode, c1.Method),
            ("partial_10", c10.SourceCode, c10.Method),
            ("partial_100", c100.SourceCode, c100.Method)
        };

        foreach (var (Description, SourceCode, Method) in groups)
        {
            var compilation = SourceCode.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.Source");
            var to = compilation.GetTypeByMetadataName("Test.Destination");
            var constructor = to.GetPublicConstructors().ToArray().First();

            yield return (new TypesMapPair(Description, from, to) { Constructor = constructor, Method = Method });
        }
    }

    public IEnumerable<TypesMapPair> GeneratePartialConstructorMapPairs()
    {
        var c1 = SyntaxGenerator.GeneratePartialConstructorMapMethodAndSourceCode(1);
        var c10 = SyntaxGenerator.GeneratePartialConstructorMapMethodAndSourceCode(10);
        var c100 = SyntaxGenerator.GeneratePartialConstructorMapMethodAndSourceCode(100);
        var groups = new List<(string Description, string Classes, MethodDeclarationSyntax Method)>()
        {
            ("partial_ctr_1", c1.SourceCode, c1.Method),
            ("partial_ctr_10", c10.SourceCode, c10.Method),
            ("partial_ctr_100", c100.SourceCode, c100.Method)
        };

        foreach (var (Description, SourceCode, Method) in groups)
        {
            var compilation = SourceCode.CreateCompilation("test");
            var from = compilation.GetTypeByMetadataName("Test.Source");
            var to = compilation.GetTypeByMetadataName("Test.Destination");
            var constructor = to.GetPublicConstructors().ToArray().First();

            yield return (new TypesMapPair(Description, from, to) { Constructor = constructor, Method = Method });
        }
    }
}

public class TypesMapPair
{
    public TypesMapPair(string name, ITypeSymbol from, ITypeSymbol to)
    {
        Name = name;
        From = from;
        To = to;
    }

    public string Name { get; set; }
    public ITypeSymbol From { get; set; }
    public ITypeSymbol To { get; set; }
    public IMethodSymbol Constructor { get; set; }
    public MethodDeclarationSyntax Method { get; set; }

    public override string ToString() => Name;
}
