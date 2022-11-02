using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    private readonly bool _isInitialized;
    private readonly Compilation _compilation;
    private ValueStringBuilder _builder;

    public MapperSourceBuilder(Compilation compilation)
    {
        _compilation = compilation;
        _builder = new ValueStringBuilder();
        _isInitialized = true;
    }

    public string BuildMapperClass(IReadOnlyCollection<TypeMap> maps)
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
        foreach(var map in maps)
        {
            MappingMethod(map);
            if (counter < maps.Count)
            {
                _builder.Append("\r\n\r\n");
            }
            counter++;
        }

        _builder.Append("\r\n    }\r\n}");

        return _builder.ToString();
    }

    private void MappingMethod(TypeMap map)
    {
        if (map is ClassMapWithStub classMapWithStub)
        {
            GenerateClassMapWithStubMethod(classMapWithStub);
        }
        else if (map is ClassMapWith classMapWith)
        {
            if (classMapWith.Arguments.Length > 0)
            {
                GenerateClassMapWithMethod(classMapWith);
            }
        }
        else if (map is ClassMap classMap)
        {
            GenerateClassMapMethod(classMap);
        }
        else if (map is EnumMap enumMap)
        {
            GenerateEnumMapMethod(enumMap);
        }
        else if (map is CollectionMap collectionMap)
        {
            GenerateCollectionMapMethod(collectionMap);
        }
    }

    public override string ToString() => _builder.ToString();
    public ReadOnlySpan<char> AsSpan() => _builder.AsSpan();
}
