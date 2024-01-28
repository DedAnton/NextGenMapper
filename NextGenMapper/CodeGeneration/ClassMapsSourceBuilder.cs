using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct ClassMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<ClassMap> classMaps)
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
        foreach (var map in classMaps)
        {
            GenerateClassMapMethod(map);
            if (counter < classMaps.Length)
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

    private void GenerateClassMapMethod(ClassMap map)
    {
        MapMethodDifinition(map.Source, map.Destination);
        ConstructorInvocation(map);
    }

    private void ConstructorInvocation(ClassMap map)
    {
        _builder.Append($" => new {map.Destination}");
        ConstructorArguments(map.ConstructorProperties);
        InitializerAssignments(map.InitializerProperties);
        _builder.Append(';');
    }

    private void ConstructorArguments(ImmutableArray<PropertyMap> constructorProperties)
    {
        if (constructorProperties.Length == 0)
        {
            _builder.Append("()");
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

    private void ConstructorArgument(PropertyMap map)
    {
        if (map.IsTypesEquals || map.HasImplicitConversion)
        {
            _builder.Append($"source.{map.SourceName}");
        }
        else
        {
            var nullCheck = map.IsSourceNullable ? "?" : "";
            _builder.Append($"source.{map.SourceName}{nullCheck}.Map<{map.DestinationType}>()");
        }
    }

    private void InitializerAssignment(PropertyMap map)
    {
        if (map.IsTypesEquals || map.HasImplicitConversion)
        {
            _builder.Append($"{map.DestinationName} = source.{map.SourceName}");
        }
        else
        {
            var nullCheck = map.IsSourceNullable ? "?" : "";
            _builder.Append($"{map.DestinationName} = source.{map.SourceName}{nullCheck}.Map<{map.DestinationType}>()");
        }
    }
}
