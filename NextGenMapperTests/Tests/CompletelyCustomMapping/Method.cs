namespace NextGenMapperTests.Tests.CompletelyCustomMapping;

[TestClass]
public class Method : SourceGeneratorVerifier
{
    public override string TestGroup => "UserMap";

    [TestMethod]
    public Task WrongMethodName_ShouldNotMap()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination MyOwnMapMethodName<To>(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodWithoutParameters_ShouldNotMap()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>() => new Destination { PropertyB = 1 };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodWithMultipleParameters_ShouldNotMap()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this Source source, int secondParameter) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodNotExtension_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyOnly(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodNotGeneric_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyOnly(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodHasTwoTypeParameters_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To, SecondTypeParameter>(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyOnly(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodReturnsVoid_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static void Map<To>(this Source source) { }
    }
}
";

        return VerifyOnly(new string[] { source1, source2 }, ignoreSourceErrors: true);
    }

    [TestMethod]
    public Task MethodReturnsByRef_ShouldMap()
    {
        var source1 =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1).Map<int>();
}

public class Source
{
    private int _property;
    public ref int Property => ref _property;

    public Source(int property) => _property = property;
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static ref int Map<To>(this Source source) => ref source.Property;
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodReturnsByRefReadonly_ShouldMap()
    {
        var source1 =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1).Map<int>();
}

public class Source
{
    private int _property;
    public ref readonly int Property => ref _property;

    public Source(int property) => _property = property;
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static ref readonly int Map<To>(this Source source) => ref source.Property;
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodIsPrivate_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        private static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyOnly(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodIsPublic_Diagnostic()
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

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        public static Destination Map<To>(this Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyOnly(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodWithInParameter_ShouldMap()
    {
        var source1 =
@"using NextGenMapper;

namespace Test;

public class Program
{
    private Source _source = new Source();
    public ref Source Source => ref _source;

    public object RunTest() => Source.Map<Destination>();
}

public struct Source
{
    public int PropertyA { get; set; }

    public Source()
    {
        PropertyA = 1;
    }
}

public class Destination
{
    public int PropertyB { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this in Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }

    [TestMethod]
    public Task MethodWithRefParameter_ShouldMap()
    {
        var source1 =
@"using NextGenMapper;

namespace Test;

public class Program
{
    private Source _source = new Source();
    public ref Source Source => ref _source;

    public object RunTest() => Source.Map<Destination>();
}

public struct Source
{
    public int PropertyA { get; set; }

    public Source()
    {
        PropertyA = 1;
    }
}

public class Destination
{
    public int PropertyB { get; set; }
}
";

        var source2 =
@"using NextGenMapper;
using Test;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Destination Map<To>(this ref Source source) => new Destination { PropertyB = source.PropertyA };
    }
}
";

        return VerifyAndRun(new string[] { source1, source2 });
    }
}
