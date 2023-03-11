using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ProjectionMap : IMap, IEquatable<ProjectionMap>
{
    public ProjectionMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> properties)
    {
        Source = source;
        Destination = destination;
        Properties = properties;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> Properties { get; }

    public bool Equals(ProjectionMap other) => Source == other.Source && Destination == other.Destination;
}
