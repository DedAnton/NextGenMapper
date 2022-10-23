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

        var directory1 = GetPath(methodName, variant: "FirstRun");
        var directory2 = GetPath(methodName, variant: "SecondRun");
        await Task.WhenAll(
            Verify(generatorResults1).UseGeneratorResultSettings(Path.Combine("..", "..", directory1)),
            Verify(functionResult1).UseMapResultSettings(Path.Combine("..", "..", directory1)),
            Verify(generatorResults2).UseGeneratorResultSettings(Path.Combine("..", "..", directory2)),
            Verify(functionResult2).UseMapResultSettings(Path.Combine("..", "..", directory2)));
    }
}
