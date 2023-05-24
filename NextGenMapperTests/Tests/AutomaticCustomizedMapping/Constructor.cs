namespace NextGenMapperTests.Tests.AutomaticCustomizedMapping;

[TestClass]
public class Constructor : SourceGeneratorVerifier
{
    public override string TestGroup => "ConfiguredMap";

    [TestMethod]
    public Task ConstructorInitializeSingleProperty_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(propertyB: new Source().PropertyA);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
}

public class Destination
{
    public int PropertyB { get; }

    public Destination(int propertyB)
    {
        PropertyB = propertyB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConstructorInitializeTwoProperties_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }

    public int ForMapWith { get; set; }

    public Destination(int property, int forMapWith)
    {
        Property = property;
        ForMapWith = forMapWith;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ConstructorInitializeManyProperties_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(
        property9: 1,
        property10: 1,
        property11: 1,
        property12: 1,
        property13: 1,
        property14: 1,
        property15: 1,
        property16: 1);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = 1;
    public int Property3 { get; set; } = 1;
    public int Property4 { get; set; } = 1;
    public int Property5 { get; set; } = 1;
    public int Property6 { get; set; } = 1;
    public int Property7 { get; set; } = 1;
    public int Property8 { get; set; } = 1;
}

public class Destination
{
    public int Property1 { get; } = -1;
    public int Property2 { get; } = -1;
    public int Property3 { get; } = -1;
    public int Property4 { get; } = -1;
    public int Property5 { get; } = -1;
    public int Property6 { get; } = -1;
    public int Property7 { get; } = -1;
    public int Property8 { get; } = -1;
    public int Property9 { get; } = -1;
    public int Property10 { get; } = -1;
    public int Property11 { get; } = -1;
    public int Property12 { get; } = -1;
    public int Property13 { get; } = -1;
    public int Property14 { get; } = -1;
    public int Property15 { get; } = -1;
    public int Property16 { get; } = -1;

    public Destination(int property1, int property2, int property3, int property4, int property5, int property6, int property7, int property8, int property9, int property10, int property11, int property12, int property13, int property14, int property15, int property16)
    {
        Property1 = property1;
        Property2 = property2;
        Property3 = property3;
        Property4 = property4;
        Property5 = property5;
        Property6 = property6;
        Property7 = property7;
        Property8 = property8;
        Property9 = property9;
        Property10 = property10;
        Property11 = property11;
        Property12 = property12;
        Property13 = property13;
        Property14 = property14;
        Property15 = property15;
        Property16 = property16;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task OneFromConstructorOneFromInitializer_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith1: 1, ForMapWith2: 1);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = 1;
}

public class Destination
{
    public int Property1 { get; }
    public int Property2 { get; set; }

    public int ForMapWith1 { get; }
    public int ForMapWith2 { get; set; }

    public Destination(int property1, int forMapWith1)
    {
        Property1 = property1;
        ForMapWith1 = forMapWith1;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task OneFromConstructorOneReadonly_ShouldMapFirst()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = -1;
}

public class Destination
{
    public int Property1 { get; }
    public int Property2 { get; }
    public int ForMapWith { get; set; }

