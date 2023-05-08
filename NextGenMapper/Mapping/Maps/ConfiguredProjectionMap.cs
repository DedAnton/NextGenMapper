using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ConfiguredProjectionMap : IMap, IEquatable<ConfiguredMap>
{
    public ConfiguredProjectionMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> initializerProperties,
        ImmutableArray<NameTypePair> userArguments,
        ConfiguredMapMockMethod? mockMethod,
        bool isSuccess)
    {
        Source = source;
        Destination = destination; 
        InitializerProperties = initializerProperties;
        UserArguments = userArguments;
        MockMethod = mockMethod;
        IsSuccess = isSuccess;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public ImmutableArray<NameTypePair> UserArguments { get; }
    public ConfiguredMapMockMethod? MockMethod { get; }
    public bool IsSuccess { get; }

    public bool Equals(ConfiguredMap other)
    {
        if (Source != other.Source
        || Destination != other.Destination
        || UserArguments.Length != other.UserArguments.Length)
        {
            return false;
        }

        for (var i = 0; i < UserArguments.Length; i++)
        {
            if (UserArguments[i].Type != other.UserArguments[i].Type)
            {
                return false;
            }
        }

        return true;
    }
}