using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;


namespace NextGenMapperTests
{
    [TestClass]
    public class RecordMapping
    {
        //test cases are limited because records are classes

        [TestMethod]
        public void MappingRecord()
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
        public void MappingRecordsWithNotMappedInitProperty()
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
        public void MappingRecordsWithMappedInitProperty()
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

        [TestMethod]
        public void MappingRecordsWithTwoConstructors()
        {
            var classes = @"
public record Source(string Name, DateTime Birthday);
public record Destination(string Name, DateTime Birthday, string Property)
{
    public Destination(string name, DateTime birthday)
        : this (name, birthday, ""default"")
    { }
}
";

            var validateFunction = @"
var source = new Source(""Anton"", new DateTime(1997, 05, 20));

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && destination.Property == ""default"";

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
        public void PartialRecordMapping()
        {
            var classes = @"
public record Source (string FirstName, string SecondName, DateTime Birthday, int Height);
public record Destination(string Name, string Birthday, int Height)
{
    public Destination(string name, string birthday)
        :this(name, birthday, -1)
    { }
}
";

            var validateFunction = @"
var source = new Source(""Anton"", ""Ryabchikov"", new DateTime(1997, 05, 20), 190);

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString() && source.Height == destination.Height;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"", source.Birthday.ToShortDateString());
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
        public void JustOneConstructorRecordMapping()
        {
            var classes = @"
public record Source(string FirstName, string SecondName, int Height, string City, DateTime Birthday);
public record Destination(string Name, int Height, string City, string Birthday);
";

            var validateFunction = @"
var source = new Source(""Anton"", ""Ryabchikov"", 190, ""Dinamo"", new DateTime(1997, 05, 20));

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
    }
}
