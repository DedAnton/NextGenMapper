using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextGenMapper;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class CollectionsMapping
    {
        //List
        [DataRow("var source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("var source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("var source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("var source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("var source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("var source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("var source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //Array
        [DataRow("var source = new Source[] { source1, source2 };", "List <Destination>")]
        [DataRow("var source = new Source[] { source1, source2 };", "Destination[]")]
        [DataRow("var source = new Source[] { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("var source = new Source[] { source1, source2 };", "ICollection<Destination>")]
        [DataRow("var source = new Source[] { source1, source2 };", "IList<Destination>")]
        [DataRow("var source = new Source[] { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("var source = new Source[] { source1, source2 };", "IReadOnlyList<Destination>")]

        //IEnumerable
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //Array as IEnumerable
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "List<Destination>")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "Destination[]")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "IList<Destination>")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new Source[] { source1, source2 };", "IReadOnlyList<Destination>")]

        //List as IEnumerable
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IEnumerable<Source> source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //ICollection
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "Destination[]")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("ICollection<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //Array as ICollection
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "List<Destination>")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "Destination[]")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "ICollection<Destination>")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "IList<Destination>")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("ICollection<Source> source = new Source[] { source1, source2 };", "IReadOnlyList<Destination>")]

        //List as ICollection
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("ICollection<Source> source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //IList
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IList<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //Array as IList
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "List<Destination>")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "Destination[]")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "IList<Destination>")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IList<Source> source = new Source[] { source1, source2 };", "IReadOnlyList<Destination>")]

        //List as IList
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IList<Source> source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //IReadOnlyCollection
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //List as IReadOnlyCollection
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IReadOnlyCollection<Source> source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //IReadOnlyList
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new MyCollection<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        //List as IReadOnlyList
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "List<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "Destination[]")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "IEnumerable<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "ICollection<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "IList<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "IReadOnlyCollection<Destination>")]
        [DataRow("IReadOnlyList<Source> source = new List<Source> { source1, source2 };", "IReadOnlyList<Destination>")]

        [DataTestMethod]
        public void CollectionMapping(string sourceTypeVar, string destinationType)
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
}

public class MyCollection<T> : IEnumerable<T>, ICollection<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
{
    private readonly List<T> _items = new();

    public T this[int index]
    {
        get { return _items[index]; }
        set { _items.Insert(index, value); }
    }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public void Add(T item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(T item) => _items.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    public int IndexOf(T item) => _items.IndexOf(item);

    public void Insert(int index, T item) => _items.Insert(index, item);

    public bool Remove(T item) => _items.Remove(item);

    public void RemoveAt(int index) => _items.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
";

            var validateFunction = @"
var source1 = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };
var source2 = new Source { Name = ""Roman"", Birthday = new DateTime(1996, 07, 08) };
" + sourceTypeVar + @"

var destinationRaw = source.Map<" + destinationType + @">();
var destination = System.Linq.Enumerable.ToArray(destinationRaw);

var destinationArray = System.Linq.Enumerable.ToArray(destination);
var sourceArray = System.Linq.Enumerable.ToArray(source);

var isValid = sourceArray[0].Name == destinationArray[0].Name && sourceArray[0].Birthday == destinationArray[0].Birthday
           && sourceArray[1].Name == destinationArray[1].Name && sourceArray[1].Birthday == destinationArray[1].Birthday;

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

        [TestMethod]
        public void MappingCollectionOfCollection()
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
var collection1 = new Source[] { source1, source2 };
var collection2 = new Source[] { source1, source2 };
var source = new List<Source[]> { collection1, collection2 };

var destination = source.Map<List<Destination>[]>();

var isValid = source[0][0].Name == destination[0][0].Name && source[0][0].Birthday == destination[0][0].Birthday
    && source[0][1].Name == destination[0][1].Name && source[0][1].Birthday == destination[0][1].Birthday
    && source[1][0].Name == destination[1][0].Name && source[1][0].Birthday == destination[1][0].Birthday
    && source[1][1].Name == destination[1][1].Name && source[1][1].Birthday == destination[1][1].Birthday;

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
