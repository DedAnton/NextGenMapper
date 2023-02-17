using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;

namespace NextGenMapper.CodeAnalysis.Targets;

internal readonly struct Target
{
    public MapTarget MapTarget { get; }
    public ConfiguredMapTarget ConfiguredMapTarget { get; }
    public UserMapTarget UserMapTarget { get; }
    public ErrorMapTarget ErrorMapTarget { get; }
    public TargetType Type { get; }

    private Target(
        TargetType type,
        MapTarget classMapTarget = default, 
        ConfiguredMapTarget configuredMapTarget = default, 
        UserMapTarget userMapTarget = default,
        ErrorMapTarget errorMapTarget = default)
    {
        MapTarget = classMapTarget;
        ConfiguredMapTarget = configuredMapTarget;
        UserMapTarget = userMapTarget;
        ErrorMapTarget = errorMapTarget;
        Type = type;
    }

    public static Target Map(ITypeSymbol source, ITypeSymbol destination, Location location, SemanticModel semanticModel)
        => new(TargetType.Map, classMapTarget: new MapTarget(source, destination, location, semanticModel));

    public static Target ConfiguredMap(
        ITypeSymbol source, 
        ITypeSymbol destination, 
        SeparatedSyntaxList<ArgumentSyntax> arguments, 
        bool isCompleteMethod, 
        Location location, 
        SemanticModel semanticModel) 
        => new(
            TargetType.ConfiguredMap, 
            configuredMapTarget: new ConfiguredMapTarget(source, destination, arguments, isCompleteMethod, location, semanticModel));

    public static Target UserMap(ITypeSymbol source, ITypeSymbol destination) 
        => new(TargetType.UserMap, userMapTarget: new UserMapTarget(source, destination));

    public static Target Error(Diagnostic diagnostic) => new(TargetType.Error, errorMapTarget: new ErrorMapTarget(diagnostic));

    public static Target Empty = new(TargetType.Empty);
}