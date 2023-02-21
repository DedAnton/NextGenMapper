namespace NextGenMapperTests.Tests.CompletelyCustomMapping;

[TestClass]
public class Class : SourceGeneratorVerifier
{
    public override string TestGroup => "UserMap";

    [TestMethod]
    public Task ClassNameNotMapper_ShouldMap()
    {
        var source1 =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int PropertyA { get; set; } = 1;
}

public class Destination
{
    public int PropertyB { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static class MyOwnMapperClassName
    {
        internal static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }
}
