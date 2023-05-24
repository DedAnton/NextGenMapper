namespace NextGenMapperTests.Tests.AutomaticMapping;

[TestClass]
public class Enums : SourceGeneratorVerifier
{
    public override string TestGroup => "Map";

    [TestMethod]
    public Task NamesAreEquals_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => Source.B.Map<Destination>();
}

public enum Source
{
    A = 1,
    B = 2,
    C = 3
}

public enum Destination
{
    A = 4,
    B = 5,
    C = 6
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task NamesAreEqualsButDifferentCase_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => Source.B.Map<Destination>();
}

public enum Source
{
    A = 1,
    B = 2,
    c = 3
}

public enum Destination
{
    A = 4,
    b = 5,
    C = 6
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ValuesAreEquals_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => Source.B.Map<Destination>();
}

public enum Source
{
    A = 1,
    B = 2,
    C = 3
}

public enum Destination
{
    D = 1,
    E = 2,
    F = 3
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task NotMappedValue_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => Source.B.Map<Destination>();
}

public enum Source
{
    A,
    B,
    C = 7
}

public enum Destination
{
    A,
    B,
    D = 11
}";

        return VerifyOnly(source);
    }

    [DataRow("sbyte")]
    [DataRow("byte")]
    [DataRow("short")]
    [DataRow("ushort")]
    [DataRow("int")]
    [DataRow("uint")]
    [DataRow("long")]
    [DataTestMethod]
    public Task DifferentUnderlyingTypes_ShouldMap(string underlyingType)
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Destination[] { Source.Min.Map<Destination>(), Source.Normal.Map<Destination>(), Source.Max.Map<Destination>() };
}

public enum Source : " + underlyingType + @"
{
    Min = " + underlyingType + @".MinValue,
    Normal = 1,
    Max = " + underlyingType + @".MaxValue
}

public enum Destination : " + underlyingType + @"
{
    Min = " + underlyingType + @".MinValue,
    Normal = 1,
    Max = " + underlyingType + @".MaxValue
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UlongUnderlyingType_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => Source.Min.Map<Destination>();
}

public enum Source : ulong
{
    Min = ulong.MinValue,
    Normal = 1,
    Max = ulong.MaxValue
}

public enum Destination : ulong
{
    Min = ulong.MinValue,
    Normal = 1,
    Max = ulong.MaxValue
}";

        return VerifyOnly(source);
    }
}
