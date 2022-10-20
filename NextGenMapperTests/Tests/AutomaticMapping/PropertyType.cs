using System.Security.Cryptography.X509Certificates;

namespace NextGenMapperTests.Tests.AutomaticMapping;

[TestClass]
public class PropertyType : SourceGeneratorVerifier
{
//    [TestMethod]
//    public Task MapNullable_ShouldMap()
//    {
//        var source =
//@"
//using NextGenMapper;

//namespace Test;

//public class Program
//{
//    public object RunTest() => new Source().Map<Destination>();
//}

//#nullable enable
//public class Source
//{
//    public string? PropertyA { get; set; } = ""good"";
//    public int? PropertyB { get; set; } = 1;

//    public string PropertyC { get; set; } = ""good"";
//    public int PropertyD { get; set; } = 1;

//    public string? PropertyE { get; set; } = ""good"";
//    public int? PropertyF { get; set; } = 1;
//}

//public class Destination
//{
//    public string PropertyA { get; set; } = ""bad"";
//    public int PropertyB { get; set; } = -1;

//    public string? PropertyC { get; set; } = ""bad"";
//    public int? PropertyD { get; set; } = -1;

//    public string? PropertyE { get; set; } = ""bad"";
//    public int? PropertyF { get; set; } = -1;
//}
//#nullable disable";

//        return VerifyAndRun(source);
//    }

    [TestMethod]
    public Task CommonUsedTypes_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public byte Property1 { get; set; } = 1;
    public sbyte Property2 { get; set; } = 1;
    public short Property3 { get; set; } = 1;
    public ushort Property4 { get; set; } = 1;
    public int Property5 { get; set; } = 1;
    public uint Property6 { get; set; } = 1;
    public long Property7 { get; set; } = 1;
    public ulong Property8 { get; set; } = 1;
    public float Property9 { get; set; } = 1.0f;
    public double Property10 { get; set; } = 1.0;
    public decimal Property11 { get; set; } = 1.0M;
    public bool Property12 { get; set; } = true;
    public char Property13 { get; set; } = 'A';
    public string Property14 { get; set; } = ""good"";
    public object Property15 { get; set; } = ""good"";
    public DateTime Property16 { get; set; } = new DateTime(638011579930000000);
    public DateTimeOffset Property17 { get; set; } = new DateTimeOffset(638011579930000000, TimeSpan.FromHours(3));
    public DateOnly Property18 { get; set; } = new DateOnly(2022, 10, 12);
    public TimeOnly Property19 { get; set; } = new TimeOnly(7, 57, 12, 248);
    public TimeSpan Property20 { get; set; } = new TimeSpan(638011579930000000);
}

public class Destination
{
    public byte Property1 { get; set; }
    public sbyte Property2 { get; set; }
    public short Property3 { get; set; }
    public ushort Property4 { get; set; }
    public int Property5 { get; set; }
    public uint Property6 { get; set; }
    public long Property7 { get; set; }
    public ulong Property8 { get; set; }
    public float Property9 { get; set; }
    public double Property10 { get; set; }
    public decimal Property11 { get; set; }
    public bool Property12 { get; set; }
    public char Property13 { get; set; } = 'A';
    public string Property14 { get; set; }
    public object Property15 { get; set; }
    public DateTime Property16 { get; set; }
    public DateTimeOffset Property17 { get; set; }
    public DateOnly Property18 { get; set; }
    public TimeOnly Property19 { get; set; }
    public TimeSpan Property20 { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesEquals_ShouldMap()
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
    public SameType SameProperty { get; set; } = new SameType();
}

public class Destination
{
    public SameType SameProperty { get; set; }
}

public class SameType
{
    public int Property { get; set; } = 1;
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentClasses_ShouldMap()
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
    public ClassA SameProperty { get; set; } = new ClassA();
}

public class Destination
{
    public ClassB SameProperty { get; set; } = new ClassB();
}

public class ClassA
{
    public int Property { get; set; } = 1;
}

public class ClassB
{
    public int Property { get; set; } = -1;
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentEnums_ShouldMap()
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
    public EnumA SameProperty { get; set; } = EnumA.Good;
}

public class Destination
{
    public EnumB SameProperty { get; set; } = EnumB.Bad;
}

public enum EnumA
{
    Good,
    Bad
}

public enum EnumB
{
    Good,
    Bad
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentCollections_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest() => new Source().Map<Destination>();
}

public class Source
{
    public int[] SameProperty { get; set; } = new int[] { 1 };
}

public class Destination
{
    public List<int> SameProperty { get; set; } = new List<int> { -1 };
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task TryMapDifferentPropertyTypeKind_Diagnostic()
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
    public ClassA Property1 { get; set; } 
    public ClassA Property2 { get; set; }
    public ClassA Property3 { get; set; }
}

public class Destination
{
    public EnumA Property1 { get; set; } 
    public StructA Property2 { get; set; }
    public int[] Property3 { get; set; }
}

public class ClassA
{
    public int Property { get; set; }
}

public enum EnumA
{
    A,
    B,
    C
}

public struct StructA
{
    public int Property { get; set; }
}
";
        return VerifyOnly(source);
    }

    [TestMethod]
    public Task CirularReferences_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new SourceA().Map<DestinationA>();
}

public class SourceA
{
    public SourceB Reference { get; set; }
}
public class SourceB
{
    public SourceC Reference { get; set; }
}
public class SourceC
{
    public SourceA Reference { get; set; }
}

public class DestinationA
{
    public DestinationB Reference { get; set; }
}
public class DestinationB
{
    public DestinationC Reference { get; set; }
}
public class DestinationC
{
    public DestinationA Reference { get; set; }
}
";
        return VerifyOnly(source);
    }

    [TestMethod]
    public Task TypesNotEqualsAndCanNotBeMapped_Diagnostic()
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
}

public class Destination
{
    public string SamePropertyName { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task GenericTypesWithEqualsTypesArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<int>{ Property = 1 }.Map<Destination<int>>();
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
    public Task GenericTypesWithNotEqualsTypesArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<ClassA>{ Property = new ClassA() }.Map<Destination<ClassB>>();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }
}

public class ClassA
{
    public int Property { get; set; } = 1;
}

public class ClassB
{
    public int Property { get; set; } = -1;
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task GenericTypesWithNotEqualsTypesArgumentsThatCanNotBeMapped_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source<int>{ Property = 1 }.Map<Destination<string>>();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }
}";

        return VerifyOnly(source);
    }
}
