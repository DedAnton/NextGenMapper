namespace NextGenMapperTests.Tests.AutomaticProjection;

[TestClass]
public class Constructor : SourceGeneratorVerifier
{
    public override string TestGroup => "Project";

    [TestMethod]
    public Task ClassWithoutDefaultConstructor_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination>();
}

public class Source
{
    public int Property { get; set; }
}

public class Destination
{
    public int Property { get; }

    public Destination(int property)
    {
        Property = property;
    }
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task DefinedConstructorWithNoArgumentsConstructor_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination>();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; } = -1;

    public Destination()
    {
    }
}
";

        return VerifyAndRun(source);
    }
}