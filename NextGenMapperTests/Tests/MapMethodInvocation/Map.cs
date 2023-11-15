namespace NextGenMapperTests.Tests.MapMethodInvocation;

[TestClass]
public class Map : SourceGeneratorVerifier
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
        return source.Map<Destination>();
    }
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

        var destinationCollection = sourceArray.Select(x => x.Map<Destination>());

        return destinationCollection.First();
    }
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
    public object RunTest() => GetSource().Map<Destination>();

    private Source GetSource() => new Source();
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
    public object RunTest() => new Source { Property = 1 }.Map<Destination>();
}

public class Source
{
    public int Property { get; set; }
}

public class Destination
{
    public int Property { get; set; }
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
    public object RunTest() => Source.Map<Destination>();
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
    public object RunTest() => source.Map<Destination>();
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task InvocateOnElvisOperator_ShouldMap()
    {
        var source =
@"#nullable enable
using NextGenMapper;

namespace Test;

public class Program
{
    public object? RunTest()
    {
        Source? source = new Source();
        return source?.Map<Destination>();
    }
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

        return VerifyAndRun(source);
    }
}
