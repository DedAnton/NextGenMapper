using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;


namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class FlatteningAndUnflatteningTest
    {
        [TestMethod]
        public void MappingByInitializerWithFlattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new User { Address = new Address { City = ""Dinamo"", Street = ""3rd Builders Street"" } };

var destination = source.Map<UserFlat>();

var isValid = source.Address.City == destination.AddressCity && source.Address.Street == destination.AddressStreet;

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
        public void MappingByConstructorWithFlattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; }
    public string AddressStreet { get; }

    public UserFlat(string addressCity, string addressStreet)
    {
        AddressCity = addressCity;
        AddressStreet = addressStreet;
    }
}";

            var validateFunction = @"
var source = new User { Address = new Address { City = ""Dinamo"", Street = ""3rd Builders Street"" } };

var destination = source.Map<UserFlat>();

var isValid = source.Address.City == destination.AddressCity && source.Address.Street == destination.AddressStreet;

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
        public void MappingByInitializerWithUnflattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"" };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet;

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
        public void MappingByConstructorWithUnflattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; }

    public User(Address address)
    {
        Address = address;
    }
}

public class Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = city;
        Street = street;
    }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"" };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet;

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
        public void MappingByConstructorAndInitializerWithTwoFlattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
    public PhoneNumber PhoneNumber { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class PhoneNumber
{
    public string Code { get; set; }
    public string Number { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; }
    public string AddressStreet { get; }
    public string PhoneNumberNumber { get; set; }
    public string PhoneNumberCode { get; set; }

    public UserFlat(string addressCity, string addressStreet)
    {
        AddressCity = addressCity;
        AddressStreet = addressStreet;
    }
}";

            var validateFunction = @"
var source = new User { Address = new Address { City = ""Dinamo"", Street = ""3rd Builders Street"" }, PhoneNumber = new PhoneNumber { Number = ""(911) 012 34 56"", Code = ""+7""} };

var destination = source.Map<UserFlat>();

var isValid = source.Address.City == destination.AddressCity && source.Address.Street == destination.AddressStreet 
    && source.PhoneNumber.Number == destination.PhoneNumberNumber && source.PhoneNumber.Code == destination.PhoneNumberCode;

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
        public void MappingByConstructorAndInitializerWithTwoUnflattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; }
    public PhoneNumber PhoneNumber { get; set; }

    public User(Address address)
    {
        Address = address;
    }
}

public class Address
{
    public string City { get; }
    public string Street { get; set; }

    public Address(string city)
    {
        City = city;
    }
}

