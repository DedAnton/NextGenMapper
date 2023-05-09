using System.Collections.Immutable;

namespace NextGenMapperTests.Tests.AutomaticMapping;

[TestClass]
public class Collections : SourceGeneratorVerifier
{
    public override string TestGroup => "Map";

    [DataRow("var source = new List<int> { 1, 2, 3 };", "List")]
    [DataRow("var source = new byte[] { 1, 2, 3 };", "Array")]
    [DataRow("IEnumerable<int> source = new MyCollection<int> { 1, 2, 3 };", "IEnumerable")]
    [DataRow("IEnumerable<int> source = new int[] { 1, 2, 3 };", "ArrayAsIEnumerable")]
    [DataRow("IEnumerable<int> source = new List<int> { 1, 2, 3 };", "ListAsIEnumerable")]
    [DataRow("ICollection<int> source = new MyCollection<int> { 1, 2, 3 };", "ICollection")]
    [DataRow("IList<int> source = new MyCollection<int> { 1, 2, 3 };", "IList")]
    [DataRow("IReadOnlyList<int> source = new MyCollection<int> { 1, 2, 3 };", "IReadOnlyList")]
    [DataRow("IReadOnlyCollection<int> source = new MyCollection<int> { 1, 2, 3 };", "IReadOnlyCollection")]
    //[DataRow("var source = ImmutableArray.Create(1, 2, 3);", "ImmutableArray")]
    //[DataRow("var source = ImmutableList.Create(1, 2, 3);", "ImmutaleList")]
    [DataTestMethod]
    public Task MapFromDifferentCollections_ShouldMap(string sourceCollection, string variantName)
    {
        var source =
@"using NextGenMapper;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Test;

public class Program
{
   
    public object RunTest()
    {
        " + sourceCollection + @"        

        return source.Map<int[]>();
    }
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

        return VerifyAndRun(source, variant: variantName);
    }

    [DataRow("List<int>", "List")]
    [DataRow("long[]", "Array")]
    [DataRow("IEnumerable<long>", "IEnumerable")]
    [DataRow("ICollection<int>", "ICollection")]
    [DataRow("IList<int>", "IList")]
    [DataRow("IReadOnlyList<int>", "IReadOnlyList")]
    [DataRow("IReadOnlyCollection<int>", "IReadOnlyCollection")]
    //[DataRow("ImmutableArray<int>", "ImmutableArray")]
    //[DataRow("ImmutableList<int>", "ImmutableList")]
    [DataTestMethod]
    public Task MapToDifferentCollections_ShouldMap(string destinationCollectionType, string variantName)
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Test;

public class Program
{
   
    public object RunTest()
    {
        IEnumerable<int> source = new int[] { 1, 2, 3 };       

        return source.Map<" + destinationCollectionType + @">();
    }
}";

        return VerifyAndRun(source, variant: variantName);
    }

    [TestMethod]
    public Task CollectionOfClasses_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source[] { new Source(1), new Source(2), new Source(3) };       

        return source.Map<List<Destination>>();
    }
}

public record Source(int Property);

public class Destination
{
    public int Property { get; set; }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task CollectionOfEnums_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source[] { Source.A, Source.B, Source.C };       

        return source.Map<List<Destination>>();
    }
}

public enum Source
{
    A,
    B,
    C
}

public enum Destination
{
    A,
    B,
    C
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task CollectionOfCollections_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new int[][] { new int[] { 1, 2, 3}, new int[] { 4, 5, 6}, new int[] { 7, 8, 9} };       

        return source.Map<List<List<int>>>();
    }
}";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task UnmapableClass_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest()
    {
        var source = new Source[] { new Source() };       

        return source.Map<List<Destination>>();
    }
}

public class Source
{
    public int PropertyA { get; set; } = 1;
}

public class Destination
{
    public int PropertyB { get; set; }
}";

        return VerifyOnly(source);
    }

    [TestMethod]
    public Task WithCustomMapping_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test
{
    public class Program
    {
        public object RunTest()
        {
            var source = new Source[] { new Source() };       

            return source.Map<List<Destination>>();
        }
    }

    public class Source
    {
        public int PropertyA { get; set; } = 1;
    }

    public class Destination
    {
        public int PropertyB { get; set; }
    }
}

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => new Test.Destination { PropertyB = source.PropertyA };
    }
}


";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task CirularReferences_Diagnostic()
    {
        var source =
@"using NextGenMapper;

namespace Test;

public class Program
{
    public object RunTest() => new Source[] { new Source() }.Map<Destination[]>();
}

public class Source
{
    public Source[] Property { get; set; } = new Source[0];
}

public class Destination
{
    public Destination[] Property { get; set; }
}
";
        return VerifyOnly(source);
    }

    [TestMethod]
    public Task NotSupportedCollectionType_Diagnostic()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;

namespace Test;

public class Program
{
    public object RunTest() => new int[] { 1, 2, 3 }.Map<Dictionary<int, int>>();
}
";
        return VerifyOnly(source);
    }

    [TestMethod]
    public Task NotSupportedCollectionTypeWithCustomMapping_ShouldMap()
    {
        var source =
@"using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    public class Program
    {
        public object RunTest() => new int[] { 1, 2, 3 }.Map<Dictionary<int, int>>();
    }
}

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Dictionary<int, int> Map<To>(this int[] source) => source.ToDictionary(x => x, x => x);
    }
}


";

        return VerifyAndRun(source);
    }

    [TestMethod]
    public Task MapNullableReferenceTypeCollectionWithNullValueAndCallMapMethod_ShouldMap()
    {
        var source =
@"#nullable enable
using NextGenMapper;

namespace Test
{
    public class Program
    {
        public object RunTest() => new Source?[] { null }.Map<Destination?[]>();
    }
}

record Source(int Property);
record Destination(int Property);
";

        return VerifyAndRun(source);
    }
}
