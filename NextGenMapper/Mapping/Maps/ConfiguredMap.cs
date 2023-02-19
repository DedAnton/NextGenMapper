using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ConfiguredMap : IMap, IEquatable<ConfiguredMap>
{
    public ConfiguredMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties,
        ImmutableArray<NameTypePair> userArguments,
        ImmutableArray<ConfiguredMapMockMethod> mockMethods,
        bool isSuccess)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
        UserArguments = userArguments;
        MockMethods = mockMethods;
        IsSuccess = isSuccess;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public ImmutableArray<NameTypePair> UserArguments { get; }
    public ImmutableArray<ConfiguredMapMockMethod> MockMethods { get; }
    public bool IsSuccess { get; }

    public bool Equals(ConfiguredMap other)
    {
        if (Source != other.Source
        || Destination != other.Destination
        || UserArguments.Length != other.UserArguments.Length)
        {
            return false;
        }

        for(var i = 0; i < UserArguments.Length;i++)
        {
            if (UserArguments[i].Type != other.UserArguments[i].Type)
            {
                return false;
            }
        }

        return true;
    }
}