    public Destination(int property1)
    {
        Property1 = property1;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnmapableConstructor_ShouldUseDefaultConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
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
    public int ForMapWith { get; set; }

    public Destination(int property1, int notMatched)
    {
        Property1 = property1;
    }

    public Destination()
    {
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnmapableConstructor_ShouldUseSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = 1;
}

public class Destination
{
    public int Property1 { get; } = -1;
    public int Property2 { get; set; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int property1, int notMatched)
    {
        Property1 = property1;
    }

    public Destination(int property1)
    {
        Property1 = property1;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ParametersOrderNotCorrspondPropertiesOrder_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 5);
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = 2;
    public int Property3 { get; set; } = 3;
    public int Property4 { get; set; } = 4;
}

public class Destination
{
    public int Property1 { get; }
    public int Property2 { get; }
    public int Property3 { get; }
    public int Property4 { get; }
    public int ForMapWith { get; set; }

    public Destination(int property4, int property2, int forMapWith, int property3, int property1)
    {
        Property1 = property1;
        Property2 = property2;
        Property3 = property3;
        Property4 = property4;
        ForMapWith = forMapWith;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task SinglePrivateConstructor_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    private Destination(int property)
    {
        Property = property;
    }
}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    [TestMethod]
    public Task ProtectedConstructor_ShouldNotMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    protected Destination(int parameter)
    {
        Property = parameter;
    }
}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    [TestMethod]
    public Task InternalConstructor_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    internal Destination(int property)
    {
        Property = property;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ProtectedInternalConstructor_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    protected internal Destination(int property)
    {
        Property = property;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PrivateProtectedConstructor_ShouldNotMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    private protected Destination(int parameter)
    {
        Property = parameter;
    }
}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    [TestMethod]
    public Task ExpressionBodiedConstructor_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; set; }

    public Destination(int property) => Property = property;
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnusableWithoutUserProvided_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; }

    public Destination(int property, int forMapWith)
    {
        Property = property;
        ForMapWith = forMapWith;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesAndParameterNamesNotEqual_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int Property { get; set; } = 1;
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; }

    public Destination(int uniqueParameterName, int forMapWith)
    {
        Property = uniqueParameterName;
        ForMapWith = forMapWith;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesNamesNotEqual_ShouldNotMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = -1;
}

public class Destination
{
    public int PropertyB { get; }
    public int ForMapWith { get; }

    public Destination(int PropertyA, int forMapWith)
    {
        PropertyB = PropertyA;
        ForMapWith = forMapWith;
    }
}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    [TestMethod]
    public Task ParameterNotInitializeProperty_ShouldNotMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int Property { get; set; }
}

public class Destination
{
    public int Property { get; }
    public int ForMapWith { get; }

    public Destination(int Property)
    {
        //nothing
    }
}
";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

    //[TestMethod]
//    public Task ParameterInitializeMultipleProperty_ShouldMapFirst()
//    {
//        var source =
//@"using NextGenMapper;

//namespace Test;

//public class Program
//{
//    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
//}

//public class Source
//{
//    public int PropertyA { get; set; } = 1;
//    public int PropertyB { get; set; } = -1;
//}

//public class Destination
//{
//    public int PropertyA { get; }
//    public int PropertyB { get; }
//    public int ForMapWith { get; }

//    public Destination(int parameter, int forMapWith)
//    {
//        PropertyA = parameter;
//        PropertyB = parameter;
//        ForMapWith = forMapWith;
//    }
//}
//";

//        return VerifyAndRun(source);
//    }

    [TestMethod]
    public Task DifferentWaysInitializeProperty_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(forMapWith: 1);
}

public class Source
{
    public int PropertyA { get; } = 1;
    public int? PropertyB { get; } = 1;
    public int PropertyC { get; } = 1;
    public int? PropertyD { get; } = 1;
}

public class Destination
{
    public int PropertyA { get; }
    public int PropertyB { get; }
    public int PropertyC { get; }
    public int PropertyD { get; }
    public int ForMapWith { get; }

    public Destination(int parameterA, int? parameterB, int PropertyC, int? PropertyD, int forMapWith)
    {
        PropertyA = parameterA;
        PropertyB = parameterB ?? throw new ArgumentNullException(nameof(parameterB));
        this.PropertyC = PropertyC;
        this.PropertyD = PropertyD ?? throw new ArgumentNullException(nameof(PropertyD));

        ForMapWith = forMapWith;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ThisConstructor_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
}

public class Destination
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterA)
    {
        PropertyA = parameterA;
    }

    public Destination(int parameterA, int parameterB) : this(parameterA)
    {
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ThisConstructorDifferentNames_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
}

public class Destination
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int thisParameterA)
    {
        PropertyA = thisParameterA;
    }

    public Destination(int parameterA, int parameterB) : this(parameterA)
    {
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ThisConstructorOptionalArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
    public int PropertyC { get; set; } = 1;
}

