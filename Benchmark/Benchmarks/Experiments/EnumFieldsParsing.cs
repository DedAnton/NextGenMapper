using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net50)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumFieldsParsing
{
    private EnumDeclarationSyntax enumDeclaration;
    private SemanticModel enumSemanticModel;

    [GlobalSetup(Targets = new string[] { nameof(FromSyntaxOptimized), nameof(FromSyntax), nameof(FromSymbols) })]
    public void SetupBenchmark()
    {
        var source = @"
public namespace Test
{
    public enum MyEnum
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10
    }
}";
        var compilation = source.CreateCompilation("test");
        enumDeclaration = compilation.GetTypeByMetadataName("Test.MyEnum").GetFirstDeclaration() as EnumDeclarationSyntax;
        enumSemanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees.First());
    }

    [BenchmarkCategory("EnumFields"), Benchmark]
    public List<EnumField> FromSyntaxOptimized()
    {
        var fields = new List<EnumField>(enumDeclaration.Members.Count);

        foreach (var member in enumDeclaration.Members)
        {
            fields.Add(new EnumField(
                member.Identifier.ValueText,
                (member.EqualsValue?.Value as LiteralExpressionSyntax)?.Token.Value?.UnboxToLong()));
        }

        return fields;
    }

    [BenchmarkCategory("EnumFields"), Benchmark(Baseline = true)]
    public List<EnumField> FromSyntax() => enumDeclaration.Members.Select(x => new EnumField(x.Identifier.ValueText, x.EqualsValue?.Value?.As<LiteralExpressionSyntax>()?.Token.Value?.UnboxToLong())).ToList();

    [BenchmarkCategory("EnumFields"), Benchmark]
    public List<IFieldSymbol> FromSymbols() => enumDeclaration.Members.Select(x => enumSemanticModel.GetDeclaredSymbol(x)).OfType<IFieldSymbol>().ToList();
}
