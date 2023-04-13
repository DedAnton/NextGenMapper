namespace NextGenMapperTests.Tests.AutomaticProjection;

[TestClass]
public class Special : SourceGeneratorVerifier
{
    public override string TestGroup => "Projection";

    [TestMethod]
    public Task CommonGeneric_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source<int> { Property = 1} }.AsQueryable().Project<Destination<int>>().First();
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
    public Task GenericWithTypeThatNeedMap_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source<A>() }.AsQueryable().Project<Destination<B>>();
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

        return VerifyOnly(source);
    }

    //TODO: think about what to do with structs
    //[TestMethod]
    public Task Struct_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination>().First();
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
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Source>();
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
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new[] { new Source() }.AsQueryable();
        source.Project<DestinationStruct>();
        source.Project<DestinationEnum>();
        source.Project<int[]>();

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
using System.Linq;

namespace Test
{
    public class Program
    {
        public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination>().First();
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
using System.Linq;

new[] { new Source() }.AsQueryable().Project<Destination>().First();

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

    [TestMethod]
    public Task MappableTypesHasImplicitConversion_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new NestedClass { Property = 1 } }.AsQueryable().Project<BaseClass>();
}

public class BaseClass
{
    public int Property { get; set; }
}

public class NestedClass : BaseClass
{

}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MappableTypesHasUserDefinedImplicitConversion_Diagnostic()
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
    public int Property { get; set; }

    public static implicit operator Destination(Source source)
    {
        return new Destination { Property = source.Property };
    }
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public async Task ToNullableReferenceType_ShouldRemoveNullableAnnotation()
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().Project<Destination?>().First();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}";

        await VerifyAndRun(source);
    }

    [TestMethod]
    public async Task NullableAndNotNullableTogether_ShouldGenerateOneMapMethod()
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => 1;

    private IQueryable<Destination> FromNullable(IQueryable<Source?> source) => source.Project<Destination>();
    private IQueryable<Destination?> ToNullable(IQueryable<Source> source) => source.Project<Destination?>();
    private IQueryable<Destination?> FromNullableToNullable(IQueryable<Source?> source) => source.Project<Destination?>();
    private IQueryable<Destination> NotNullable(IQueryable<Source> source) => source.Project<Destination>();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}";

        await VerifyOnly(source, ignoreSourceErrors: true);
    }
}
