﻿using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct ConfiguredProjectionMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<ConfiguredProjectionMap> projectionMaps)
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

    private void GenerateProjectionMapMethod(ConfiguredProjectionMap map)
    {
        MapMethodDifinition(map);
        MapMethodBody(map);
    }

    private void MapMethodDifinition(ConfiguredProjectionMap map)
    {
        _builder.Append($@"        internal static IQueryable<{map.Destination}> ProjectWith<To>
        (
            this IQueryable<{map.Source}> source");
        foreach (var argument in map.UserArguments)
        {
            _builder.Append($",\r\n            {argument.Type} {argument.Name}");
        }
        _builder.Append("\r\n        )");
    }

    private void MapMethodBody(ConfiguredProjectionMap map)
    {
        _builder.Append($"\r\n            => source.Select(x => new {map.Destination}");
        InitializerAssignments(map.InitializerProperties);
        _builder.Append(");");
    }

    private void InitializerAssignments(ImmutableArray<PropertyMap> initializerProperties)
    {
        if (initializerProperties.Length == 0)
        {
            return;
        }

        var lastIndex = initializerProperties.Length - 1;
        _builder.Append("\r\n        {\r\n");
        for (var i = 0; i < initializerProperties.Length; i++)
        {
            _builder.Append("            ");
            InitializerAssignment(initializerProperties[i]);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        }");
    }

    private void InitializerAssignment(PropertyMap propertyMap)
    {
        if (propertyMap.UserArgumentName is not null)
        {
            _builder.Append($"{propertyMap.DestinationName} = {propertyMap.UserArgumentName}");
        }
        else
        {
            _builder.Append($"{propertyMap.DestinationName} = x.{propertyMap.SourceName}");
        }
    }

    public string BuildMapperClass(ImmutableArray<ConfiguredMapMockMethod> mockMethods)
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
        foreach (var mockMethod in mockMethods)
        {
            GenerateClassMapWithMockMethod(mockMethod);
            if (counter < mockMethods.Length)
            {
                _builder.Append("\r\n\r\n");
            }
            counter++;
        }

        _builder.Append("\r\n    }\r\n}");

        return _builder.ToString();
    }

    private void GenerateClassMapWithMockMethod(ConfiguredMapMockMethod mockMethod)
    {
        _builder.Append($@"        internal static IQueryable<{mockMethod.Destination}> ProjectWith<To>
        (
            this IQueryable<{mockMethod.Source}> source");
        foreach (var parameter in mockMethod.Parameters)
        {
            _builder.Append($",\r\n            {parameter.Type} {parameter.Name} = default!");
        }
        _builder.Append("\r\n        )\r\n        {\r\n            ");
        _builder.Append("throw new System.NotImplementedException(\"This method is a mock and is not intended to be called\");\r\n        }");
    }
}