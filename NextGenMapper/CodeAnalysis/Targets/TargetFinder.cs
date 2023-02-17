using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis.Targets;

internal class MapMethodAnalysisResult
{
    public static MapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType) => new(true, sourceType, destinationType);
    public static MapMethodAnalysisResult Fail() => new(false, null, null);

    private MapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
    }
}

internal class ConfiguredMapMethodAnalysisResult
{
    public static ConfiguredMapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType, bool isCompleteMethod) => new(true, sourceType, destinationType, isCompleteMethod);
    public static ConfiguredMapMethodAnalysisResult Fail() => new(false, null, null, false);

    private ConfiguredMapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType, bool isCompleteMethod)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
        IsCompleteMethod = isCompleteMethod;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }
    public bool IsCompleteMethod { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType, out bool isCompleteMethod)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
        isCompleteMethod = IsCompleteMethod;
    }
}

internal class UserMapMethodAnalysisResult
{
    public static UserMapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType, IMethodSymbol method)
        => new(true, sourceType, destinationType, method);
    public static UserMapMethodAnalysisResult Fail() => new(false, null, null, null);

    private UserMapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType, IMethodSymbol? method)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
        Method = method;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }
    public IMethodSymbol? Method { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType, out IMethodSymbol? method)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
        method = Method;
    }
}

internal static class TargetFinder
{
    public static Target GetMapTarget(SyntaxNode node, SemanticModel semanticModel)
    {
        if (node is not InvocationExpressionSyntax mapMethodInvocation)
        {
            return Target.Empty;
        }

        var (isMapMethodInvocation, sourceType, destinationType) = SourceCodeAnalyzer.AnalyzeMapMethod(mapMethodInvocation, semanticModel);
        if (!isMapMethodInvocation || sourceType is null || destinationType is null)
        {
            return Target.Empty;
        }

        var location = mapMethodInvocation.GetLocation();

        if (SourceCodeAnalyzer.IsTypesHasErrors(sourceType, destinationType))
        {
            return Target.Empty;
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.MappedTypesEquals(location);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceType, destinationType, semanticModel))
        {
            var diagnostic = Diagnostics.MappedTypesHasImplicitConversion(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreClasses(sourceType, destinationType)
            || SourceCodeAnalyzer.IsTypesAreEnums(sourceType, destinationType)
            || SourceCodeAnalyzer.IsTypesAreCollections(sourceType, destinationType))
        {
            return Target.Map(sourceType, destinationType, location, semanticModel);
        }

        var notSupportedDiagnostic = Diagnostics.MappingNotSupported(location, sourceType, destinationType);
        return Target.Error(notSupportedDiagnostic);
    }

    public static Target GetConfiguredMapTarget(SyntaxNode node, SemanticModel semanticModel)
    {
        if (node is not InvocationExpressionSyntax configuredMapMethodInvocation)
        {
            return Target.Empty;
        }

        var (isConfiguredMapMethodInvocation, sourceType, destinationType, isCompleteMethod)
            = SourceCodeAnalyzer.AnalyzeConfiguredMapMethod(configuredMapMethodInvocation, semanticModel);
        if (!isConfiguredMapMethodInvocation || sourceType is null || destinationType is null)
        {
            return Target.Empty;
        }

        var location = configuredMapMethodInvocation.GetLocation();

        if (SourceCodeAnalyzer.IsTypesHasErrors(sourceType, destinationType))
        {
            return Target.Empty;
        }

        if (!SourceCodeAnalyzer.IsTypesAreClasses(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.MapWithNotSupportedForMapWith(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.MappedTypesEquals(location);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceType, destinationType, semanticModel))
        {
            var diagnostic = Diagnostics.MappedTypesHasImplicitConversion(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        var arguments = configuredMapMethodInvocation.ArgumentList.Arguments;
        return Target.ConfiguredMap(sourceType, destinationType, arguments, isCompleteMethod, location, semanticModel);
    }

    public static Target GetUserMapTarget(SyntaxNode node, SemanticModel semanticModel)
    {
        if (node is not MethodDeclarationSyntax userMapMethodDeclaration)
        {
            return Target.Empty;
        }

        var (isUserMapMethodDeclaration, sourceType, destinationType, method)
            = SourceCodeAnalyzer.AnalyzeUserMapMethod(userMapMethodDeclaration, semanticModel);
        if (!isUserMapMethodDeclaration || sourceType is null || destinationType is null || method is null)
        {
            return Target.Empty;
        }

        var location = userMapMethodDeclaration.GetLocation();

        if (SourceCodeAnalyzer.IsTypesHasErrors(sourceType, destinationType))
        {
            return Target.Empty;
        }

        if (!method.IsExtensionMethod)
        {
            var diagnostic = Diagnostics.MapMethodMustBeExtension(location);

            return Target.Error(diagnostic);
        }

        if (!method.IsGenericMethod || method.TypeParameters.Length != 1)
        {
            var diagnostic = Diagnostics.MapMethodMustBeGeneric(location);

            return Target.Error(diagnostic);
        }

        if (!method.IsExtensionMethod)
        {
            var diagnostic = Diagnostics.MapMethodMustNotReturnVoid(location);

            return Target.Error(diagnostic);
        }

        return Target.UserMap(sourceType, destinationType);
    }
}
