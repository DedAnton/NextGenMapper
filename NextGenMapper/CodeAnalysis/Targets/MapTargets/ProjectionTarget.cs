using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.MapTargets;

internal readonly struct ProjectionTarget
{
    public ProjectionTarget(ITypeSymbol source, ITypeSymbol destination, Location location, SemanticModel semanticModel)
    {
        Source = source;
        Destination = destination;
        Location = location;
        SemanticModel = semanticModel;
    }

    public ITypeSymbol Source { get; }
    public ITypeSymbol Destination { get; }
    public Location Location { get; }
    public SemanticModel SemanticModel { get; }

    public void Deconstruct(out ITypeSymbol source, out ITypeSymbol destination, out Location location, out SemanticModel semanticModel)
    {
        source = Source;
        destination = Destination;
        location = Location;
        semanticModel = SemanticModel;
    }
}
