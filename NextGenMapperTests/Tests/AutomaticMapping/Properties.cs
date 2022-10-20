namespace NextGenMapperTests.Tests.AutomaticMapping;

[TestClass]
public class Properties : SourceGeneratorVerifier
{
    [TestMethod]
    public Task NamesShouldBeEqual_OtherwiseIgnored()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int SamePropertyName { get; set; } = 1;
    public int DifferentPropertyNameA { get; set; } = -1;
    public int differentPropertyNameC { get; set; } = -1;
    public int differentpropertynamed { get; set; } = -1;
}

public class Destination
{
    public int SamePropertyName { get; set; } = -1;
    public int DifferentPropertyNameB { get; set; } = 0;
    public int DifferentPropertyNameC { get; set; } = 0;
    public int DifferentPropertyNameD { get; set; } = 0;
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task DifferentAccessModifiers_ShuldMapPublicAndInternal()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>().ToString();
}

public class Source
{
    public int Property1 { get; set; } = 1;
    public int Property2 { get; set; } = -1;
    public int Property3 { get; set; } = -1;
    public int Property4 { get; set; } = 1;
    public int Property5 { get; set; } = 1;
    public int Property6 { get; set; } = -1;

    public int Property7 { get; set; } = 1;
    private int Property8 { get; set; } = -1;
    protected int Property9 { get; set; } = -1;
    internal int Property10 { get; set; } = 1;
    protected internal int Property11 { get; set; } = 1;
    private protected int Property12 { get; set; } = -1;
}

public class Destination
{
    public int Property1 { get; set; }
    private int Property2 { get; set; }
    protected int Property3 { get; set; }
    internal int Property4 { get; set; }
    protected internal int Property5 { get; set; }
    private protected int Property6 { get; set; }

    public int Property7 { get; set; }
    public int Property8 { get; set; }
    public int Property9 { get; set; }
    public int Property10 { get; set; }
    public int Property11 { get; set; }
    public int Property12 { get; set; }

    public override string ToString() => $""{Property1}, {Property2}, {Property3}, {Property4}, {Property5}, {Property6}, {Property7}, {Property8}, {Property9}, {Property10}, {Property11}, {Property12}"";
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertyFromBaseClass_ShouldBeIgnored()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class SourceBase
{
    public int DerrivedProperty { get; set; } = -1;
}

public class Source : SourceBase
{
    public int PropertyName { get; set; } = 1;
}

public class Destination
{
    public int PropertyName { get; set; }
    public int DerrivedProperty { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task DifferentPropertiesAccessors()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int Property1 { get; set; } = 1;
    private int _property2 = -1;
    public int Property2
    {
        set => _property2 = value;
    }

    public int Property3 { get; set; } = -1;
    public int Property4 { get; set; } = 1;
    public int Property5 { get; set; } = 1;
}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
        
    public int Property3 { get; }
    public int Property4 { get; set; }
    public int Property5 { get; init; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task DifferentPropertiesAccessorsAccessModifiers_ShouldMapPublicAndInternal()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int Property1 { get; } = 1;
    public int Property2 { get; } = -1;
    public int Property3 { get; } = -1;
    public int Property4 { get; } = 1;
    public int Property5 { get; } = 1;
    public int Property6 { get; } = -1;

    public int Property7 { get; set; } = 1;
    public int Property8 { private get; set; } = -1;
    public int Property9 { protected get; set; } = -1;
    public int Property10 { internal get; set; } = 1;
    public int Property11 { protected internal get; set; } = 1;
    public int Property12 { private protected get; set; } = -1;
}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; private set; }
    public int Property3 { get; protected set; }
    public int Property4 { get; internal set; }
    public int Property5 { get; protected internal set; }
    public int Property6 { get; private protected set; }

    public int Property7 { get; set; }
    public int Property8 { get; set; }
    public int Property9 { get; set; }
    public int Property10 { get; set; }
    public int Property11 { get; set; }
    public int Property12 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task MapFromClassWithoutProperties_ShouldDiagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
}

public class Destination
{
    public int PropertyName { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MapToClassWithoutProperties_ShouldDiagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int PropertyName { get; set; }
}

public class Destination
{
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MapFromClassWithOnlyUnsuitableProperties_ShouldDiagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    private int Property1 { get; set; } = -1;
    protected int Property2 { get; set; } = -1;
    private protected int Property3 { get; set; } = -1;

    public int Property4 { private get; set; } = -1;
    public int Property5 { protected get; set; } = -1;
    public int Property6 { private protected get; set; } = -1;
}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }

    public int Property4 { get; set; }
    public int Property5 { get; set; }
    public int Property6 { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task MapToClassWithOnlyUnsuitableProperties_ShouldDiagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }

    public int Property4 { get; set; }
    public int Property5 { get; set; }
    public int Property6 { get; set; }
}

public class Destination
{
    private int Property1 { get; set; } = -1;
    protected int Property2 { get; set; } = -1;
    private protected int Property3 { get; set; } = -1;

    public int Property4 { get; private set; } = -1;
    public int Property5 { get; protected set; } = -1;
    public int Property6 { get; private protected set; } = -1;
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task ClassesWithNoMatchedProperties_ShouldDiagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int PropertyA { get; set; }
    public int PropertyB { get; set; }
}

public class Destination
{
    public int PropertyC { get; set; }
    public int PropertyD { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task PropertiesWithBackingFields_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    private int _propertyA = 1;
    public int PropertyA 
    {
        get => _propertyA;
        set => _propertyA = value;
    }

    private int _propertyB = 1;
    public int PropertyB
    {
        get { return _propertyB; } 
        set { _propertyB = value; } 
    }
}

public class Destination
{
    private int _propertyA = 1;
    public int PropertyA 
    {
        get => _propertyA;
        set => _propertyA = value;
    }

    private int _propertyB = 1;
    public int PropertyB
    {
        get => _propertyB;
        set => _propertyB = value;
    }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task MapFromExpressionBodiedReadonlyProperty_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int PropertyA => 1;
}

public class Destination
{
    public int PropertyA { get; set; }
}";

        return VerifyAndRun(source);
    }
}
