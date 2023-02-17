﻿using Microsoft.CodeAnalysis;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct PotentialErrorMap
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
}
