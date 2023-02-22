using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.MapTargets;

internal readonly struct UserMapTarget
{
    public UserMapTarget(ITypeSymbol source, ITypeSymbol destination)
    {
        Source = source;
        Destination = destination;
    }

    public ITypeSymbol Source { get; }
    public ITypeSymbol Destination { get; }
}
