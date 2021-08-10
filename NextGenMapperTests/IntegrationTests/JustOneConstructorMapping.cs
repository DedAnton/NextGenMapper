using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class JustOneConstructorMapping
    {
        [TestMethod]
        public void JustOneConstructorMappingCommon()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; }
    public int Height { get; }
    public string City { get; }
    public string Birthday { get; }

    public Destination() { }

    public Destination(string name, int height, string city, string birthday)
    {
        Name = name;
        Height = height;
        City = city;
        Birthday = birthday;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo"", Birthday = new DateTime(1997, 05, 20)};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && source.City == destination.City && source.Birthday.ToShortDateString() == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"", default, default, source.Birthday.ToShortDateString());
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsEmpty, generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void JustOneConstructorMappingWithInitializer()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; }
    public int Height { get; }
    public string City { get; set; }
    public DateTime Birthday { get; set; }

    public Destination() { }

    public Destination(string name, int height)
    {
        Name = name;
        Height = height;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo"", Birthday = new DateTime(1997, 05, 20)};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && source.City == destination.City && source.Birthday == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"", default) { };
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsEmpty, generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void PartialJustOneConstructorMappingBadParametersOrder()
        {
            var classes = @"
public class Source
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public Destination(DateTime birthday, string name, string id)
    {
        Name = name;
        Birthday = birthday;
        Id = id;
    }

    public string Name { get; }
    public DateTime Birthday { get; }
    public string Id { get; }
}";

            var validateFunction = @"
var source = new Source { Id = 123, Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && source.Id.ToString() == destination.Id;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination(default, default, source.Id.ToString());
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }
    }
}
