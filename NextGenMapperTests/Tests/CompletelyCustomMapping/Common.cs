namespace NextGenMapperTests.Tests.CompletelyCustomMapping;

[TestClass]
public class Common : SourceGeneratorVerifier
{
    public override string TestGroup => "UserMap";

    [TestMethod]
    public Task TypesCanNotBeMapped_ShouldMapUsingCustomMethod()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA.ToString() };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task TypesCanBeMapped_ShouldMapUsingCustomMethod()
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
    public int Property { get; set; } = -1;
}

public class Destination
{
    public int Property { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source) => new Destination { Property = -1 * source.Property };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task UsingMapWithMethod_ShouldMap()
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
    public int Property { get; set; } = -1;
}

public class Destination
{
    public int Property { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source) => source.MapWith<Destination>(Property: source.Property * -1);
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }
}
