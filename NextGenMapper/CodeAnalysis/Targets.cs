using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;

internal abstract record Target()
{
    public static EmptyTarget Empty = new();
}

internal sealed record ErrorTarget(Diagnostic Error) : Target();

internal sealed record EmptyTarget() : Target();

internal sealed record UserMapTarget(ITypeSymbol Source, ITypeSymbol Destination, Location Location) : Target();

internal enum MapTargetKind
{
    Undefined,
    Class,
    Collection,
    Enum
}
internal sealed record MapTarget(ITypeSymbol Source, ITypeSymbol Destination, MapTargetKind Kind, Location Location, SemanticModel SemanticModel) : Target();

internal sealed record ConfiguredMapTarget(
    ITypeSymbol Source,
    ITypeSymbol Destination,
    Location Location,
    SemanticModel SemanticModel,
    SeparatedSyntaxList<ArgumentSyntax> Arguments,
    bool IsSuccessOverloadResolution)
    : Target()
{
    public ConfiguredMapTarget(ITypeSymbol source, ITypeSymbol destination, MapMethodInfo methodInfo)
        : this(source, destination, methodInfo.Location, methodInfo.SemanticModel, methodInfo.InvocationSyntax.ArgumentList.Arguments, methodInfo.IsSuccessOverloadResolution)
    {

    }
}

internal sealed record ProjectionTarget(ITypeSymbol Source, ITypeSymbol Destination, Location Location) : Target();

internal sealed record ConfiguredProjectionTarget(
    ITypeSymbol Source,
    ITypeSymbol Destination,
    Location Location,
    SeparatedSyntaxList<ArgumentSyntax> Arguments,
    bool IsSuccessOverloadResolution)
    : Target()
{
    public ConfiguredProjectionTarget(ITypeSymbol source, ITypeSymbol destination, MapMethodInfo methodInfo)
        : this(source, destination, methodInfo.Location, methodInfo.InvocationSyntax.ArgumentList.Arguments, methodInfo.IsSuccessOverloadResolution)
    {

    }
}