public class PhoneNumber
{
    public string Code { get; set; }
    public string Number { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
    public string PhoneNumberNumber { get; set; }
    public string PhoneNumberCode { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", PhoneNumberNumber = ""(911) 012 34 56"", PhoneNumberCode = ""+7"" };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet 
    && destination.PhoneNumber.Number == source.PhoneNumberNumber && destination.PhoneNumber.Code == source.PhoneNumberCode;

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
        public void PartialMappingByInitializerWithFlattening()
        {
            var classes = @"
public class User
{
    public DateTime Date { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class UserFlat
{
    public string Date { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new User { Address = new Address { City = ""Dinamo"", Street = ""3rd Builders Street"" }, Date = new DateTime(2021, 08, 09) };

var destination = source.Map<UserFlat>();

var isValid = source.Address.City == destination.AddressCity && source.Address.Street == destination.AddressStreet && source.Date.ToShortDateString() == destination.Date;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public UserFlat Map(User source) => new UserFlat { Date = source.Date.ToShortDateString() };
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void PartialMappingByConstructorWithUnflattening()
        {
            var classes = @"
public class User
{
    public string Date { get; set; }
    public Address Address { get; }

    public User()
    {
    }

    public User(Address address)
    {
        Address = address;
    }
}

public class Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = city;
        Street = street;
    }
}

public class UserFlat
{
    public DateTime Date { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", Date = new DateTime(2021, 08, 09) };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet && source.Date.ToShortDateString() == destination.Date;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public User Map(UserFlat source) => new User { Date = source.Date.ToShortDateString() };
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void PartialJOCMappingWithFlattening()
        {
            var classes = @"
public class User
{
    public DateTime Date { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
}

public class UserFlat
{
    public string Date { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }

    public UserFlat(string date, string addressCity, string addressStreet)
    {
        Date = date;
        AddressCity = addressCity;
        AddressStreet = addressStreet;
    }
}";

            var validateFunction = @"
var source = new User { Address = new Address { City = ""Dinamo"", Street = ""3rd Builders Street"" }, Date = new DateTime(2021, 08, 09) };

var destination = source.Map<UserFlat>();

var isValid = source.Address.City == destination.AddressCity && source.Address.Street == destination.AddressStreet && source.Date.ToShortDateString() == destination.Date;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public UserFlat Map(User source) => new UserFlat(source.Date.ToShortDateString(), default, default);
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void PartialJOCMappingWithUnflattening()
        {
            var classes = @"
public class User
{
    public string Date { get; }
    public Address Address { get; }

    public User(string date, Address address)
    {
        Date = date;
        Address = address;
    }
}

public class Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = city;
        Street = street;
    }
}

public class UserFlat
{
    public DateTime Date { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", Date = new DateTime(2021, 08, 09) };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet && source.Date.ToShortDateString() == destination.Date;

if (!isValid) throw new MapFailedException(source, destination);";

            var customMapping = @"
[Partial]
public User Map(UserFlat source) => new User(source.Date.ToShortDateString(), default);
";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction, customMapping);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));

            var testResult = userSourceCompilation.TestMapper(out var source, out var destination, out var message);
            Assert.IsTrue(testResult, TestExtensions.GetObjectsString(source, destination, message));
        }

        [TestMethod]
        public void MappingByConstructorWithUnflatteningAndNotUnflatteningProperty()
        {
            var classes = @"
public class User
{
    public Address Address { get; }
    public string NotUnflatteningProperty { get; }

    public User(Address address, string notUnflatteningProperty)
    {
        Address = address;
        NotUnflatteningProperty = notUnflatteningProperty;
    }
}

public class Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = city;
        Street = street;
    }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
    public string NotUnflatteningProperty { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", NotUnflatteningProperty = ""value"" };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet && destination.NotUnflatteningProperty == source.NotUnflatteningProperty;

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
        public void MappingByInitializerWithUnflatteningAndNotUnflatteningProperty()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
    public string NotUnflatteningProperty { get; set; }
}

public class Address
{
    public string City { get; }
    public string Street { get; }

    public Address(string city, string street)
    {
        City = city;
        Street = street;
    }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
    public string NotUnflatteningProperty { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", NotUnflatteningProperty = ""value"" };

var destination = source.Map<User>();

var isValid = destination.Address.City == source.AddressCity && destination.Address.Street == source.AddressStreet && destination.NotUnflatteningProperty == source.NotUnflatteningProperty;

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
        public void MappingByInitializerWithFalseUnflattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; set; }
    public string NotUnflatteningProperty { get; set; }
}

public class Address
{
    public string Property { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
    public string NotUnflatteningProperty { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", NotUnflatteningProperty = ""value"" };

var destination = source.Map<User>();

var isValid = destination.Address == null && destination.NotUnflatteningProperty == source.NotUnflatteningProperty;

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
        public void MappingByConstructorWithFalseUnflattening()
        {
            var classes = @"
public class User
{
    public Address Address { get; }
    public string NotUnflatteningProperty { get; }

    public User(Address address, string notUnflatteningProperty)
    {
        Address = address;
        NotUnflatteningProperty = notUnflatteningProperty;
    }
}

public class Address
{
    public string Property { get; set; }
}

public class UserFlat
{
    public string AddressCity { get; set; }
    public string AddressStreet { get; set; }
    public string NotUnflatteningProperty { get; set; }
}";

            var validateFunction = @"
var source = new UserFlat { AddressCity = ""Dinamo"", AddressStreet = ""3rd Builders Street"", NotUnflatteningProperty = ""value"" };

var destination = source.Map<User>();

var isValid = destination.Address == null && destination.NotUnflatteningProperty == null;

if (!isValid) throw new MapFailedException(source, destination);";

            var userSource = TestExtensions.GenerateSource(classes, validateFunction);
            var userSourceCompilation = userSource.RunGenerators(out var generatorDiagnostics, generators: new MapperGenerator());
            Assert.IsTrue(generatorDiagnostics.IsFilteredEmpty(), generatorDiagnostics.PrintDiagnostics("Generator deagnostics:"));
            var userSourceDiagnostics = userSourceCompilation.GetDiagnostics();
            Assert.IsTrue(userSourceDiagnostics.IsFilteredEmpty(), userSourceDiagnostics.PrintDiagnostics("Users source diagnostics:"));
            var testResult = userSourceCompilation.TestMapper(out _, out _, out var message);
            Assert.IsTrue(testResult == false && message == "Error when mapping Test.UserFlat to Test.User, mapping function was not found. Create custom mapping function.");
        }

    }
}
