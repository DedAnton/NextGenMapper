namespace NextGenMapperTests.Tests.AutomaticProjection;

[TestClass]
public class All : SourceGeneratorVerifier
{
    public override string TestGroup => "Project";

    [TestMethod]
    public Task SimpleProjection_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination>().First();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; } = -1;
}
";

        return VerifyAndRun(source);
    }
}
