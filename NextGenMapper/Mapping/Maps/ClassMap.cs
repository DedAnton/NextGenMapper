using Microsoft.CodeAnalysis;
using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ClassMap : IMap, IEquatable<ClassMap>
{
    public ClassMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties,
        Location location)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
        Location = location;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }
    public Location Location { get; }

    public bool Equals(ClassMap other) => Source == other.Source && Destination == other.Destination;
}
