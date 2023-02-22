namespace NextGenMapperTests.Tests.SpecialCases;

[TestClass]
public class TypesFromDll : SourceGeneratorVerifier
{
    public override string TestGroup => "SpecialCases";

    [TestMethod]
    public Task FromEnumFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => EnumFromDll.B.Map<Destination>();
}

public enum Destination
{
    A,
    B,
    C
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ToEnumFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => Source.B.Map<EnumFromDll>();
}

public enum Source
{
    A,
    B,
    C
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task FromClassFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new ClassFromDll { PropertyA = 1, PropertyB = -1 }.Map<Destination>();
}

public class Destination
{
    public int PropertyA { get; set; }
    public int propertyB { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ToClassFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new Source { PropertyA = 1, propertyB = -1 }.Map<ClassFromDll>();
}

public class Source
{
    public int PropertyA { get; set; }
    public int propertyB { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task FromClassWithConstructorFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new ClassWithConstructorFromDll(1, 1, 1).Map<Destination>();
}

public class Destination
{
    public int PropertyA { get; set; }
    public int PropertyB { get; set; }
    public int PropertyC { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ToClassWithConstructorFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<ClassWithConstructorFromDll>();
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
    public int PropertyC { get; set; } = 1;
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task FromRecordFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new RecordFromDll(1).Map<Destination>();
}

public record Destination(int Property);";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ToRecordFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1).Map<RecordFromDll>();
}

public record Source(int Property);";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConfiguredMapFromClassFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new ClassFromDll { PropertyA = 1, PropertyB = -1 }.MapWith<Destination>(ForMapWith: 1);
}

public class Destination
{
    public int PropertyA { get; set; }
    public int propertyB { get; set; }
    public int ForMapWith { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConfiguredMapToClassFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new Source { PropertyA = 1, propertyB = -1 }.MapWith<ClassFromDll>(PropertyB: 1);
}

public class Source
{
    public int PropertyA { get; set; }
    public int propertyB { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConfiguredMapFromClassWithConstructorFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new ClassWithConstructorFromDll(1, 1, 1).MapWith<Destination>(ForMapWith: 1);
}

public class Destination
{
    public int PropertyA { get; set; }
    public int PropertyB { get; set; }
    public int PropertyC { get; set; }
    public int ForMapWith { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConfiguredMapToClassWithConstructorFromDll_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using TypesFromDllTest;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<ClassWithConstructorFromDll>(pROpeRTyc: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
}";

        return VerifyAndRun(source);
    }
}
