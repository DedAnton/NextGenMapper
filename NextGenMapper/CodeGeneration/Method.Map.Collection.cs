using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    public void GenerateCollectionMapMethod(CollectionMap map)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(MapperSourceBuilder)} was not initialized");
        }
        MapMethodDifinition(map.From, map.To);
        CollectionMappingMethodBody(map);
    }

    private void CollectionMappingMethodBody(CollectionMap map)
    {
        _builder.Append("\r\n        {\r\n");
        SourceCollection(map);
        DestinationCollection(map);
        _builder.Append("            for (var i = 0; i < length; i++)\r\n            {\r\n");
        DestinationCollectionAssigment(map);
        _builder.Append("\r\n            }\r\n\r\n");
        _builder.Append("            return destination;\r\n        }");
    }

    private void SourceCollection(CollectionMap map)
    {
        if (map.CollectionFrom.IsInterface())
        {
            _builder.Append(
@"            if (!source.TryGetSpan(out var sourceCollection))
            {
                sourceCollection = System.Linq.Enumerable.ToArray(source);
            }
            var length = sourceCollection.Length;
");
        }

        if (map.CollectionFrom.IsList())
        {
            _builder.Append(
@"            #if NET5_0_OR_GREATER
            var sourceCollection = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);
            var length = sourceCollection.Length;
            #else
            var sourceCollection = source;
            var length = sourceCollection.Count;
            #endif
");
        }

        if (map.CollectionFrom.IsArray())
        {
            _builder.Append(
                $"            var sourceCollection = new System.Span<{map.ItemFrom}>(source);\r\n" +
                $"            var length = sourceCollection.Length;\r\n");
        }
    }

    private void DestinationCollection(CollectionMap map)
    {
        if (map.CollectionTo.IsArray() || map.CollectionTo.IsArrayInterface())
        {
            _builder.Append($"            var destination = new {map.ItemTo}[length];\r\n");
        }

        if (map.CollectionTo.IsList() || map.CollectionTo.IsListInterface())
        {
            _builder.Append($"            var destination = new System.Collections.Generic.List<{map.ItemTo}>(length);\r\n");
        }
    }

    private void DestinationCollectionAssigment(CollectionMap map)
    {
        if (map.CollectionTo.IsArray() || map.CollectionTo.IsArrayInterface())
        {
            _builder.Append("                destination[i] = sourceCollection[i]");

            if (!map.IsItemsTypesEquals && !_compilation.HasImplicitConversion(map.ItemFrom, map.ItemTo))
            {
                _builder.Append($".Map<{map.ItemTo}>()");
            }
        }

        if (map.CollectionTo.IsList() || map.CollectionTo.IsListInterface())
        {
            _builder.Append("                destination.Add(sourceCollection[i]");

            if (!map.IsItemsTypesEquals && !_compilation.HasImplicitConversion(map.ItemFrom, map.ItemTo))
            {
                _builder.Append($".Map<{map.ItemTo}>()");
            }

            _builder.Append(')');
        }

        _builder.Append(';');
    }
}
