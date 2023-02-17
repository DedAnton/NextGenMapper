using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;

namespace NextGenMapperTests.Tests.SpecialCases;

[TestClass]
public class Special : SourceGeneratorVerifier
{
    public override string TestGroup => "SpecialCases";

    [TestMethod]
    public async Task MultipleRunsOneGenerator_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}
";

        var generator = new MapperGenerator();
        var methodName = nameof(MultipleRunsOneGenerator_ShouldMap);

        var generatorResults1 = RunGenerator(new[] { source }, out var sourceErrors1, out var outputCompilation1, generator);
        if (sourceErrors1.Length > 0)
        {
            throw new SourceException(sourceErrors1);
        }
        var functionResult1 = RunMappingFunction(outputCompilation1, methodName);

        var generatorResults2 = RunGenerator(new[] { source }, out var sourceErrors2, out var outputCompilation2, generator);
        if (sourceErrors2.Length > 0)
        {
            throw new SourceException(sourceErrors2);
        }
        var functionResult2 = RunMappingFunction(outputCompilation2, methodName);

        GetPath(out var generatorRunDirectory, out var mapRunDirectory);
        generatorRunDirectory = Path.Combine("..", "..", generatorRunDirectory);
        mapRunDirectory = Path.Combine("..", "..", mapRunDirectory);
        await Task.WhenAll(
            Verify(generatorResults1).UseMySettings(generatorRunDirectory, methodName, "FirstRun"),
            Verify(functionResult1).UseMySettings(mapRunDirectory, methodName, "FirstRun"),
            Verify(generatorResults2).UseMySettings(generatorRunDirectory, methodName, "SecondRun"),
            Verify(functionResult2).UseMySettings(mapRunDirectory, methodName, "SecondRun"));
    }

//    [TestMethod]
//    public async Task Debug()
//    {
//        var source =
//@"using NextGenMapper;

//namespace Test;

//public class Program
//{
//    public object RunTest() => new Source().Map<Destination>();
//}

//public class Source
//{
//    public int SamePropertyName { get; set; } = 1;
//    public int DifferentPropertyNameA { get; set; } = -1;
//    public int differentPropertyNameC { get; set; } = -1;
//    public int differentpropertynamed { get; set; } = -1;
//}

//public class Destination
//{
//    public int SamePropertyName { get; set; } = -1;
//    public int DifferentPropertyNameB { get; set; } = 0;
//    public int DifferentPropertyNameC { get; set; } = 0;
//    public int DifferentPropertyNameD { get; set; } = 0;
//}";

//        var generator = new MapperGenerator();
//        var driver = CSharpGeneratorDriver.Create(generator);
//        var methodName = nameof(Debug);

//        var generatorResults = RunIncrementalGenerator(new[] { source }, out var sourceErrors1, out var outputCompilation1, ref driver);
//        if (sourceErrors1.Length > 0)
//        {
//            throw new SourceException(sourceErrors1);
//        }
//        //var functionResult1 = RunMappingFunction(outputCompilation1, methodName);

//        GetPath(out var generatorRunDirectory, out var mapRunDirectory);
//        generatorRunDirectory = Path.Combine("..", "..", generatorRunDirectory);
//        mapRunDirectory = Path.Combine("..", "..", mapRunDirectory);
//        await Verify(generatorResults).UseMySettings(generatorRunDirectory, methodName, "FirstRun");
//    }

//    private protected GeneratorDriverRunResult RunIncrementalGenerator(string[] sources, out Diagnostic[] sourceErrors, out Compilation outputCompilation, ref CSharpGeneratorDriver driver)
//    {
//        var compilation = CreateCompilation(sources, GetType().Name);
//        var parseOptions = CreateParseOptions();
//        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out var _);

//        sourceErrors = outputCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();

//        return driver.GetRunResult();
//    }
}
