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

        var (isMapMethodInvocation, sourceType, destinationType) = 
            SourceCodeAnalyzer.AnalyzeMapMethod(mapMethodInvocation, semanticModel, cancellationToken);
        if (!isMapMethodInvocation || sourceType is null || destinationType is null)
        {
            return Target.Empty;
        }

        var location = mapMethodInvocation.GetLocation();

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

        var (isConfiguredMapMethodInvocation, sourceType, destinationType, isCompleteMethod)
            = SourceCodeAnalyzer.AnalyzeConfiguredMapMethod(configuredMapMethodInvocation, semanticModel, cancellationToken);
        if (!isConfiguredMapMethodInvocation || sourceType is null || destinationType is null)
        {
            return Target.Empty;
        }

        var location = configuredMapMethodInvocation.GetLocation();

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

        var (isUserMapMethodDeclaration, sourceType, destinationType, method)
            = SourceCodeAnalyzer.AnalyzeUserMapMethod(userMapMethodDeclaration, semanticModel, cancellationToken);
        if (!isUserMapMethodDeclaration || sourceType is null || destinationType is null || method is null)
        {
            return ImmutableArray.Create(Target.Empty);
        }

        var location = userMapMethodDeclaration.GetLocation();

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

    public static Target GetProjectionTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax mapMethodInvocation)
        {
            return Target.Empty;
        }

        var location = mapMethodInvocation.GetLocation();

        var (isMapMethodInvocation, sourceType, destinationType) =
            SourceCodeAnalyzer.AnalyzeProjectionMethod(mapMethodInvocation, semanticModel, cancellationToken);
        if (!isMapMethodInvocation || sourceType is null || destinationType is null)
        {
            if (SourceCodeAnalyzer.IsProjectionWithNonGenericIQueryable(mapMethodInvocation, semanticModel, cancellationToken))
            {
                var diagnostic = Diagnostics.NonGenericIQueryableError(location);

                return Target.Error(diagnostic);
            }

            return Target.Empty;
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.ProjectedTypesEquals(location);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceType, destinationType, semanticModel))
        {
            var diagnostic = Diagnostics.ProjectedTypesHasImplicitConversion(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreClasses(sourceType, destinationType))
        {
            return Target.ProjectionMap(sourceType, destinationType, location);
        }

        var notSupportedDiagnostic = Diagnostics.ProjectionNotSupported(location, sourceType, destinationType);
        return Target.Error(notSupportedDiagnostic);
    }

    public static Target GetConfiguredProjectionTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax configuredMapMethodInvocation)
        {
            return Target.Empty;
        }

        var location = configuredMapMethodInvocation.GetLocation();

        var (isConfiguredMapMethodInvocation, sourceType, destinationType, isCompleteMethod)
            = SourceCodeAnalyzer.AnalyzeConfiguredProjectionMethod(configuredMapMethodInvocation, semanticModel, cancellationToken);
        if (!isConfiguredMapMethodInvocation || sourceType is null || destinationType is null)
        {
            if (SourceCodeAnalyzer.IsProjectionWithNonGenericIQueryable(configuredMapMethodInvocation, semanticModel, cancellationToken))
            {
                var diagnostic = Diagnostics.NonGenericIQueryableError(location);

                return Target.Error(diagnostic);
            }

            return Target.Empty;
        }

        if (!SourceCodeAnalyzer.IsTypesAreClasses(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.ConfiguredProjectionNotSupported(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(sourceType, destinationType))
        {
            var diagnostic = Diagnostics.ProjectedTypesEquals(location);

            return Target.Error(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceType, destinationType, semanticModel))
        {
            var diagnostic = Diagnostics.ProjectedTypesHasImplicitConversion(location, sourceType, destinationType);

            return Target.Error(diagnostic);
        }

        var arguments = configuredMapMethodInvocation.ArgumentList.Arguments;
        return Target.ConfiguredProjectionMap(sourceType, destinationType, arguments, isCompleteMethod, location, semanticModel);
    }
}
