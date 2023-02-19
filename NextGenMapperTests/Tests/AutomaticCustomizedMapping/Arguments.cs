namespace NextGenMapperTests.Tests.AutomaticCustomizedMapping;

[TestClass]
public class Arguments : SourceGeneratorVerifier
{
    public override string TestGroup => "MapWith";

    [TestMethod]
    public Task UserProvideNoOneArguments_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
    public int ForMapWith { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task UserProvideHalfArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith1: 1, ForMapWith2: 1);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = 1;
}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int ForMapWith1 { get; set; }
    public int ForMapWith2 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UserProvideAllArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(
        Property1: 1,
        Property2: 1,
        Property3: 1,
        Property4: 1);
}

public class Source
{

}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }
    public int Property4 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ArgumentsOrderDoesNotCorrespondProperties_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(
        Property4: 4,
        Property1: 1,
        Property3: 3,
        Property2: 2);
}

public class Source
{

}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }
    public int Property4 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ArgumentsOrderDoesNotCorrespondParameters_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(
        property4: 4,
        property1: 1,
        property3: 3,
        property2: 2);
}

public class Source
{

}

public class Destination
{
    public int Property1 { get; }
    public int Property2 { get; }
    public int Property3 { get; }
    public int Property4 { get; }

    public Destination(int property1, int property2, int property3, int property4)
    {
        Property1 = property1;
        Property2 = property2;
        Property3 = property3;
        Property4 = property4;
    }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnnamedArgument_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(1);
}

public class Source
{

}

public class Destination
{
    public int Property { get; set; }
}";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }
}
