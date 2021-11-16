using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using System.Linq;

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
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.Any(x => x.Id == "NGM001"));
        }
    }
}
