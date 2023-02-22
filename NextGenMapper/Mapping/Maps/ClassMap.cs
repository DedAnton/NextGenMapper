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
        ImmutableArray<PropertyMap> initializerProperties)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }

    public bool Equals(ClassMap other) => Source == other.Source && Destination == other.Destination;
}
