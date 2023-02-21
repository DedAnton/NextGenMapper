namespace NextGenMapperTests.Tests.CompletelyCustomMapping;

[TestClass]
public class Namespace : SourceGeneratorVerifier
{
    public override string TestGroup => "UserMap";

    [TestMethod]
    public Task FileScopedNamespace_ShouldMap()
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
    public string PropertyB { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper;

internal static partial class Mapper
{
    internal static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA.ToString() };
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task WrongNamespace_ShouldNotMap()
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
    public int PropertyA { get; set; } = -1;
    public int ProeprtyForAutoMapping { get; set; } = 1;
}

public class Destination
{
    public int PropertyB { get; set; }
    public int ProeprtyForAutoMapping { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace MyOwnNamespace;

internal static partial class Mapper
{
    internal static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA };
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }
}
