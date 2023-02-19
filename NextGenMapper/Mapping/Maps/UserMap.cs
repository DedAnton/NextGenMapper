using System;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct UserMap : IMap, IEquatable<UserMap>
{
    public UserMap(
        string source,
        string destination)
    {
        Source = source;
        Destination = destination;
    }

    public string Source { get; }
    public string Destination { get; }

    public bool Equals(UserMap other) => Source == other.Source && Destination == other.Destination;
}