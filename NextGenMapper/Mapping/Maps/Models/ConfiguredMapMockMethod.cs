using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps.Models;

internal readonly struct ConfiguredMapMockMethod
{
    public ConfiguredMapMockMethod(string source, string destination, ImmutableArray<NameTypePair> parameters)
    {
        Source = source;
        Destination = destination;
        Parameters = parameters;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<NameTypePair> Parameters { get; }
}
