namespace NextGenMapperTests.Tests.AutomaticCustomizedProjection;

[TestClass]
public class Special : SourceGeneratorVerifier
{
    public override string TestGroup => "ConfiguredProjection";

    [TestMethod]
    public Task Struct_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>();
}

public struct Source
{
    public int Property { get; set; }
}

public struct Destination
{
    public int Property { get; set; }

    public int ForMapWith { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task NonGenericIQueryable_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => ((System.Collections.IEnumerable)new[] { new Source() }).AsQueryable().ProjectWith<Destination>();
}

public struct Source
{
    public int Property { get; set; }
}

public struct Destination
{
    public int Property { get; set; }

    public int ForMapWith { get; set; }
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
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Source>();
}

public class Source
{
    public int Property { get; set; }
}";

        return VerifyOnly(source, ignoreSourceErrors: true);
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
        source.ProjectWith<DestinationStruct>();
        source.ProjectWith<DestinationEnum>();
        source.ProjectWith<int[]>();

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
        public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(Property: 1).First();
    }

    public class Source
    {

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

new[] { new Source() }.AsQueryable().ProjectWith<Destination>(Property: 1).First();

public class Source
{
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
    public Task MapAndMapWithAndProjectAndProjectWithTogether_ShouldMapAll()
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
        var mapResult = source.Map<Destination>();
        var mapWithResult = source.MapWith<Destination>(ForMapWith: 1);
        var projectResult = new[] { new Source() }.AsQueryable().Project<Destination>().First();
        var projectWithResult = new[] { new Source() }.AsQueryable().ProjectWith<Destination>(ForMapWith: 1).First();

        return new[] { mapResult, mapWithResult, projectResult, projectWithResult };
    }
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

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task TwoProjectWithMethodsTogether_ShouldMapBoth()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new[] { new Source() }.AsQueryable();
        var projectWithResult1 = source.ProjectWith<Destination>(ForMapWith1: 1).First();
        var projectWithResult2 = source.ProjectWith<Destination>(ForMapWith2: 1).First();

        return new[] { projectWithResult1, projectWithResult2 };
    }
}

public class Source
{
}

public class Destination
{
    public int ForMapWith1 { get; set; }
    public long ForMapWith2 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task TwoSameProjectWithMethodsTogether_ShouldMapBoth()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new[] { new Source() }.AsQueryable();
        var projectWithResult1 = source.ProjectWith<Destination>(ForMapWith1: 1).First();
        var projectWithResult2 = source.ProjectWith<Destination>(ForMapWith1: 2).First();

        return new[] { projectWithResult1, projectWithResult2 };
    }
}

public class Source
{
}

public class Destination
{
    public int ForMapWith1 { get; set; }
    public long ForMapWith2 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task TwoMapWithMethodsAndSameSignature_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new[] { new Source() }.AsQueryable();
        var projectWithResult1 = source.ProjectWith<Destination>(ForMapWith1: 1);
        var projectWithResult2 = source.ProjectWith<Destination>(ForMapWith2: 1);

        return new[] { projectWithResult1, projectWithResult2 };
    }
}

public class Source
{
}

public class Destination
{
    public int ForMapWith1 { get; set; }
    public int ForMapWith2 { get; set; }
}";

        return VerifyOnly(source);
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
    public object RunTest() => new[] { new NestedClass { Property = -1 } }.AsQueryable().ProjectWith<BaseClass>(Property: 1).First();
}

public class BaseClass
{
    public int Property { get; set; }
}

public class NestedClass : BaseClass
{

}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
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
    public object RunTest() => new[] { new Source{ Property = 1 } }.AsQueryable().ProjectWith<Destination>(Property: 1).First();
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

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    [DataRow("Source?", "Destination", "FromNullableClass")]
    [DataRow("Source", "Destination?", "ToNullableClass")]
    [DataTestMethod]
    public async Task NullableReferenceType_ShouldRemoveNullableAnnotation(string from, string to, string variant)
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => MapWrap(new());

    private " + to + @" MapWrap(" + from + @" source) => new[] { source }.AsQueryable().ProjectWith<" + to + @">(ForMapWith: 1).First();
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

        await VerifyOnly(source, ignoreSourceErrors: true, variant: variant);
    }

    [DataRow("Source", "Destination", "Class")]
    [DataTestMethod]
    public async Task NullableAndNotNullableTogether_ShouldGenerateOneMapMethod(string from, string to, string variant)
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

    private " + to + @" FromNullable(IQueryable<" + from + @"?> source) => source.ProjectWith<" + to + @">(ForMapWith1: 1).First();
    private " + to + @"? ToNullable(IQueryable<" + from + @"> source) => source.ProjectWith<" + to + @"?>(ForMapWith2: 1).First();
    private " + to + @"? FromNullableToNullable(IQueryable<" + from + @"?> source) => source.ProjectWith<" + to + @"?>(ForMapWith3: 1).First();
    private " + to + @" NotNullable(IQueryable<" + from + @"> source) => source.ProjectWith<" + to + @">(ForMapWith4: 1).First();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
    public byte ForMapWith1 { get; set; }
    public short ForMapWith2 { get; set; }
    public int ForMapWith3 { get; set; }
    public long ForMapWith4 { get; set; }
}";

        await VerifyOnly(source, ignoreSourceErrors: true, variant: variant);
    }
}