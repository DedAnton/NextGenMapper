using Microsoft.CodeAnalysis;
using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ConfiguredProjectionMap : IMap, IEquatable<ConfiguredProjectionMap>
{
    public ConfiguredProjectionMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> initializerProperties,
        ImmutableArray<NameTypePair> userArguments,
        ConfiguredMapMockMethod? mockMethod,
        bool isSuccess,
        Location location)
    {
        Source = source;
        Destination = destination; 
        InitializerProperties = initializerProperties;
        UserArguments = userArguments;
        MockMethod = mockMethod;
        IsSuccess = isSuccess;
        Location = location;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public ImmutableArray<NameTypePair> UserArguments { get; }
    public ConfiguredMapMockMethod? MockMethod { get; }
    public bool IsSuccess { get; }
    public Location Location { get; }

    public bool Equals(ConfiguredProjectionMap other)
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

    public bool EqualsWithArgumentsNames(ConfiguredProjectionMap other)
    {
        if (Source != other.Source
        || Destination != other.Destination
        || UserArguments.Length != other.UserArguments.Length)
        {
            return false;
        }

        for (var i = 0; i < UserArguments.Length; i++)
        {
            if (UserArguments[i].Type != other.UserArguments[i].Type
                || UserArguments[i].Name != other.UserArguments[i].Name)
            {
                return false;
            }
        }

        return true;
    }
}