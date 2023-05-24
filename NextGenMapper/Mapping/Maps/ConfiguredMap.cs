using Microsoft.CodeAnalysis;
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
        bool isSuccess,
        Location location)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
        UserArguments = userArguments;
        MockMethods = mockMethods;
        IsSuccess = isSuccess;
        Location = location;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public ImmutableArray<NameTypePair> UserArguments { get; }
    public ImmutableArray<ConfiguredMapMockMethod> MockMethods { get; }
    public bool IsSuccess { get; }
    public Location Location { get; }

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

    public bool EqualsWithArgumentsNames(ConfiguredMap other)
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