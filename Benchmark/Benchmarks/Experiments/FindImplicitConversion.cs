using Benchmark.Utils;
using NextGenMapper.Extensions;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class FindImplicitConversion
{
    private SemanticModel semanticModel;
    private ITypeSymbol byteSymbol;
    private ITypeSymbol longSymbol;

    [GlobalSetup]
    public void Setup()
    {
        var source = @"
public namespace Test
{
    public class MyClass
    {
        public byte PropertyByte { get; }
        public long PropertyLong { get; }
    }
}";
        var compilation = CompilationHelper.CreateCompilation(new[] { source }, "bench");
        semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees.First());
        var classSymbol = compilation.GetTypeByMetadataName("Test.MyClass");
        byteSymbol = classSymbol.GetPublicProperties()[0].Type;
        longSymbol = classSymbol.GetPublicProperties()[1].Type;
    }

    [Benchmark]
    public bool ByMicrosoft() => semanticModel.Compilation.HasImplicitConversion(byteSymbol, longSymbol);

    [Benchmark]
    public bool ByMe() => ImplicitNumericConversionValidator.HasImplicitConversion(byteSymbol, longSymbol);
}

public static class ImplicitNumericConversionValidator
{
    private static readonly Dictionary<SpecialType, HashSet<SpecialType>> _implicitNumericConversions =
        new()
        {
            { SpecialType.System_SByte, new() { SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Byte, new() { SpecialType.System_Int16, SpecialType.System_UInt16, SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Int16, new() { SpecialType.System_Int32, SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_UInt16, new() { SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Int32, new() { SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_UInt32, new() { SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Int64, new() { SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_UInt64, new() { SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Char, new() { SpecialType.System_UInt16, SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
            { SpecialType.System_Single, new() { SpecialType.System_Double } }
        };

    public static bool HasImplicitConversion(ITypeSymbol from, ITypeSymbol to)
        => _implicitNumericConversions.TryGetValue(from.SpecialType, out var implicitTypes) && implicitTypes.Contains(to.SpecialType);
}
