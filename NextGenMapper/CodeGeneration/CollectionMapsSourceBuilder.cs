﻿using NextGenMapper.Mapping.Maps;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct CollectionMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<CollectionMap> collectionMaps)
    {
        _builder.Append(
@"#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
");
        var counter = 1;
        foreach (var map in collectionMaps)
        {
            GenerateCollectionMapMethod(map);
            if (counter < collectionMaps.Length)
            {
                _builder.Append("\r\n\r\n");
            }
            counter++;
        }

        _builder.Append("\r\n    }\r\n}");

        return _builder.ToString();
    }

    public override string ToString() => _builder.ToString();
    public ReadOnlySpan<char> AsSpan() => _builder.AsSpan();

    private void MapMethodDifinition(string from, string to)
    {
        _builder.Append($"        internal static {to} Map<To>(this {from} source)");
    }

    public void GenerateCollectionMapMethod(CollectionMap map)
    {
        MapMethodDifinition(map.Source, map.Destination);
        CollectionMappingMethodBody(map);
    }

    private void CollectionMappingMethodBody(CollectionMap map)
    {
        _builder.Append("\r\n        {\r\n");
        SourceCollection(map);
        DestinationCollection(map);
        _builder.Append("            for (var i = 0; i < length; i++)\r\n            {\r\n");
        DestinationCollectionAssignment(map);
        _builder.Append("\r\n            }\r\n\r\n");
        Return(map);
    }

    private void SourceCollection(CollectionMap map)
    {
        var nullCheck = map.IsSourceItemNullable ? "?" : "";

        if (map.SourceKind.IsInterface())
        {
            _builder.Append(
@"            if (!source.TryGetSpan(out var sourceCollection))
            {
                sourceCollection = System.Linq.Enumerable.ToArray(source);
            }
            var length = sourceCollection.Length;
");
        }

        if (map.SourceKind.IsList())
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

        if (map.SourceKind.IsArray())
        {

            _builder.Append(
                $"            var sourceCollection = new System.Span<{map.SourceItem}{nullCheck}>(source);\r\n" +
                $"            var length = sourceCollection.Length;\r\n");
        }

        if (map.SourceKind.IsImmutableArray())
        {
            _builder.Append(
                $"            var sourceCollection = source.AsSpan();\r\n" +
                $"            var length = sourceCollection.Length;\r\n");
        }

        if (map.SourceKind.IsImmutableList() || map.SourceKind.IsIImmutableList())
        {
            _builder.Append(
                $"            var sourceCollection = source;\r\n" +
                $"            var length = sourceCollection.Count;\r\n");
        }
    }

    private void DestinationCollection(CollectionMap map)
    {
        if (map.DestinationKind.IsArray() || map.DestinationKind.IsArrayInterface())
        {
            _builder.Append($"            var destination = new {map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}[length];\r\n");
        }

        if (map.DestinationKind.IsList() || map.DestinationKind.IsListInterface())
        {
            _builder.Append($"            var destination = new System.Collections.Generic.List<{map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}>(length);\r\n");
        }

        if (map.DestinationKind.IsImmutableArray() || map.DestinationKind.IsIImmutableList())
        {
            _builder.Append($"            var destination = System.Collections.Immutable.ImmutableArray.CreateBuilder<{map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}>(length);\r\n");
        }

        if (map.DestinationKind.IsImmutableList())
        {
            _builder.Append($"            var destination = System.Collections.Immutable.ImmutableList.CreateBuilder<{map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}>();\r\n");
        }
    }

    private void DestinationCollectionAssignment(CollectionMap map)
    {
        var nullCheck = map.IsSourceItemNullable ? "?" : "";

        if (map.DestinationKind.IsArray() || map.DestinationKind.IsArrayInterface())
        {
            _builder.Append("                destination[i] = sourceCollection[i]");

            if (!map.IsItemsEquals && !map.IsItemsHasImpicitConversion)
            {
                _builder.Append($"{nullCheck}.Map<{map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}>()");
            }
        }

        if (map.DestinationKind.IsList() || map.DestinationKind.IsListInterface() || map.DestinationKind.IsImmutableArray()
            || map.DestinationKind.IsImmutableList() || map.DestinationKind.IsIImmutableList())
        {
            _builder.Append("                destination.Add(sourceCollection[i]");

            if (!map.IsItemsEquals && !map.IsItemsHasImpicitConversion)
            {
                _builder.Append($"{nullCheck}.Map<{map.DestinationItem}{(map.IsDestinationItemNullable ? "?" : "")}>()");
            }

            _builder.Append(')');
        }

        _builder.Append(';');
    }

    private void Return(CollectionMap map)
    {
        _builder.Append("            return destination");

        if (map.DestinationKind.IsImmutableArray() || map.DestinationKind.IsIImmutableList())
        {
            _builder.Append(".MoveToImmutable()");
        }
        else if (map.DestinationKind.IsImmutableList())
        {
            _builder.Append(".ToImmutable()");
        }

        _builder.Append(";\r\n        }");
    }
}