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
}
