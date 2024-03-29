﻿namespace NextGenMapperTests.Tests.AutomaticCustomizedMapping;

[TestClass]
public class Special : SourceGeneratorVerifier
{
    public override string TestGroup => "ConfiguredMap";

    [TestMethod]
    public Task CommonGeneric_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<int> { Property = 1 }.MapWith<Destination<int>>(ForMapWith: 1);
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }

    public T ForMapWith { get; set; }
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
    public object RunTest() => new Source<A> { Property = new A(1) }.MapWith<Destination<B>>(ForMapWith: 1);
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }

    public int ForMapWith { get; set; }
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
    public object RunTest() => new Source().MapWith<Destination>();
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

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Source>();
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

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        source.MapWith<DestinationStruct>();
        source.MapWith<DestinationEnum>();
        source.MapWith<int[]>();

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
        public object RunTest() => new Source().MapWith<Destination>(Property: 1);
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

new Source().MapWith<Destination>(Property: 1);

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
    public Task MapAndMapWithTogether_ShouldMapBoth()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        var mapResult = source.Map<Destination>();
        var mapWithResult = source.MapWith<Destination>(ForMapWith: 1);

        return new[] { mapResult, mapWithResult };
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
    public Task TwoMapWithMethodsTogether_ShouldMapBoth()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        var mapWithResult1 = source.MapWith<Destination>(ForMapWith1: 1);
        var mapWithResult2 = source.MapWith<Destination>(ForMapWith2: 1);

        return new[] { mapWithResult1, mapWithResult2 };
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
    public Task TwoSameMapWithMethodsTogether_ShouldMapBoth()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        var mapWithResult1 = source.MapWith<Destination>(ForMapWith1: 1);
        var mapWithResult2 = source.MapWith<Destination>(ForMapWith1: 2);

        return new[] { mapWithResult1, mapWithResult2 };
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

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source();
        var mapWithResult1 = source.MapWith<Destination>(ForMapWith1: 1);
        var mapWithResult2 = source.MapWith<Destination>(ForMapWith2: 1);

        return new[] { mapWithResult1, mapWithResult2 };
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

namespace Test;

public class Program
{
    public object RunTest() => new NestedClass { Property = -1 }.MapWith<BaseClass>(Property: 1);
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

namespace Test;

public class Program
{
    public object RunTest() => new Source { Property = 1 }.MapWith<Destination>(Property: 1);
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

namespace Test;

public class Program
{
    public object RunTest() => MapWrap(new());

    private " + to + @" MapWrap(" + from + @" source) => source.MapWith<" + to + @">(ForMapWith: 1);
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

    private " + to + @" FromNullable(" + from + @"? source) => source.MapWith<" + to + @">(ForMapWith1: 1);
    private " + to + @"? ToNullable(" + from + @" source) => source.MapWith<" + to + @"?>(ForMapWith2: 1);
    private " + to + @"? FromNullableToNullable(" + from + @"? source) => source.MapWith<" + to + @"?>(ForMapWith3: 1);
    private " + to + @" NotNullable(" + from + @" source) => source.MapWith<" + to + @">(ForMapWith4: 1);
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
        await VerifyOnly(source, variant: variant);
    }
}