public class Destination
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int PropertyC { get; } = -1;
    public int PropertyD { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterC, int parameterD = 1)
    {
        PropertyC = parameterC;
        PropertyD = parameterD;
    }

    public Destination(int parameterA, int parameterB, int parameterC) : this(parameterC)
    {
        PropertyA = parameterA;
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ThisConstructorOptionalMappedArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
    public int PropertyC { get; set; } = 1;
    public int PropertyD { get; set; } = 1;
}

public class Destination
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int PropertyC { get; } = -1;
    public int PropertyD { get; }
    public int ForMapWith { get; set; }

    public Destination(int parameterC, int parameterD = -1)
    {
        PropertyC = parameterC;
        PropertyD = parameterD;
    }

    public Destination(int parameterA, int parameterB, int parameterC, int parameterD) : this(parameterC, parameterD)
    {
        PropertyA = parameterA;
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ThisConstructorNamedArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().MapWith<Destination>(ForMapWith: 1);
}

public class Source
{
    public int PropertyA { get; set; } = 1;
    public int PropertyB { get; set; } = 1;
    public int PropertyC { get; set; } = 1;
    public int PropertyD { get; set; } = 1;
}

public class Destination
{
    public int PropertyA { get; }
    public int PropertyB { get; }
    public int PropertyC { get; }
    public int PropertyD { get; }
    public int ForMapWith { get; set; }

    public Destination(int PropertyB, int PropertyC = 0, int PropertyD = 0)
    {
        this.PropertyB = PropertyB;
        this.PropertyC = PropertyC;
        this.PropertyD = PropertyD;
    }

    public Destination(int parameterA, int parameterB, int parameterC, int parameterD) 
        : this(parameterB, PropertyD:  parameterD, PropertyC: parameterC)
    {
        PropertyA = parameterA;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task Record_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1).MapWith<Destination>(ForMapWith: 1);
}

public record Source(int Property);

public record Destination(int Property, int ForMapWith);
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task RecordThisConstructor_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1, 1).MapWith<Destination>(ForMapWith: 1);
}

public record Source(int PropertyA, int PropertyB);

public record Destination(int PropertyA)
{
    public int PropertyB { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterA, int parameterB) : this(parameterA)
    {
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task RecordThisConstructorOptionalArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1, 1, 1).MapWith<Destination>(ForMapWith: 1);
}

public record Source(int PropertyA, int PropertyB, int PropertyC);

public record Destination(int PropertyC, int PropertyD = 1)
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterA, int parameterB, int parameterC) : this(parameterC)
    {
        PropertyA = parameterA;
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task RecordThisConstructorOptionalMappedArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1, 1, 1, 1).MapWith<Destination>(ForMapWith: 1);
}

public record Source(int PropertyA, int PropertyB, int PropertyC, int PropertyD);

public record Destination(int PropertyC, int PropertyD = -1)
{
    public int PropertyA { get; } = -1;
    public int PropertyB { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterA, int parameterB, int parameterC, int parameterD) : this(parameterC, parameterD)
    {
        PropertyA = parameterA;
        PropertyB = parameterB;
    }
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task RecordThisConstructorNamedArgument_ShouldMapUsingSecondConstructor()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source(1, 1, 1, 1).MapWith<Destination>(ForMapWith: 1);
}

public record Source(int PropertyA, int PropertyB, int PropertyC, int PropertyD);

public record Destination(int PropertyB, int PropertyC = 0, int PropertyD = 0)
{
    public int PropertyA { get; } = -1;
    public int ForMapWith { get; set; }

    public Destination(int parameterA, int parameterB, int parameterC, int parameterD) 
        : this(parameterB, PropertyD:  parameterD, PropertyC: parameterC)
    {
        PropertyA = parameterA;
    }
}
";

        return VerifyAndRun(source);
    }
}