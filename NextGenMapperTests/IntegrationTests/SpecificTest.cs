using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class SpecificTest
    {
        [TestMethod]
        public void CircularReferenses()
        {
            var classes = @"
public class SourceA
{
    public string Id { get; set; }
    public SourceB Ref { get; set; }
}
public class SourceB
{
    public string Id { get; set; }
    public SourceC Ref { get; set; }
}
public class SourceC
{
    public string Id { get; set; }
    public SourceA Ref { get; set; }
}

public class DestinationA
{
    public string Id { get; set; }
    public DestinationB Ref { get; set; }
}
public class DestinationB
{
    public string Id { get; set; }
    public DestinationC Ref { get; set; }
}
public class DestinationC
{
    public string Id { get; set; }
    public DestinationA Ref { get; set; }
}";

            var validateFunction = @"
var sourceC = new SourceC { Id = ""C"" };
var sourceB = new SourceB { Id = ""B"", Ref = sourceC };
var sourceA = new SourceA { Id = ""A"", Ref = sourceB };
sourceC.Ref = sourceA;

var destinationA = sourceA.Map<DestinationA>();

var destinationB = destinationA.Ref;
var destinationC = destinationB.Ref;

var isValid = sourceA.Id == destinationA.Id && sourceB.Id == destinationB.Id && sourceC.Id == destinationC.Id;

if (!isValid) throw new MapFailedException(sourceA, destinationA);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            _ = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM001");
        }

        [TestMethod]
        public void MultipleRunsOneGenerator()
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
            var generator = new MapperGenerator();
            //first run
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            //second run
            userSourceCompilation = userSource.RunGenerators(out generatorDiagnostics, generators: generator);
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));

            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }
    }
}
