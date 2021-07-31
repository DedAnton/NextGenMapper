using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests
{
    [TestClass]
    public class EnumMapping
    {
        [TestMethod]
        public void MappingByName()
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
var source = EnumFrom.ValueB;

var destination = source.Map<EnumTo>();

var isValid = destination == EnumTo.valueB;

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
        public void MappingByValue()
        {
            var classes = @"
public enum EnumFrom   
{   ValueA = 1,
    ValueB = 2,
    ValueC = 3
}

public enum EnumTo
{
    ValueD = 1,
    ValueE = 2,
    ValueF = 3
}";

            var validateFunction = @"
var source = EnumFrom.ValueB;

var destination = source.Map<EnumTo>();

var isValid = destination == EnumTo.ValueE;

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
        public void MappingByNameAndValue()
        {
            var classes = @"
public enum EnumFrom   
{   ValueA,
    ZXC = 5,
    ValueC
}

public enum EnumTo
{
    valueA,
    QWE = 5,
    valueC
}";

            var validateFunction = @"
var sourceN = EnumFrom.ValueA;
var sourceV = EnumFrom.ZXC;

var destinationN = sourceN.Map<EnumTo>();
var destinationV = sourceV.Map<EnumTo>();

var isValid = destinationN == EnumTo.valueA && destinationV == EnumTo.QWE;

if (!isValid) throw new MapFailedException(sourceN, destinationN);";

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
