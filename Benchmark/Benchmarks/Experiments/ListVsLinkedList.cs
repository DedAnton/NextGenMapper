namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class ListVsLinkedList
{
    [Params(10, 100, 1000)]
    public int Length;

    [Benchmark]
    public List<int> List()
    {
        var list = new List<int>();
        for (int i = 0; i < Length; i++)
        {
            list.Add(i);
        }

        return list;
    }

    //[Benchmark]
    //public NextGenMapper.Utils.MapsLinkedList<int> LinkedList()
    //{
    //    var list = NextGenMapper.Utils.MapsLinkedList<int>.Create();
    //    for (int i = 0; i < Length; i++)
    //    {
    //        list.Add(i);
    //    }

    //    return list;
    //}

    [Benchmark]
    public int[] Array()
    {
        var array = System.Array.Empty<int>();
        for (int i = 0; i < Length; i++)
        {
            var newArray = new int[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = i;
            array = newArray;
        }

        return array;
    }
}
