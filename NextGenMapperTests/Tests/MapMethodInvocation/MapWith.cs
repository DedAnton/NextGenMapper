namespace NextGenMapperTests.Tests.MapMethodInvocation;

[TestClass]
public class MapWith : SourceGeneratorVerifier
{
    public override string TestGroup => "MapMethodInvocation";

    [TestMethod]
    public Task InvocateOnVariable_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        return source.MapWith<Destination>(propertyB: source.PropertyA);
    }
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnVariableInLambda_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        var sourceArray = new Source[] { source };

        var destinationCollection = sourceArray.Select(x => x.MapWith<Destination>(propertyB: source.PropertyA));

        return destinationCollection.First();
    }
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnMethod_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => GetSource().MapWith<Destination>(propertyB: GetSource().PropertyA);

    private Source GetSource() => new Source();
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnConstructor_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(propertyB: new Source().PropertyA);
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnInitializer_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source { PropertyA = 1 }.MapWith<Destination>(propertyB: new Source { PropertyA = 1 }.PropertyA);
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnProeprty_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    private Source Source => new Source();
    public object RunTest() => Source.MapWith<Destination>(propertyB: Source.PropertyA);
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnField_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    private Source source = new Source();
    public object RunTest() => source.MapWith<Destination>(propertyB: source.PropertyA);
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

        return VerifyAndRun(source);
    }
}
