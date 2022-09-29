using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using NextGenMapper.PostInitialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenMapperTests.IntegrationTests;

[TestClass]
public class MapWith
{
    [TestMethod]
    public void MapWith_Initializer_FirstParameter()
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
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

        var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
        Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
    }

    [TestMethod]
    public void MapWith_Initializer_SecondParameter()
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
    public void MapWith_Initializer_ManyParameters()
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
    public void MapWith_WithoutArguments_MustCreateDiagnostic()
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
        userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM005");
    }

    [TestMethod]
    public void MapWith_AllArguments_MustCreateDiagnostic()
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
        userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM006", "Wrong diagnostinc");
    }

    [TestMethod]
    public async Task MapWithNonSettableProperty()
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
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public int Age { get; }
}";

        var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20), Age = 15 };

var destination = source.MapWith<Destination>(name: ""good"");
//var destination = source.MapWith<Destination>(age: 30);

var isValid = ""good"" == destination.Name && source.Birthday == destination.Birthday && 0 == destination.Age;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generated = "using NextGenMapper.Extensions;\r\n\r\nnamespace NextGenMapper\r\n{\r\n    internal static partial class Mapper\r\n    {\r\n        internal static Test.Destination MapWith<To>\r\n        (\r\n            this Test.Source source,\r\n            string name = default,\r\n            System.DateTime birthday = default\r\n        )\r\n        {\r\n            throw new System.NotImplementedException(\"This method is a stub and is not intended to be called\");\r\n        }\r\n\r\n        internal static Test.Destination MapWith<To>\r\n        (\r\n            this Test.Source source,\r\n            string name\r\n        )\r\n        => new Test.Destination\r\n        (\r\n        )\r\n        {\r\n            Name = name,\r\n            Birthday = source.Birthday\r\n        };\r\n\r\n    }\r\n}";
        await new CSharpSourceGeneratorVerifier<MapperGenerator>.Test
        {
            TestState =
                    {
                        Sources = { userSource },
                        GeneratedSources =
                        {
                            (typeof(MapperGenerator), "MapperExtensions.g.cs", SourceText.From(ExtensionsSource.Source, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                            (typeof(MapperGenerator), "StartMapper.g.cs", SourceText.From(StartMapperSource.StartMapper, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                            (typeof(MapperGenerator), "Mapper.g.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                        },
                    },
        }.RunAsync();

        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

        var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
        Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
    }

    [TestMethod]
    public void MapWith_NonExistentArgument()
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

var destination = source.MapWith<Destination>(nonExistent: 20);

var isValid = true;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
        var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
        Assert.IsTrue(userSourceDiagnostics.Where(x => x.Id != "CS8019").Single().ToString() == "(15,47): error CS1739: The best overload for 'MapWith' does not have a parameter named 'nonExistent'");
    }

    [TestMethod]
    public void MapWith_Enum()
    {
        var classes = @"
enum SourceEnum
{
    ValueA,
    ValueB,
    ValueC
}

enum DestinationEnum
{
    ValueA,
    ValueB,
    ValueC
}";

        var validateFunction = @"
var source = SourceEnum.ValueA;

var destination = source.MapWith<DestinationEnum>();

var isValid = true;

if (!isValid) throw new MapFailedException(source, destination);";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);
        var generator = new MapperGenerator();
        userSource.RunGenerators(out var generatorDiagnostics, generators: generator);
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM007");
    }

    [TestMethod]
    public void MapWith_And_Map_Together()
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

var destination1 = source.Map<Destination>();
var destination2 = source.MapWith<Destination>(""good"");

var isValid1 = source.Name == destination1.Name && source.Birthday == destination1.Birthday;
var isValid2 = ""good"" == destination2.Name && source.Birthday == destination2.Birthday;

if (!isValid1) throw new MapFailedException(source, destination1);
if (!isValid2) throw new MapFailedException(source, destination2);";

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
    public void MapWith_Multiple()
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
    public int Age { get; set; }
}";

        var validateFunction = @"
var source = new Source { Name = ""Anton"", Age = 20 };

var destination1 = source.MapWith<Destination>(name: ""good"");
var destination2 = source.MapWith<Destination>(age: 1000);

var isValid1 = ""good"" == destination1.Name && source.Age == destination1.Age;
var isValid2 = source.Name == destination2.Name && 1000 == destination2.Age;

if (!isValid1) throw new MapFailedException(source, destination1);
if (!isValid2) throw new MapFailedException(source, destination2);";

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
    public void MapWith_MultipleButSameType()
    {
        var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
}

public class Destination
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
}";

        var validateFunction = @"
var source = new Source { FirstName = ""Vasya"", SecondName = ""Pupkin"" };

var destination1 = source.MapWith<Destination>(firstName: ""good"");
var destination2 = source.MapWith<Destination>(secondName: ""good"");";

        var userSource = TestExtensions.GenerateSource(classes, validateFunction);

        userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
        Assert.IsTrue(generatorDiagnostics.Single().Id == "NGM010");
        Assert.IsTrue(generatorDiagnostics.Single().ToString() == "(16,20): error NGM010: Can not map 'Test.Source' to Test.Destination. Better function member can not be selected for 'MapWith'. Multiple functions have the same signatures (number and type of parameters)");
    }
}
