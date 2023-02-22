using Microsoft.CodeAnalysis;
using System;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct PotentialErrorMap : IMap, IEquatable<PotentialErrorMap>
{
    public PotentialErrorMap(string source, string destination, Diagnostic diagnostic)
    {
        Source = source;
        Destination = destination;
        Diagnostic = diagnostic;
    }

    public string Source { get; }
    public string Destination { get; }
    public Diagnostic Diagnostic { get; }

    public bool Equals(PotentialErrorMap other)
        => Source == other.Source
        && Destination == other.Destination
        && Diagnostic == other.Diagnostic;
}
