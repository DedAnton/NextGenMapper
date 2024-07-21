using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct EnumMap : IMap, IEquatable<EnumMap>
{
    public EnumMap(string source, string destination, ImmutableArray<EnumFieldMap> fields)
    {
        Source = source;
        Destination = destination;
        Fields = fields;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<EnumFieldMap> Fields { get; }

    public bool Equals(EnumMap other) => Source == other.Source && Destination == other.Destination;
}
