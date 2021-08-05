using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests
{
    [TestClass]
    public class CommonRecordsMapping
    {

        [TestMethod]
        public void MappingShortRecords()
        {
            var classes = @"
public record Source(string Name, DateTime Birthday);
public record Destination(string Name, DateTime Birthday);
";

            var validateFunction = @"
var source = new Source(""Anton"", new DateTime(1997, 05, 20));

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
        public void MappingShortRecordsWithNotMappedInitProperty()
        {
            var classes = @"
public record Source(string Name, DateTime Birthday);
public record Destination(string Name, DateTime Birthday)
{
    public string NotMappedProperty { get; init; }
}
";

            var validateFunction = @"
var source = new Source(""Anton"", new DateTime(1997, 05, 20));

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && destination.NotMappedProperty == null;

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
        public void MappingShortRecordsWithMappedInitProperty()
        {
            var classes = @"
public record Source(string Name, DateTime Birthday, string MappedProperty);
public record Destination(string Name, DateTime Birthday)
{
    public string MappedProperty { get; init; }
}
";

            var validateFunction = @"
var source = new Source(""Anton"", new DateTime(1997, 05, 20), ""property"");

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && source.MappedProperty == destination.MappedProperty;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }


        #region Same test as for common class mapping

        [TestMethod]
        public void MappingInitializer()
        {
            var classes = @"
public record Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string ToInitializer { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public int Age { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public record Destination
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
public record Source
{
    public string Name { get; set; }
    public SourceInclude Include { get; set; }
}
public record SourceInclude
{
    public int Property { get; set; }
}

public record Destination
{
    public string Name { get; set; }
    public DestinationInclude Include { get; set; }
}
public record DestinationInclude
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
public record Source
{
    public string Name { get; set; }
    public SourceInclude Include { get; set; }
}
public record SourceInclude
{
    public int Property { get; set; }
}

public record Destination
{
    public string Name { get; }
    public DestinationInclude Include { get; }

    public Destination(string name, DestinationInclude include)
    {
        Name = name;
        Include = include;
    }
}
public record DestinationInclude
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

        #endregion

    }
}
