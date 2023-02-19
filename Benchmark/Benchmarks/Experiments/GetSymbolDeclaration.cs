using Benchmark.Utils;
using Microsoft.CodeAnalysis.CSharp;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class GetSymbolDeclaration
{
    private ISymbol constructorSymbol;

    [GlobalSetup]
    public void Setup()
    {
        var source = @"
public namespace Test
{
    public class MyClass
    {
        public MyClass(
            int property1, 
            int property2, 
            int property3, 
            int property4, 
            int property5, 
            int property6, 
            int property7, 
            int property8, 
            int property9, 
            int property10)
        {
            Property1 = property1;
            Property2 = property2;
            Property3 = property3;
            Property4 = property4;
            Property5 = property5;
            Property6 = property6;
            Property7 = property7;
            Property8 = property8;
            Property9 = property9;
            Property10 = property10;
        }

        public int Property1 { get; }
        public int Property2 { get; }
        public int Property3 { get; }
        public int Property4 { get; }
        public int Property5 { get; }
        public int Property6 { get; }
        public int Property7 { get; }
        public int Property8 { get; }
        public int Property9 { get; }
        public int Property10 { get; }
    }
}";
        var compilation = CompilationHelper.CreateCompilation(new[] { source }, "bench");
        constructorSymbol = compilation.GetTypeByMetadataName("Test.MyClass").Constructors[0];
    }

    [Benchmark]
    public SyntaxNode DeclaringSyntaxReferences() => GetFirstDeclaration(constructorSymbol);

    [Benchmark]
    public SyntaxNode GetCompilationUnitRoot() => GetDeclarationFromLocation(constructorSymbol);

    private SyntaxNode GetFirstDeclaration(ISymbol symbol)
    {
        if (symbol.DeclaringSyntaxReferences.Length > 0)
        {
            return symbol.DeclaringSyntaxReferences[0].GetSyntax();
        }

        return null;
    }

    private SyntaxNode GetDeclarationFromLocation(ISymbol symbol)
    {
        var location = symbol.Locations.FirstOrDefault();
        if (location is not null && location.SourceTree is not null)
        {
            return location.SourceTree.GetCompilationUnitRoot().FindNode(location.SourceSpan);
        }

        return null;
    }
}
