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

    [TestMethod]
    public Task MappableTypesHasImplicitConversion_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new NestedClass { Property = 1 }.Map<BaseClass>();
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

    public static implicit operator Destination(Source source)
    {
        return new Destination { Property = source.Property };
    }
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MappableCollectionsHasImplicitConversion_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest() => new int[]{ 1, 2, 3 }.Map<IEnumerable<int>>();
}
";

        return VerifyOnly(source);
    }

    [DataRow("Source?", "Destination", "FromNullableClass")]
    [DataRow("List<Source>?", "List<Destination>", "FromNullableCollectionList")]
    [DataRow("Source[]?", "List<Destination>", "FromNullableCollectionArray")]
    [DataRow("IEnumerable<Source>?", "List<Destination>", "FromNullableCollectionIEnumerable")]
    [DataRow("List<Source?>?", "List<Destination>", "FromNullableCollectionItem")]
    [DataTestMethod]
    [ExpectedException(typeof(NullableException))]
    public async Task FromNullableReferenceType_ShouldRemoveNullableAnnotation(string from, string to, string variant)
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest() => MapWrap(null);

    private " + to + @" MapWrap(" + from + @" source) => source.Map<" + to + @">();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}";

        await VerifyOnly(source, ignoreSourceErrors: true, variant: variant);
        await VerifyOnly(source, variant: variant);
    }

    [DataRow("Source", "Destination?", "ToNullableClass")]
    [DataRow("List<Source>", "List<Destination>?", "ToNullableCollection")]
    [DataRow("List<Source>", "List<Destination?>", "ToNullableCollectionItem")]
    [DataTestMethod]
    public async Task ToNullableReferenceType_ShouldRemoveNullableAnnotation(string from, string to, string variant)
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object? RunTest() => MapWrap(new());

    private " + to + @" MapWrap(" + from + @" source) => source.Map<" + to + @">();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}";

        await VerifyAndRun(source, variant: variant);
    }

    [DataRow("Source", "Destination", "Class")]
    [DataRow("List<Source>", "List<Destination>", "Collection")]
    [DataRow("List<Source>", "List<Destination>", "CollectionItem")]
    [DataTestMethod]
    [ExpectedException(typeof(NullableException))]
    public async Task NullableAndNotNullableTogether_ShouldGenerateOneMapMethod(string from, string to, string variant)
    {
        var source =
@"#nullable enable
using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest() => 1;

    private " + to + @" FromNullable(" + from + @"? source) => source.Map<" + to + @">();
    private " + to + @"? ToNullable(" + from + @" source) => source.Map<" + to + @"?>();
    private " + to + @"? FromNullableToNullable(" + from + @"? source) => source.Map<" + to + @"?>();
    private " + to + @" NotNullable(" + from + @" source) => source.Map<" + to + @">();
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; set; }
}";

        await VerifyOnly(source, ignoreSourceErrors: true, variant: variant);
        await VerifyOnly(source, variant: variant);
    }
}
