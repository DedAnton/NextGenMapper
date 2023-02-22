using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.CodeAnalysis.Targets;

internal static class TargetFinder
{
    public static Target GetMapTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax mapMethodInvocation)
        {
            return Target.Empty;
        }

        cancellationToken.ThrowIfCancellationRequested();
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

    public static Target GetConfiguredMapTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax configuredMapMethodInvocation)
        {
            return Target.Empty;
        }

        cancellationToken.ThrowIfCancellationRequested();
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

    public static ImmutableArray<Target> GetUserMapTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not MethodDeclarationSyntax userMapMethodDeclaration)
        {
            return ImmutableArray.Create(Target.Empty);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var (isUserMapMethodDeclaration, sourceType, destinationType, method)
            = SourceCodeAnalyzer.AnalyzeUserMapMethod(userMapMethodDeclaration, semanticModel);
        if (!isUserMapMethodDeclaration || sourceType is null || destinationType is null || method is null)
        {
            return ImmutableArray.Create(Target.Empty);
        }

        var location = userMapMethodDeclaration.GetLocation();

        if (SourceCodeAnalyzer.IsTypesHasErrors(sourceType, destinationType))
        {
            return ImmutableArray.Create(Target.Empty);
        }

        if (!method.IsGenericMethod || method.TypeParameters.Length != 1)
        {
            var diagnostic = Diagnostics.MapMethodMustBeGeneric(location);

            return ImmutableArray.Create(Target.Error(diagnostic));
        }

        var userMapTarget = Target.UserMap(sourceType, destinationType);

        if (!method.IsExtensionMethod)
        {
            var diagnostic = Diagnostics.MapMethodMustBeExtension(location);

            return ImmutableArray.Create(Target.Error(diagnostic), userMapTarget);
        }

        if (method.ReturnsVoid)
        {
            var diagnostic = Diagnostics.MapMethodMustNotReturnVoid(location);

            return ImmutableArray.Create(Target.Error(diagnostic), userMapTarget);
        }

        if (method.DeclaredAccessibility != Accessibility.Internal)
        {
            var diagnostic = Diagnostics.MapMethodMustBeInternal(location);

            return ImmutableArray.Create(Target.Error(diagnostic), userMapTarget);
        }

        return ImmutableArray.Create(userMapTarget);
    }
}
