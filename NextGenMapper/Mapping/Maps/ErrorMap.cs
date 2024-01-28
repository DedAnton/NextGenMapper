using Microsoft.CodeAnalysis;
using System;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct ErrorMap : IEquatable<ErrorMap>
{
    public ErrorMap(string source, string destination, Diagnostic diagnostic)
    {
        Source = source;
        Destination = destination;
        Diagnostic = diagnostic;
    }

    public string Source { get; }
    public string Destination { get; }
    public Diagnostic Diagnostic { get; }

    public bool Equals(ErrorMap other)
        => Source == other.Source
        && Destination == other.Destination
        && Diagnostic == other.Diagnostic;
}
