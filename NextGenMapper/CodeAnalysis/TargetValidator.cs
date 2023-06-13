using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis;

public record TargetValidationResult(
    bool IsTypesAreEquals, 
    bool IsTypesHasImplicitConversion, 
    bool IsTypesAreClasses, 
    bool? IsTypesAreCollections,
    bool? IsTypesAreEnums);

internal class TargetValidator
{
    private static TargetValidationResult Validate(ITypeSymbol source, ITypeSymbol destination, SemanticModel semanticModel) => new(
        SourceCodeAnalyzer.IsTypesAreEquals(source, destination),
        SourceCodeAnalyzer.IsTypesHasImplicitConversion(source, destination, semanticModel),
        SourceCodeAnalyzer.IsTypesAreClasses(source, destination),
        SourceCodeAnalyzer.IsTypesAreCollections(source, destination),
        SourceCodeAnalyzer.IsTypesAreEnums(source, destination));

    public static TargetValidationResult Validate(MapTarget target, SemanticModel semanticModel)
        => Validate(target.Source, target.Destination, semanticModel);

    public static TargetValidationResult Validate(ConfiguredMapTarget target, SemanticModel semanticModel)
        => Validate(target.Source, target.Destination, semanticModel);

    public static TargetValidationResult Validate(ProjectionTarget target, SemanticModel semanticModel)
        => Validate(target.Source, target.Destination, semanticModel);

    public static TargetValidationResult Validate(ConfiguredProjectionTarget target, SemanticModel semanticModel)
        => Validate(target.Source, target.Destination, semanticModel);
}
