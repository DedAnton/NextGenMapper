using NextGenMapper.CodeAnalysis.Maps;
using System;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    public void GenerateClassMapWithStubMethod(ClassMapWithStub map)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(MapperSourceBuilder)} was not initialized");
        }

        _builder.Append(
$@"        internal static {map.To} MapWith<To>
        (
            this {map.From} source,
");
        var lastIndex = map.Parameters.Length - 1;
        for (var i = 0; i < map.Parameters.Length; i++)
        {
            _builder.Append($"            {map.Parameters[i].Type} {map.Parameters[i].Name} = defatult!");
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        )\r\n        {\r\n            ");
        _builder.Append("throw new System.NotImplementedException(\"This method is a stub and is not intended to be called\");\r\n        }");
    }
}
