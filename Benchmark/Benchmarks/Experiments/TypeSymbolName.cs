using Benchmark.Utils;
using NextGenMapper.Utils;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class TypeSymbolName
{
    private ITypeSymbol classSymbol;

    [GlobalSetup]
    public void Setup()
    {
        var source = @"
public namespace Test1.Test2.Test3
{
    public namespace Test4.Test5
    {
        public class MyClass
        {
        
        }
    }
}";
        var compilation = CompilationHelper.CreateCompilation(new[] { source }, "bench");
        classSymbol = compilation.GetTypeByMetadataName("Test1.Test2.Test3.Test4.Test5.MyClass");
    }

    //[Benchmark]
    public string ToStringCommon() => classSymbol.ToString();

    //[Benchmark]
    public string ToDisplayString() => classSymbol.ToDisplayString();

    //[Benchmark]
    public string ToDisplayStringNotNullable() => classSymbol.ToDisplayString(NullableFlowState.None);

    [Benchmark]
    public string MyToString() => MyToString(classSymbol);

    private string MyToString(ITypeSymbol typeSymbol)
    {
        var sb = new ValueStringBuilder();
        ISymbol currentSymbol = typeSymbol;
        while (!currentSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.Append(currentSymbol.Name);
            sb.Append('.');
            currentSymbol = currentSymbol.ContainingNamespace;
        }

        return sb.ToString();
    }
}
