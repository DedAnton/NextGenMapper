using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct ProjectionMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<ProjectionMap> projectionMaps)
    {
        _builder.Append(
@"#nullable enable
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
");
        var counter = 1;
        foreach (var map in projectionMaps)
        {
            GenerateProjectionMapMethod(map);
            if (counter < projectionMaps.Length)
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

    private void GenerateProjectionMapMethod(ProjectionMap map)
    {
        MapMethodDifinition(map.Source, map.Destination);
        MapMethodBody(map);
    }

    private void MapMethodDifinition(string from, string to)
    {
        _builder.Append($"        internal static IQueryable<{to}> Project<To>(this IQueryable<{from}> source)");
    }

    private void MapMethodBody(ProjectionMap map)
    {
        _builder.Append($"\r\n            => source.Select(x => new {map.Destination}");
        InitializerAssignments(map.Properties);
        _builder.Append(");");
    }

    private void InitializerAssignments(ImmutableArray<PropertyMap> initializerProperties)
    {
        if (initializerProperties.Length == 0)
        {
            return;
        }

        var lastIndex = initializerProperties.Length - 1;
        _builder.Append("\r\n            {\r\n");
        for (var i = 0; i < initializerProperties.Length; i++)
        {
            _builder.Append($"                {initializerProperties[i].DestinationName} = x.{initializerProperties[i].SourceName}");
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n            }");
    }
}