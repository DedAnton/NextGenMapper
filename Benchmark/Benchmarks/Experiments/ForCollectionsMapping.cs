using Benchmark.Utils;
using BenchmarkDotNet.Engines;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Reflection;
using System.ComponentModel;

namespace Benchmark.Benchmarks.Experiments;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ForCollectionsMapping
{
    [Params(100)]
    public int Length;

    private int[] array;
    private List<int> list;
    private IEnumerable<int> enumerable;
    private IEnumerable<int> arrayAsEnumerable;
    private ICollection<int> collection;
    private IList<int> iList;
    private Consumer consumer;

    [GlobalSetup]
    public void SetupBenchmark()
    {
        var rnd = new Random(12345);
        enumerable = Enumerable.Range(0, Length).Select(x => rnd.Next());
        list = enumerable.ToList();
        array = enumerable.ToArray();
        arrayAsEnumerable = array;
        var myCollection = new MyCollection<int>();
        list.ForEach(x => myCollection.Add(x));
        collection = myCollection;
        iList = myCollection;
        consumer = new Consumer();
    }

    //[Benchmark, BenchmarkCategory("MapLinq")]
    public void MapArrayLinq() => array.Select(x => x).Consume(consumer);

    //[Benchmark, BenchmarkCategory("MapLinq")]
    public void MapListLinq() => list.Select(x => x).Consume(consumer);

    //[Benchmark, BenchmarkCategory("MapLinq")]
    public void MapEnumerableLinq() => enumerable.Select(x => x).Consume(consumer);

    //[Benchmark, BenchmarkCategory("MapLinq")]
    public void MapArrayAsEnumerableLinq() => arrayAsEnumerable.Select(x => x).Consume(consumer);

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapArrayLinqToArray() => array.Select(x => x).ToArray();

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapListLinqToArray() => list.Select(x => x).ToArray();

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapEnumerableLinqToArray() => enumerable.Select(x => x).ToArray();

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapCollectionLinqToArray() => collection.Select(x => x).ToArray();

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapIListLinqToArray() => iList.Select(x => x).ToArray();

    //[Benchmark, BenchmarkCategory("MapLinqToArray")]
    public int[] MapArrayAsEnumerableLinqToArray() => arrayAsEnumerable.Select(x => x).ToArray();

    //[Benchmark]
    public List<int> MapEnumerableToList()
    {
        var output = new List<int>();
        foreach(var item in enumerable)
        {
            output.Add(item);
        }

        return output;
    }

    //[Benchmark]
    public void MapEnumerable() => MapEnumerable(enumerable).Consume(consumer);
    private IEnumerable<int> MapEnumerable(IEnumerable<int> input)
    {
        foreach (var item in input)
        {
            yield return item;
        }
    }

    //[Benchmark]
    public List<int> MapEnumerableEnumeratorToList()
    {
        var output = new List<int>();
        using (IEnumerator<int> e = enumerable.GetEnumerator())
        {
            while (e.MoveNext())
            {
                output.Add(e.Current);
            }
        }

        return output;
    }

    //[Benchmark]
    public void MapEnumerableEnumerator() => MapEnumerableEnumerator(enumerable).Consume(consumer);
    private IEnumerable<int> MapEnumerableEnumerator(IEnumerable<int> input)
    {
        using IEnumerator<int> e = input.GetEnumerator();
        while (e.MoveNext())
        {
            yield return e.Current;
        }
    }



    //[Benchmark]
    public void SmartMapEnumerable() => SmartMapEnumerable(enumerable).Consume(consumer);
    //[Benchmark]
    public void SmartMapArrayAsIEnumerable() => SmartMapEnumerable(arrayAsEnumerable).Consume(consumer);
    private IEnumerable<int> SmartMapEnumerable(IEnumerable<int> input)
    {
        if (input is int[] inputArray)
        {
            var output = new int[inputArray.Length];
            var span = inputArray.AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }

            return output;
        }
        else if (input is List<int> listInput)
        {
            var output = new int[listInput.Count];
            var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(listInput);
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }

