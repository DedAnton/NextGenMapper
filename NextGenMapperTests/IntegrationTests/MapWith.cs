using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using System;
using System.Linq;

namespace NextGenMapperTests.IntegrationTests;

[TestClass]
public class MapWith
{
    [TestMethod]
    public void MapWithMappingInitializer_FirstParameter()
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

var destination = source.MapWith<Destination>(""good"");

var isValid = ""good"" == destination.Name && source.Birthday == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
        //TODO: add more tests for mapwith
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

        var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
        Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
    }

    [TestMethod]
    public void MapWithMappingInitializer_SecondParameter()
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

var destination = source.MapWith<Destination>(birthday: new DateTime(2022, 05, 20));

var isValid = source.Name == destination.Name && new DateTime(2022, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

        var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
        Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
    }

    [TestMethod]
    public void MapWithMappingInitializer_ManyParameters()
    {
        var classes = @"
public class Source
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int E { get; set; }
    public int F { get; set; }
}

public class Destination
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int E { get; set; }
    public int F { get; set; }
}";

        var validateFunction = @"
var source = new Source { A = 1, B = 2, C = 3, D = 4, E = 5, F = 6 };

var destination = source.MapWith<Destination>(10, 20, d: 40, f: 60);

var isValid = destination.A == 10 && destination.B == 20 
    && destination.C == source.C && destination.D == 40 
    && destination.E == source.E && destination.F == 60;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

        var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
        Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
    }

    [TestMethod]
    public void MapWithMapping_WithoutArguments_MustCreateDiagnostic()
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

var destination = source.MapWith<Destination>();

var isValid = true;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM009");
    }

    [TestMethod]
    public void MapWithMapping_AllArguments_MustCreateDiagnostic()
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

var destination = source.MapWith<Destination>(""TEST"", DateTime.MinValue);

var isValid = true;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM010");
    }

}
