using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests
{
    [TestClass]
    public class PartialMapping
    {
        [TestMethod]
        public void PartialExpressionMapping()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public DateTime Birthday { get; set; }
    public int Height { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public string Birthday { get; set; }
    public int Height { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20), Height = 190 };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString() && source.Height == destination.Height;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
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
        public void PartialBlockMapping()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public DateTime Birthday { get; set; }
    public int Height { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public string Birthday { get; set; }
    public int Height { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Birthday = new DateTime(1997, 05, 20), Height = 190 };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Birthday == source.Birthday.ToShortDateString() && source.Height == destination.Height;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
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
        public void PartialMappingWithInitializer()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"" };
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
        public void PartialMappingWithConstructor()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public int Height { get; }
    public string City { get; }

    public Destination() { }

    public Destination(string name, int height, string city)
    {
        Name = name;
        Height = height;
        City = city;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && source.City == destination.City;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"" };
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
        public void PartialMappingWithCustomConstructor()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; }
    public int Height { get; }
    public string City { get; }

    public Destination(string name) => Name = name;

    public Destination(string name, int height, string city)
    {
        Name = name;
        Height = height;
        City = city;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && source.City == destination.City;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"");
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
        public void PartialMappingWithConstructorAndInitializer()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public int Height { get; }
    public string City { get; set; }

    public Destination() { }

    public Destination(string name, int height)
    {
        Name = name;
        Height = height;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && source.City == destination.City;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"" };
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
        public void PartialMappingWithNotSettableProperties()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; }
    public int Height { get; set; }
    public string City { get; } = ""none"";

    public Destination(string name)
    {
        Name = name;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height && destination.City == ""none"";

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"");
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
        public void PartialMappingWithNotNotMatchedConstructor()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
    public string City { get; set; }
}

public class Destination
{
    public string Name { get; }
    public int Height { get; } = -1;
    public string City { get; } = ""none"";

    public Destination(string name) 
    {
        Name = name;
    }

    public Destination(string name, int height, string city, string notMatchedParameter)
    {
        Name = name;
        Height = height;
        City = notMatchedParameter;
    }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190, City = ""Dinamo""};

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && destination.Height == -1 && destination.City == ""none"";

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination($""{source.FirstName} {source.SecondName}"");
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
        public void PartialMappingWithImplicitConversion()
        {
            var classes = @"
public class Source
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public int Height { get; set; }
}

public class Destination
{
    public string Name { get; set; }
    public long Height { get; set; }
}";

            var validateFunction = @"
var source = new Source { FirstName = ""Anton"", SecondName = ""Ryabchikov"", Height = 190 };

var destination = source.Map<Destination>();

var isValid = destination.Name == ""Anton Ryabchikov"" && source.Height == destination.Height;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination { Name = $""{source.FirstName} {source.SecondName}"" };
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
        public void PartialMappingBadParametersOrder()
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
    public Destination(DateTime birthday, string name)
    {
        Name = name;
        Birthday = birthday;
    }

    public Destination()
    { }

    public string Name { get; }
    public DateTime Birthday { get; }
    public string Id { get; set; }
}";

            var validateFunction = @"
var source = new Source { Id = 123, Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

var destination = source.Map<Destination>();

var isValid = source.Name == destination.Name && source.Birthday == destination.Birthday && source.Id.ToString() == destination.Id;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public Destination Map(Source source) => new Destination { Id = source.Id.ToString() };
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
