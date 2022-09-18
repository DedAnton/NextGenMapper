using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class CollectionsMapping
    {
        [TestMethod]
        public void ListMapping()
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
var source1 = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
var source2 = new Source { Name = ""Roman"", Birthday = new DateTime(1996, 07, 08) };
var source = new List<Source> { source1, source2 };

var destination = source.Map<List<Destination>>();

var isValid = source[0].Name == destination[0].Name && source[0].Birthday == destination[0].Birthday
           && source[1].Name == destination[1].Name && source[1].Birthday == destination[1].Birthday;

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
        public void ArrayMapping()
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
var source1 = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
var source2 = new Source { Name = ""Roman"", Birthday = new DateTime(1996, 07, 08) };
var source = new Source[] { source1, source2 };

var destination = source.Map<Destination[]>();

var isValid = source[0].Name == destination[0].Name && source[0].Birthday == destination[0].Birthday
           && source[1].Name == destination[1].Name && source[1].Birthday == destination[1].Birthday;

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
        public void GenericInterfaceMapping()
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
var source1 = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
var source2 = new Source { Name = ""Roman"", Birthday = new DateTime(1996, 07, 08) };
var sourceList = new List<Source> { source1, source2 };
//var source = source1;
var source = sourceList as System.Collections.Generic.IEnumerable<Source>;
var destination = source.Map<ICollection<Destination>>();

var isValid = true;

if (!isValid) throw new MapFailedException(source, null);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingCollectionOfEnum()
        {
            var classes = @"
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
var source = new List<EnumFrom> { EnumFrom.ValueA, EnumFrom.ValueB, EnumFrom.ValueC };

var destination = source.Map<List<EnumTo>>();

var isValid = destination[0] == EnumTo.valueA && destination[1] == EnumTo.valueB && destination[2] == EnumTo.valueC;

if (!isValid) throw new MapFailedException(source, destination);";

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
