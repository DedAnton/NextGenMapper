using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using NextGenMapper.PostInitialization;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class MapperClassExtension
    {
        [TestMethod]
        public void AddCustomMappingMethod()
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
    public string Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && ""1997-05-20T00:00:00.0000000"" == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapper = @"
namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static string Map<To>(this DateTime source) => source.ToString(""O"");
    }
}
";
            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapper);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void OverrideMappingMethod()
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

var isValid = ""Vasya"" == destination.Name && new DateTime(2000, 01, 12) == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapper = @"
namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination { Name = ""Vasya"", Birthday = new DateTime(2000, 01, 12) };
    }
}
";
            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapper);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void OverrideMappingMethodUsingMapWith()
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

var isValid = ""Vasya"" == destination.Name && new DateTime(1997, 05, 20) == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapper = @"
namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => source.MapWith<Test.Destination>(name: ""Vasya"");
    }
}
";
            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapper);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        //TODO: add test for file-scoped namespace
        [TestMethod]
        public async Task AddCustomMappingMethod_FileScopedNamespace()
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
    public string Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && ""1997-05-20T00:00:00.0000000"" == destination.Birthday;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapper = @"
using System;

namespace NextGenMapper;

internal static partial class Mapper
{
    internal static string Map<To>(this DateTime source) => source.ToString(""O"");
}
";
            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var generated = "using NextGenMapper.Extensions;\r\n\r\nnamespace NextGenMapper\r\n{\r\n    internal static partial class Mapper\r\n    {\r\n        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination\r\n        (\r\n        )\r\n        {\r\n            Name = source.Name,\r\n            Birthday = source.Birthday.Map<string>()\r\n        };\r\n\r\n    }\r\n}";
            await new CSharpSourceGeneratorVerifier<MapperGenerator>.Test
            {
                TestState =
                    {
                        Sources = { userSource, customMapper },
                        GeneratedSources =
                        {
                            (typeof(MapperGenerator), "MapperExtensions.g.cs", SourceText.From(ExtensionsSource.Source, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                            (typeof(MapperGenerator), "StartMapper.g.cs", SourceText.From(StartMapperSource.StartMapper, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                            (typeof(MapperGenerator), "Mapper.g.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                        }
                    },
            }.RunAsync();
        }
    }
}