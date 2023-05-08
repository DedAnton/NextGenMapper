using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis.Targets.MapTargets;

internal readonly struct ConfiguredProjectionTarget
{
    public ConfiguredProjectionTarget(ITypeSymbol source, ITypeSymbol destination, SeparatedSyntaxList<ArgumentSyntax> arguments, bool isCompleteMethod, Location location, SemanticModel semanticModel)
    {
        Source = source;
        Destination = destination;
        Arguments = arguments;
        IsCompleteMethod = isCompleteMethod;
        Location = location;
        SemanticModel = semanticModel;
    }

    public ITypeSymbol Source { get; }
    public ITypeSymbol Destination { get; }
    public SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }
    public bool IsCompleteMethod { get; }
    public Location Location { get; }
    public SemanticModel SemanticModel { get; }

    public void Deconstruct(out ITypeSymbol source, out ITypeSymbol destination, out SeparatedSyntaxList<ArgumentSyntax> arguments, out bool isCompleteMethod, out Location location, out SemanticModel semanticModel)
    {
        source = Source;
        destination = Destination;
        arguments = Arguments;
        isCompleteMethod = IsCompleteMethod;
        location = Location;
        semanticModel = SemanticModel;
    }
}