            return output;
        }
        else
        {
            var output = new List<int>();
            using (IEnumerator<int> e = input.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    output.Add(e.Current);
                }
            }

            return output;
        }
    }

    //[Benchmark, BenchmarkCategory("SmartMap")]
    public void SmartMapArrayV2() => SmartMapEnumerableV2(array).Consume(consumer);
    //[Benchmark, BenchmarkCategory("SmartMap")]
    public void SmartMapListV2() => SmartMapEnumerableV2(list).Consume(consumer);
    //[Benchmark, BenchmarkCategory("SmartMap")]
    public void SmartMapEnumerableV2() => SmartMapEnumerableV2(enumerable).Consume(consumer);
    //[Benchmark, BenchmarkCategory("SmartMap")]
    public void SmartMapArrayAsIEnumerableV2() => SmartMapEnumerableV2(arrayAsEnumerable).Consume(consumer);
    private IEnumerable<int> SmartMapEnumerableV2(IEnumerable<int> input)
    {
        Span<int> span = default;
        var isSpan = false;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
            isSpan = true;
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
            isSpan = true;
        }


        if (isSpan)
        {
            var output = new int[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }
            return output;
        }
        else
        {
            var output = new List<int>();
            using (IEnumerator<int> e = input.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    output.Add(e.Current);
                }
            }

            return output;
        }
    }

    //[Benchmark, BenchmarkCategory("SmartMapV2ToArray")]
    public int[] SmartMapArrayV2ToArray() => SmartMapEnumerableV2ToArray(array);
    //[Benchmark, BenchmarkCategory("SmartMapV2ToArray")]
    public int[] SmartMapListVToArray2() => SmartMapEnumerableV2ToArray(list);
    //[Benchmark, BenchmarkCategory("SmartMapV2ToArray")]
    public int[] SmartMapEnumerableV2ToArray() => SmartMapEnumerableV2ToArray(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV2ToArray")]
    public int[] SmartMapArrayAsIEnumerableV2ToArray() => SmartMapEnumerableV2ToArray(arrayAsEnumerable);
    private int[] SmartMapEnumerableV2ToArray(IEnumerable<int> input)
    {
        Span<int> span = default;
        var isSpan = false;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
            isSpan = true;
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
            isSpan = true;
        }


        if (isSpan)
        {
            var output = new int[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }
            return output;
        }
        else
        {
            using var en = input.GetEnumerator();
            if (en.MoveNext())
            {
                const int DefaultCapacity = 4;
                int[] arr = new int[DefaultCapacity];
                arr[0] = en.Current;
                int count = 1;

                while (en.MoveNext())
                {
                    if (count == arr.Length)
                    {
                        int newLength = count << 1;
                        if ((uint)newLength > Array.MaxLength)
                        {
                            newLength = Array.MaxLength <= count ? count + 1 : Array.MaxLength;
                        }

                        Array.Resize(ref arr, newLength);
                    }

                    arr[count++] = en.Current;
                }

                return arr;
            }

            return Array.Empty<int>();
        }
    }

    //[Benchmark, BenchmarkCategory("SmartMapV3ToArray")]
    public int[] SmartMapEnumerableV3ToArray() => SmartMapEnumerableV3ToArray(enumerable);
    private int[] SmartMapEnumerableV3ToArray(IEnumerable<int> input)
    {
        Span<int> span = default;
        var isSpan = false;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
            isSpan = true;
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
            isSpan = true;
        }


        if (isSpan)
        {
            var output = new int[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }
            return output;
        }
        else
        {
            const int DefaultCapacity = 4;
            int[] arr = new int[DefaultCapacity];
            int count = 0;
            foreach (var item in input)
            {
                if (count == arr.Length)
                {
                    int newLength = count << 1;
                    if ((uint)newLength > Array.MaxLength)
                    {
                        newLength = Array.MaxLength <= count ? count + 1 : Array.MaxLength;
                    }

                    Array.Resize(ref arr, newLength);
                }

                arr[count++] = item;
            }

            if (count > 0)
            {
                return arr;
            }

            return Array.Empty<int>();
        }
    }

    //[Benchmark, BenchmarkCategory("SmartMapV4ToArray")]
    public int[] SmartMapEnumerableV4ToArray() => SmartMapEnumerableV4ToArray(enumerable);
    private int[] SmartMapEnumerableV4ToArray(IEnumerable<int> input)
    {
        Span<int> span = default;
        var isSpan = false;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
            isSpan = true;
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
            isSpan = true;
        }


        if (isSpan)
        {
            var output = new int[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                output[i] = span[i];
            }
            return output;
        }
        else
        {
            return input.Select(x => x).ToArray();
        }
    }

    //[Benchmark, BenchmarkCategory("SmartMapV5ToArray")]
    public int[] SmartMapEnumerableV5ToArray() => SmartMapEnumerableV5ToArray(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV5ToArray")]
    public int[] SmartMapCollectionV5ToArray() => SmartMapEnumerableV5ToArray(collection);
    //[Benchmark, BenchmarkCategory("SmartMapV5ToArray")]
    public int[] SmartMapIListV5ToArray() => SmartMapEnumerableV5ToArray(iList);
    private int[] SmartMapEnumerableV5ToArray(IEnumerable<int> input)
    {
        Span<int> span;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
        }
        else
        {
            span = input.ToArray();
        }

        var output = new int[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }
        return output;
    }

    //[Benchmark, BenchmarkCategory("SmartMapV6ToArray")]
    public int[] SmartMapEnumerableV6ToArray() => SmartMapEnumerableV6ToArray(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV6ToArray")]
    public int[] SmartMapCollectionV6ToArray() => SmartMapEnumerableV6ToArray(collection);
    //[Benchmark, BenchmarkCategory("SmartMapV6ToArray")]
    public int[] SmartMapIListV6ToArray() => SmartMapEnumerableV6ToArray(iList);
    private int[] SmartMapEnumerableV6ToArray<T>(T input) where T : IEnumerable<int>
    {
        Span<int> span;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
        }
        else
        {
            span = input.ToArray();
        }

        var output = new int[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }

    //[Benchmark, BenchmarkCategory("SmartMapV7ToArray")]
    public int[] SmartMapEnumerableV7ToArray() => MapForInterfacesV7ToArray(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV7ToArray")]
    public int[] SmartMapCollectionV7ToArray() => MapForInterfacesV7ToArray(collection);
    //[Benchmark, BenchmarkCategory("SmartMapV7ToArray")]
    public int[] SmartMapIListV7ToArray() => MapForInterfacesV7ToArray(iList);
    private T[] MapForInterfacesV7ToArray<T>(IEnumerable<T> input)
    {
        if (!input.TryGetSpan(out ReadOnlySpan<T> span))
        {
            span = input.ToArray();
        }

        var output = new T[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }
        return output;
    }

    //[Benchmark, BenchmarkCategory("SmartMapV8ToArray")]
    public int[] SmartMapEnumerableV8ToArray() => MapForInterfacesV8ToArray(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV8ToArray")]
    public int[] SmartMapCollectionV8ToArray() => MapForInterfacesV8ToArray(collection);
    //[Benchmark, BenchmarkCategory("SmartMapV8ToArray")]
    public int[] SmartMapIListV8ToArray() => MapForInterfacesV8ToArray(iList);
    private T[] MapForInterfacesV8ToArray<T>(IEnumerable<T> input)
    {
        if (!input.TryGetSpan(out var span))
        {
            span = input.ToArray();
        }

        return Extensions.MapSpanToArray(ref span);
    }

    //[Benchmark, BenchmarkCategory("SmartMapV8ToArrayNoInlining")]
    public int[] SmartMapEnumerableV8ToArrayNoInlining() => MapForInterfacesV8ToArrayNoInlining(enumerable);
    //[Benchmark, BenchmarkCategory("SmartMapV8ToArrayNoInlining")]
    public int[] SmartMapCollectionV8ToArrayNoInlining() => MapForInterfacesV8ToArrayNoInlining(collection);
    //[Benchmark, BenchmarkCategory("SmartMapV8ToArray")]
    public int[] SmartMapIListV8ToArrayNoInlining() => MapForInterfacesV8ToArrayNoInlining(iList);
    private T[] MapForInterfacesV8ToArrayNoInlining<T>(IEnumerable<T> input)
    {
        if (!input.TryGetSpanNoInliining(out ReadOnlySpan<T> span))
        {
            span = input.ToArray();
        }

        return Extensions.MapSpanToArrayNoInlining(ref span);
    }

    //[Benchmark, BenchmarkCategory("SmartMapSpecialToArray")]
    public int[] SmartMapCollectionSpecialToArray() => SmartMapCollectionSpecialToArray(collection);
    private int[] SmartMapCollectionSpecialToArray(ICollection<int> input)
    {
        Span<int> span;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
        }
        else
        {
            span = input.ToArray();
        }

        var output = new int[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }


    //[Benchmark, BenchmarkCategory("SmartMapSpecialToArray")]
    public int[] SmartMapIListSpecialToArray() => SmartMapIListSpecialToArray(iList);
    private int[] SmartMapIListSpecialToArray(IList<int> input)
    {
        Span<int> span;
        if (input.GetType() == typeof(int[]))
        {
            span = Unsafe.As<int[]>(input);
        }
        else if (input.GetType() == typeof(List<int>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<int>>(input));
        }
        else
        {
            span = input.ToArray();
        }

        var output = new int[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }


    //[Benchmark]
    public int[] MapArray()
    {
        var output = new int[array.Length];
        var span = array.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }

    //[Benchmark]
    public int[] MapArrayCreateSpan()
    {
        Span<int> output = new int[array.Length];
        var span = array.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output.ToArray();
    }

    //[Benchmark]
    public int[] MapArrayAsSpan()
    {
        var output = new int[array.Length];
        var outputSpan = output.AsSpan();
        var span = array.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            outputSpan[i] = span[i];
        }

        return output;
    }

    //[Benchmark]
    public List<int> MapList()
    {
        var output = new List<int>(list.Count);
        var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
        for (var i = 0; i < span.Length; i++)
        {
            output.Add(span[i]);
        }

        return output;
    }

    //[Benchmark]
    public List<int> MapListAsSpan()
    {
        var output = new List<int>(list.Count);
        output.AddRange(new int[list.Count]);
        Span<int> outputSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(output);
        var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
        for (var i = 0; i < span.Length; i++)
        {
            outputSpan[i] = span[i];
        }

        return output;
    }

    [Benchmark]
    public int[] MapListToArrayFor()
    {
        var output = new int[list.Count];
        for (var i = 0; i < list.Count; i++)
        {
            output[i] = list[i];
        }

        return output;
    }

    [Benchmark]
    public int[] MapListToForEach()
    {
        var output = new int[list.Count];
        var count = 0;
        foreach (var item in list)
        {
            output[count++] = item;
        }

        return output;
    }

    [Benchmark]
    public int[] MapListToArrayToArray()
    {
        var output = new int[list.Count];
        Span<int> span = list.ToArray();
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }


    [Benchmark]
    public int[] MapListToArray_Reflection()
    {
        var itemsObject = _listItemsField.GetValue(list);
        var spanAllCapacity = Unsafe.As<int[]>(itemsObject).AsSpan();
        var span = spanAllCapacity[..list.Count];

        var output = new int[list.Count];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }
    private static readonly FieldInfo _listItemsField = typeof(List<int>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

    [Benchmark]
    public int[] MapListToArray_VeryUnsafe()
    {
        var listProxy = Unsafe.As<ListProxy<int>>(list);
        var span = listProxy._items.AsSpan()[..list.Count];

        var output = new int[list.Count];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }
}

internal class ListProxy<T>
{
#pragma warning disable 0649
    internal T[] _items;
#pragma warning restore 0649
}

class MyCollection<T> : IList<T>
{
    private readonly IList<T> _list = new List<T>();

    #region Implementation of IEnumerable

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<T>

    public void Add(T item)
    {
        _list.Add(item);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _list.Remove(item);
    }

    public int Count
    {
        get { return _list.Count; }
    }

    public bool IsReadOnly
    {
        get { return _list.IsReadOnly; }
    }

    #endregion

    #region Implementation of IList<T>

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public T this[int index]
    {
        get { return _list[index]; }
        set { _list[index] = value; }
    }

    #endregion
}

internal static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSpan<TSource>(this IEnumerable<TSource> source, out ReadOnlySpan<TSource> span)
    {
        bool result = true;
        if (source.GetType() == typeof(TSource[]))
        {
            span = Unsafe.As<TSource[]>(source);
        }
        #if NET5_0_OR_GREATER
        else if (source.GetType() == typeof(List<TSource>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<TSource>>(source));
        }
        #endif
        else
        {
            span = default;
            result = false;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSource[] MapSpanToArray<TSource>(ref ReadOnlySpan<TSource> span)
    {
        var output = new TSource[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<TSource> MapSpanToList<TSource>(ref ReadOnlySpan<TSource> span)
    {
        var output = new List<TSource>(span.Length);
        for (var i = 0; i < span.Length; i++)
        {
            output.Add(span[i]);
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetSpanNoInliining<TSource>(this IEnumerable<TSource> source, out ReadOnlySpan<TSource> span)
    {
        bool result = true;
        if (source.GetType() == typeof(TSource[]))
        {
            span = Unsafe.As<TSource[]>(source);
        }
        else if (source.GetType() == typeof(List<TSource>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<TSource>>(source));
        }
        else
        {
            span = default;
            result = false;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TSource[] MapSpanToArrayNoInlining<TSource>(ref ReadOnlySpan<TSource> span)
    {
        var output = new TSource[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            output[i] = span[i];
        }

        return output;
    }
}