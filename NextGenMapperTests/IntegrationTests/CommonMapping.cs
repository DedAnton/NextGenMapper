using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using System.Linq;
using System.Reflection.Emit;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class CommonMapping
    {
        [TestMethod]
        public void MappingInitializer()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingConstructor()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public Destination(string name, DateTime birthday)
    {
        Name = name;
        Birthday = birthday;
    }

    public string Name { get; }
    public DateTime Birthday { get; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingConstructorAndInitializer()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string ToInitializer { get; set; }
}

public class Destination
{
    public Destination(string name, DateTime birthday)
    {
        Name = name;
        Birthday = birthday;
    }

    public string Name { get; }
    public DateTime Birthday { get; }
    public string ToInitializer { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20), ToInitializer = ""Hello"" };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && source.ToInitializer == destination.ToInitializer;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingMultipleConstructors()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public int Age { get; set; }
}

public class Destination
{
    public Destination(string name)
    {
        Name = name;
    }

    public Destination(string name, DateTime birthday)
    {
        Name = name;
        Birthday = birthday;
    }

    public Destination(string name, DateTime birthday, int age)
    {
        Name = name;
        Birthday = birthday;
        Age = age;
    }

    public string Name { get; }
    public DateTime Birthday { get; }
    public int Age { get; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20), Age = 10 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && source.Age == destination.Age;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingWithNotSettableProperty()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public int Age { get; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Age = 24 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && destination.Age == 0;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingWithNotMappedProperty()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public int Height { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Age = 24 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && destination.Height == 0;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingBadConstuctor()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Destination
{
    public Destination(string name)
    {
        Name = name;
    }

    public Destination(string name, string birthday, int age)
    {
        Name = name;
        Birthday = birthday;
        Age = age;
    }

    public string Name { get; }
    public string Birthday { get; }
    public int Age { get; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Age = 24 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && destination.Age == 0 && destination.Birthday == null;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingInitializerWithInclude()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public SourceInclude Include { get; set; }
}
public class SourceInclude
{
    public int Property { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public DestinationInclude Include { get; set; }
}
public class DestinationInclude
{
    public int Property { get; set; }
}
";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Include = new SourceInclude { Property = 10 } };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Include.Property == destination.Include.Property;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingConstructorWithInclude()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public SourceInclude Include { get; set; }
}
public class SourceInclude
{
    public int Property { get; set; }
}

public class Destination
{
    public string Name { get; }
    public DestinationInclude Include { get; }

    public Destination(string name, DestinationInclude include)
    {
        Name = name;
        Include = include;
    }
}
public class DestinationInclude
{
    public int Property { get; }
    
    public DestinationInclude(int property) => Property = property;
}
";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Include = new SourceInclude { Property = 10 } };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Include.Property == destination.Include.Property;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void TryMappingPrimitiveTypesImplicitMustMap()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public double Age { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Age = 24 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Age == destination.Age;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingConstructorBadParametersOrder()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public Destination(DateTime birthday, string name)
    {
        Name = name;
        Birthday = birthday;
    }

    public string Name { get; }
    public DateTime Birthday { get; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingConstructorButSettableProperties()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public string Birthday { get; set; }
}

public class Destination
{
    public Destination(string name, string birthday)
    {
        Name = ""good"";
        Birthday = ""good"";
    }

    public string Name { get; set; }
    public string Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = ""1997/05/20)"" };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""good"" && destination.Birthday == ""good"";

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingEnumInClass()
        {
            var classes = @"
public class Source
{
    public EnumFrom Property { get; set; }
}

public class Destination
{
    public EnumTo Property { get; set; }
}

public enum EnumFrom   
{   ValueA,
    ValueB,
    ValueC
}

public enum EnumTo
{
    valueA = 10,
    valueB = 20,
    valueC = 30
}";

            var validateFunction = @"
var source = new Source { Property = EnumFrom.ValueA };

var destination = source.Map<Destination>();

var isValid = destination.Property == EnumTo.valueA;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingCollectionInClass()
        {
            var classes = @"
public class Source
{
    public List<int> Collection { get; set; }
}

public class Destination
{
    public List<int> Collection { get; set; }
}";

            var validateFunction = @"
var source = new Source { Collection = new List<int> { 1, 2, 3, 4 } };

var destination = source.Map<Destination>();

var isValid = source.Collection[0] == destination.Collection[0] 
    && source.Collection[1] == destination.Collection[1]
    && source.Collection[2] == destination.Collection[2] 
    && source.Collection[3] == destination.Collection[3];

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        //[TestMethod]
        public void MappingInheritanceProperty()
        {
            var classes = @"
public class SourceBase
{
    public int InheritedProperty { get;set; }
}
public class Source : SourceBase
{
    public string Name { get; set; }
}

public class DestinationBase
{
    public int InheritedProperty { get;set; }
}
public class Destination : DestinationBase
{
    public string Name { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", InheritedProperty = 10 };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.InheritedProperty == destination.InheritedProperty;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapOnMethod()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }

    public static Source Create(string name, DateTime birthdate) => new Source { Name = name, Birthday = birthdate };
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}";

            var validateFunction = @"
var destination = Source.Create(""Anton"", new DateTime(1997, 05, 20)).Map<Destination>();

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(null, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapOnConstructorMethod()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }

    public Source(string name, DateTime birthday)
    {
        Name = name;
        Birthday = birthday;
    }
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}";

            var validateFunction = @"
var destination = new Source(""Anton"", new DateTime(1997, 05, 20)).Map<Destination>();

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(null, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapOnConstructorMethodWithInitializer()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}";

            var validateFunction = @"
var destination = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20)}.Map<Destination>();

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(null, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapInLambda()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20)};
var sourceArray = new Source[] { source };

var destinationArray = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(sourceArray, x => x.Map<Destination>()));
var destination = destinationArray[0];

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(null, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapOnProperty()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }

    public static Source SourceProperty => new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}
";

            var validateFunction = @"
var destination = Source.SourceProperty.Map<Destination>();

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(Source.SourceProperty, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CallMapOnField()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }

    public static Source sourceProperty = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
}

public class Destination
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}
";

            var validateFunction = @"
var destination = Source.sourceProperty.Map<Destination>();

var isValid = ""Anton"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(Source.sourceProperty, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }
    }
}
