using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;
using System.Reflection;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class CustomMapping
    {
        [TestMethod]
        public void CustomExpressionMapping()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public string Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString();

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"", Birthday = source.Birthday.ToShortDateString() };
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
        public void CustomBlockMapping()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public DateTime Birthday { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public string Birthday { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString();

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
public Destination Map(Source source)
{
    var name = $""{source.FirstName} {source.SecondName}"";
    
    return new Destination { Name = name, Birthday = source.Birthday.ToShortDateString() };
}
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
        public void CustomMappingForSpecialTypes()
        {
            var classes = @"
public class Source
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public SourceEnum EnumProperty { get;set; }
}

public class Destination
{
    public string Name { get; set; }
    public string Birthday { get; set; }
    public byte EnumProperty { get; set; }
}

public enum SourceEnum
{
    First = 1,
    Second = 20,
    Third = 300
}
"
;

            var validateFunction = @"
var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20), EnumProperty = SourceEnum.Second };

var destination = source.Map<Destination>();

var isValid = destination.Name == source.Name && destination.Birthday == source.Birthday.ToShortDateString() && destination.EnumProperty == 20;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
public string Map(DateTime src) => src.ToShortDateString();
public byte Map(SourceEnum enumSrc) => (byte)enumSrc;
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
        public void CustomMappingWithUsingAliases()
        {
            var userSource =
@"using NextGenMapper;
using System;
using Models = Test.Models;
using DestinationAlias = Test.Models.Destination;

namespace Test
{
    public class Program
    {
        public void TestMethod()
        {
            var source = new Models.Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) };

            var destination = source.Map<DestinationAlias>();

            var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == ""20.05.1997"";

            if (!isValid) throw new MapFailedException(source, destination);
        }
    }

    public class MapFailedException : System.Exception 
    {
        public object MapSource { get; set; }
        public object MapDestination { get; set; }

        public MapFailedException(object source, object destination) 
            : base()
        {
            MapSource = source;
            MapDestination = destination;
        }
    }

    [Mapper]
    public class CustomMapper
    {
        public DestinationAlias Map(Models.Source source) => new Models.Destination { Name = $""{source.FirstName} {source.SecondName}"", Birthday = source.Birthday.ToShortDateString() };
    }
}

namespace Test.Models
{
    public class Source
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
    }
}
";
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsEmpty, generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void CustomEnumMapping()
        {
            var classes = @"
public enum EnumFrom   
{   ValueA,
    ValueB,
    ValueC
}

public enum EnumTo
{
    ValueA = 10,
    ValueB = 20,
    ValueC = 30
}";

            var validateFunction = @"
var source = EnumFrom.ValueB;

var destination = source.Map<EnumTo>();

var isValid = destination == EnumTo.ValueB;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
public EnumTo Map(EnumFrom source)
{
    return source switch
    {
        EnumFrom.ValueA => EnumTo.ValueA,
        EnumFrom.ValueB => EnumTo.ValueB,
        EnumFrom.ValueC => EnumTo.ValueC,
        _ => throw new ArgumentOutOfRangeException($""value {source} was out of range of {typeof(EnumTo)}"")
    };
}
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
        public void CustomMappingTwoMappers()
        {
            var source1 =
@"using NextGenMapper;
using System;
using System.Collections.Generic;

namespace Test.First
{
    public class Program
    {
        public void TestMethod()
        {
            var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) };

            var destination = source.Map<Destination>();

            var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString();

            if (!isValid) throw new MapFailedException(source, destination);
        }
    }

    public class MapFailedException : System.Exception 
    {
        public object MapSource { get; set; }
        public object MapDestination { get; set; }

        public MapFailedException(object source, object destination) 
            : base()
        {
            MapSource = source;
            MapDestination = destination;
        }
    }

    public class Source
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
    }

    [Mapper]
    public class CustomMapper
    {
        public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"", Birthday = source.Birthday.ToShortDateString() };
    }
}
";

            var source2 =
@"using NextGenMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Second
{
    public class Program
    {
        public void TestMethod()
        {
            var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20) };

            var destination = source.Map<Destination>();

            var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString();

            if (!isValid) throw new MapFailedException(source, destination);
        }
    }

    public class MapFailedException : System.Exception 
    {
        public object MapSource { get; set; }
        public object MapDestination { get; set; }

        public MapFailedException(object source, object destination) 
            : base()
        {
            MapSource = source;
            MapDestination = destination;
        }
    }

    public class Source
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
    }

    [Mapper]
    public class CustomMapper
    {
        public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"", Birthday = source.Birthday.ToShortDateString() };
    }
}
";

            var compilation = CSharpCompilation.Create(
                assemblyName: nameof(CustomMappingTwoMappers),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source1, new CSharpParseOptions(LanguageVersion.CSharp9)), CSharpSyntaxTree.ParseText(source2, new CSharpParseOptions(LanguageVersion.CSharp9)) },
                references: new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.CreateDriver(new MapperGenerator()).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var generatorDiagnostics);
            Assert.IsTrue(generatorDiagnostics.IsEmpty, generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = updatedCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));
        }
    }
}
