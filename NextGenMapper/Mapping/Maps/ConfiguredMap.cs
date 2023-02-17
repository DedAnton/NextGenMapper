using System.Collections.Immutable;
using NextGenMapper.Mapping.Maps.Models;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ConfiguredMap : IMap
{
    public ConfiguredMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties,
        ImmutableArray<NameTypePair> arguments,
        ImmutableArray<ConfiguredMapMockMethod> mockMethods)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
        UserArguments = arguments;
        MockMethods = mockMethods;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public ImmutableArray<NameTypePair> UserArguments { get; }
    public ImmutableArray<ConfiguredMapMockMethod> MockMethods { get; }
}

internal readonly struct NameTypePair
{
    public NameTypePair(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public string Type { get; }
}
