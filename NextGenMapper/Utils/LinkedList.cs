using NextGenMapper.Mapping.Maps;
using System.Collections.Immutable;

namespace NextGenMapper.Utils;

internal ref struct MapsList
{
    private Node? _head;
    private Node? _tail;
    private  int _count;

    private MapsList(Node? head, Node? tail, int count)
    {
        _head = head;
        _tail = tail;
        _count = count;
    }

    public MapsList()
    {
        _head = null;
        _tail = null;
        _count = 0;
    }

    public int Count => _count;

    public static MapsList Create(Map value)
    {
        var node = new Node(value);

        return new MapsList(node, node, 1);
    }

    public void AddFirst(Map value)
    {
        var node = new Node(value);

        if (_tail == null)
        {
            _head = node;
            _tail = node;
            _count = 1;
        }
        else
        {
            node._next = _head;
            _head = node;
            _count++;
        }
    }

    public void Append(MapsList list)
    {
        if (_tail == null)
        {
            _head = list._head;
            _tail = list._tail;
            _count = list._count;
        }
        else
        {
            _tail._next = list._head;
            _count += list._count;
        }
    }

    public Map[] ToArray()
    {
        var current = _tail;
        var array = new Map[_count];
        var count = 0;

        while (current != null)
        {
            array[count] = current.Value;
            current = current._next;
            count++;
        }

        return array;
    }

    public ImmutableArray<Map> ToImmutableArray()
    {
        var array = ToArray();
        return Unsafe.CastArrayToImmutableArray(ref array);
    }

    private class Node
    {
        internal Node? _next;
        internal readonly Map Value;

        public Node(Map value)
        {
            Value = value;
        }
    }
}
