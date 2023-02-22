using NextGenMapper.Mapping.Maps;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.CodeGeneration;

internal ref struct EnumMapsSourceBuilder
{
    private ValueStringBuilder _builder;

    public string BuildMapperClass(ImmutableArray<EnumMap> enumMaps)
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
        foreach (var map in enumMaps)
        {
            GenerateEnumMapMethod(map);
            if (counter < enumMaps.Length)
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

    private void GenerateEnumMapMethod(EnumMap map)
    {
        MapMethodDifinition(map.Source, map.Destination);
        EnumSwitch(map);
    }

    private void EnumSwitch(EnumMap map)
    {
        _builder.Append(" => source switch\r\n        {\r\n");
        foreach (var field in map.Fields.AsSpan())
        {
            _builder.Append("            ");
            _builder.Append($"{map.Source}.{field.Source} => {map.Destination}.{field.Destination},\r\n");
        }
        _builder.Append("            _ => throw new System.ArgumentOutOfRangeException(nameof(source), \"Error when mapping Test.Source to Test.Destination\")\r\n        };");
    }
}
