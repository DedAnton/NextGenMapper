using NextGenMapper.CodeAnalysis.Maps;
using System;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    public void GenerateEnumMapMethod(EnumMap map)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(MapperSourceBuilder)} was not initialized");
        }
        MapMethodDifinition(map.From, map.To);
        EnumSwitch(map);
    }

    private void EnumSwitch(EnumMap map)
    {
        //TODO: Remove ToArray()
        var enumFields = map.Fields.ToArray();

        _builder.Append(" => source switch\r\n        {\r\n");
        foreach (var field in enumFields)
        {
            _builder.Append("            ");
            EnumField(field);
        }
        _builder.Append("            _ => throw new System.ArgumentOutOfRangeException(nameof(source), \"Error when mapping Test.Source to Test.Destination\")\r\n        };");
    }

    private void EnumField(MemberMap field)
    {
        _builder.Append($"{field.FromType}.{field.FromName} => {field.ToType}.{field.ToName},\r\n");
    }
}
