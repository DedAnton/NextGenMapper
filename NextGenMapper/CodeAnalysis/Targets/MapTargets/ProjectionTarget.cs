using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.MapTargets;

internal readonly struct ProjectionTarget
{
    public ProjectionTarget(ITypeSymbol source, ITypeSymbol destination, Location location)
    {
        Source = source;
        Destination = destination;
        Location = location;
    }

    public ITypeSymbol Source { get; }
    public ITypeSymbol Destination { get; }
    public Location Location { get; }

    public void Deconstruct(out ITypeSymbol source, out ITypeSymbol destination, out Location location)
    {
        source = Source;
        destination = Destination;
        location = Location;
    }
}
