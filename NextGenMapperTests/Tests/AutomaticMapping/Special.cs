namespace NextGenMapperTests.Tests.AutomaticMapping;

[TestClass]
public class Special : SourceGeneratorVerifier
{
    public override string TestGroup => "Map";

    [TestMethod]
    public Task CommonGeneric_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<int> { Property = 1 }.Map<Destination<int>>();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task GenericWithTypeThatNeedMap_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<A> { Property = new A(1) }.Map<Destination<B>>();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }
}

public record A(int InnerProperty);
public record B(int InnerProperty);
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task Struct_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public struct Source
{
    public int Property { get; set; }
}

public struct Destination
{
    public int Property { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MappedTypesAreEquals_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Source>();
}

public class Source
{
    public int Property { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task DifferentKinds_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        source.Map<DestinationStruct>();
        source.Map<DestinationEnum>();
        source.Map<int[]>();

        return -1;
    }
}

public class Source
{
    public int Property { get; set; }
}

public struct DestinationStruct
{
    public int Property { get; set; }
}

public enum DestinationEnum
{
    A,
    B
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task NormalScopedNamespace_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test
{
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
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task GlobalNamespace_ShouldMap()
    {
        var source =
@"using NextGenMapper;

new Source().Map<Destination>();

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}
";

        OutputKind = Microsoft.CodeAnalysis.OutputKind.ConsoleApplication;

        var result = VerifyOnly(source);

        OutputKind = Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary;

        return result;
    }
}
