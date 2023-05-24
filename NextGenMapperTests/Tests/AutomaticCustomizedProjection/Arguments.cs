namespace NextGenMapperTests.Tests.AutomaticCustomizedProjection;

[TestClass]
public class Arguments : SourceGeneratorVerifier
{
    public override string TestGroup => "ConfiguredProjection";

    [TestMethod]
    public Task UserProvideNoOneArguments_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{    
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>();
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

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task UserProvideHalfArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{    
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(ForMapWith1: 1, ForMapWith2: 1);
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
    public int ForMapWith1 { get; set; }
    public int ForMapWith2 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UserProvideAllArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{    
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(
        Property1: 1,
        Property2: 1,
        Property3: 1,
        Property4: 1);
}

public class Source
{

}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }
    public int Property4 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task ArgumentsOrderDoesNotCorrespondProperties_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{    
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(        
        Property4: 4,
        Property1: 1,
        Property3: 3,
        Property2: 2);
}

public class Source
{

}

public class Destination
{
    public int Property1 { get; set; }
    public int Property2 { get; set; }
    public int Property3 { get; set; }
    public int Property4 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnnamedArgument_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{    
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(1);
}

public class Source
{

}

public class Destination
{
    public int Property { get; set; }
}";

        return VerifyOnly(source, ignoreSourceErrors: true);
    }

//    [TestMethod]
//    public Task ArgumentForNonExistenParameter_Diagnostic()
//    {
//        var source =
//@"using NextGenMapper;

//namespace Test;

//public class Program
//{
//    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(NonExistenParameter: 123);
//}

//public class Source
//{
//    public int Property { get; set; }
//}

//public class Destination
//{
//    public int Property { get; set; }
//}";

//        return VerifyOnly(source, ignoreSourceErrors: true);
//    }
}
