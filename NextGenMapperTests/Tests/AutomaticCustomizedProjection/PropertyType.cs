﻿namespace NextGenMapperTests.Tests.AutomaticCustomizedProjection;

[TestClass]
public class PropertyType : SourceGeneratorVerifier
{
    public override string TestGroup => "ConfiguredProjection";

    [TestMethod]
    public Task CommonUsedTypes_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(ForMapWith: 1).First();
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

    public int ForMapWith { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesEquals_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>(ForMapWith: 1).First();
}

public class Source
{
    public SameType SameProperty { get; set; } = new SameType();
}

public class Destination
{
    public SameType SameProperty { get; set; }
    
    public int ForMapWith { get; set; }
}

public class SameType
{
    public int Property { get; set; } = 1;
}
";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentClasses_Diagnostic()
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
    public ClassA SameProperty { get; set; } = new ClassA();
}

public class Destination
{
    public ClassB SameProperty { get; set; } = new ClassB();

    public int ForMapWith { get; set; }
}

public class ClassA
{
    public int Property { get; set; } = -1;
}

public class ClassB
{
    public int Property { get; set; } = 1;
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentEnums_Diagnostic()
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
    public EnumA SameProperty { get; set; } = EnumA.Bad;
}

public class Destination
{
    public EnumB SameProperty { get; set; } = EnumB.Good;

    public int ForMapWith { get; set; }
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

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task PropertiesTypesIsDifferentCollections_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>();
}

public class Source
{
    public int[] SameProperty { get; set; } = new int[] { -1 };
}

public class Destination
{
    public List<int> SameProperty { get; set; } = new List<int> { 1 };

    public int ForMapWith { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task TryMapDifferentPropertyTypeKind_Diagnostic()
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
    public ClassA Property1 { get; set; } 
    public ClassA Property2 { get; set; }
    public ClassA Property3 { get; set; }
}

public class Destination
{
    public EnumA Property1 { get; set; } 
    public StructA Property2 { get; set; }
    public int[] Property3 { get; set; }

    public int ForMapWith { get; set; }
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
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new SourceA() }.AsQueryable().ProjectWith<DestinationA>();
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

    public int ForMapWith { get; set; }
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
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source() }.AsQueryable().ProjectWith<Destination>();
}

public class Source
{
    public int SamePropertyName { get; set; } = 1;
}

public class Destination
{
    public string SamePropertyName { get; set; }

    public int ForMapWith { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task GenericTypesWithEqualsTypesArguments_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source<int>() }.AsQueryable().ProjectWith<Destination<int>>(ForMapWith: 1).First();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }

    public int ForMapWith { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task GenericTypesWithNotEqualsTypesArguments_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source<ClassA>{ Property = new ClassA() } }.AsQueryable().ProjectWith<Destination<ClassB>>();
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

public class ClassA
{
    public int Property { get; set; }
}

public class ClassB
{
    public int Property { get; set; }
}
";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task GenericTypesWithNotEqualsTypesArgumentsThatCanNotBeMapped_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Linq;

namespace Test;

public class Program
{
    public object RunTest() => new[] { new Source<int>{ Property = 1 } }.AsQueryable().ProjectWith<Destination<string>>();
}

public class Source<T>
{
    public T Property { get; set; }
}

public class Destination<T>
{
    public T Property { get; set; }

    public int ForMapWith { get; set; }
}";

        return VerifyOnly(source);
    }
}