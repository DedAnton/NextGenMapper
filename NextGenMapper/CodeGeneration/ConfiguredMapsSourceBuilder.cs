using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct ConfiguredMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<ConfiguredMap> configuredMaps)
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
        foreach (var map in configuredMaps)
        {
            GenerateClassMapWithMethod(map);
            if (counter < configuredMaps.Length)
            {
                _builder.Append("\r\n\r\n");
            }
            counter++;
        }

        _builder.Append("\r\n    }\r\n}");

        return _builder.ToString();
    }

    public string BuildMapperClass(ImmutableArray<ConfiguredMapMockMethod> mockMethods)
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

    public override string ToString() => _builder.ToString();
    public ReadOnlySpan<char> AsSpan() => _builder.AsSpan();

    private void GenerateClassMapWithMethod(ConfiguredMap map)
    {
        MethodDifinition(map);
        ConstructorInvocation(map);
    }

    private void MethodDifinition(ConfiguredMap map)
    {
        _builder.Append(
$@"        internal static {map.Destination} MapWith<To>
        (
            this {map.Source} source");
        foreach (var argument in map.UserArguments)
        {
            _builder.Append($",\r\n            {argument.Type} {argument.Name}");
        }
        //for (var i = 0; i < map.UserArguments.Length; i++)
        //{
        //    _builder.Append($",\r\n            {map.UserArguments[i].Type} {map.UserArguments[i].Name}");
        //}
        _builder.Append("\r\n        )");
    }

    private void ConstructorInvocation(ConfiguredMap map)
    {
        _builder.Append($"\r\n        => new {map.Destination}");
        ConstructorArguments(map.ConstructorProperties);
        InitializerAssignments(map.InitializerProperties);
        _builder.Append(';');
    }

    private void ConstructorArguments(ImmutableArray<PropertyMap> constructorProperties)
    {
        if (constructorProperties.Length == 0)
        {
            return;
        }

        var lastIndex = constructorProperties.Length - 1;
        _builder.Append("\r\n        (\r\n");
        for (var i = 0; i < constructorProperties.Length; i++)
        {
            _builder.Append("            ");
            ConstructorArgument(constructorProperties[i]);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        )");
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

    private void ConstructorArgument(PropertyMap propertyMap)
    {
        if (propertyMap.UserArgumentName is not null)
        {
            _builder.Append(propertyMap.UserArgumentName);
        }
        else
        {
            if (propertyMap.IsTypesEquals || propertyMap.HasImplicitConversion)
            {
                _builder.Append($"source.{propertyMap.SourceName}");
            }
            else
            {
                var nullCheck = propertyMap.IsSourceNullable ? "?" : "";
                _builder.Append($"source.{propertyMap.SourceName}{nullCheck}.Map<{propertyMap.DestinationType}>()");
            }
        }
    }

    private void InitializerAssignment(PropertyMap propertyMap)
    {
        if (propertyMap.UserArgumentName is not null)
        {
            _builder.Append($"{propertyMap.DestinationName} = {propertyMap.UserArgumentName}");
        }
        else
        {
            if (propertyMap.IsTypesEquals || propertyMap.HasImplicitConversion)
            {
                _builder.Append($"{propertyMap.DestinationName} = source.{propertyMap.SourceName}");
            }
            else
            {
                var nullCheck = propertyMap.IsSourceNullable ? "?" : "";
                _builder.Append($"{propertyMap.DestinationName} = source.{propertyMap.SourceName}{nullCheck}.Map<{propertyMap.DestinationType}>()");
            }
        }
    }

    private void GenerateClassMapWithMockMethod(ConfiguredMapMockMethod mockMethod)
    {
        _builder.Append(
$@"        internal static {mockMethod.Destination} MapWith<To>
        (
            this {mockMethod.Source} source");
        var lastIndex = mockMethod.Parameters.Length - 1;
        for (var i = 0; i < mockMethod.Parameters.Length; i++)
        {
            _builder.Append($",\r\n            {mockMethod.Parameters[i].Type} {mockMethod.Parameters[i].Name} = default!");
        }
        _builder.Append("\r\n        )\r\n        {\r\n            ");
        _builder.Append("throw new System.NotImplementedException(\"This method is a mock and is not intended to be called\");\r\n        }");
    }
}
