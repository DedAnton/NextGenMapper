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

        var analyzeResult = SourceCodeAnalyzer.AnalyzeMapMethod(mapMethodInvocation, semanticModel, cancellationToken);
        if (!analyzeResult.IsSuccess(out var successResult, out var failureResult))
        {
            if (failureResult.Error is not null)
            {
                return new ErrorTarget(failureResult.Error);
            }

            return Target.Empty;
        }
        var (source, destination, _, _, location) = successResult;

        if (SourceCodeAnalyzer.IsTypesAreEquals(source, destination))
        {
            var diagnostic = Diagnostics.MappedTypesEquals(location);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(source, destination, semanticModel))
        {
            var diagnostic = Diagnostics.MappedTypesHasImplicitConversion(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreClasses(source, destination)
            || SourceCodeAnalyzer.IsTypesAreEnums(source, destination)
            || SourceCodeAnalyzer.IsTypesAreCollections(source, destination))
        {
            return new MapTarget(source, destination, location, semanticModel);
        }

        var notSupportedDiagnostic = Diagnostics.MappingNotSupported(location, source, destination);
        return new ErrorTarget(notSupportedDiagnostic);
    }

    public static Target GetConfiguredMapTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax configuredMapMethodInvocation)
        {
            return Target.Empty;
        }

        var analyzeResult = SourceCodeAnalyzer.AnalyzeConfiguredMapMethod(configuredMapMethodInvocation, semanticModel, cancellationToken);
        if (!analyzeResult.IsSuccess(out var successResult, out var failureResult))
        {
            if (failureResult.Error is not null)
            {
                return new ErrorTarget(failureResult.Error);
            }

            return Target.Empty;
        }
        var (source, destination, _, isSuccessOverloadResolution, location) = successResult;

        if (!SourceCodeAnalyzer.IsTypesAreClasses(source, destination))
        {
            var diagnostic = Diagnostics.MapWithNotSupportedForMapWith(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(source, destination))
        {
            var diagnostic = Diagnostics.MappedTypesEquals(location);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(source, destination, semanticModel))
        {
            var diagnostic = Diagnostics.MappedTypesHasImplicitConversion(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        var arguments = configuredMapMethodInvocation.ArgumentList.Arguments;
        return new ConfiguredMapTarget(source, destination, location, semanticModel, arguments, isSuccessOverloadResolution);
    }

    public static ImmutableArray<Target> GetUserMapTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not MethodDeclarationSyntax userMapMethodDeclaration)
        {
            return ImmutableArray<Target>.Empty;
        }

        var analyzeResult = SourceCodeAnalyzer.AnalyzeUserMapMethod(userMapMethodDeclaration, semanticModel, cancellationToken);
        if (!analyzeResult.IsSuccess(out var successResult, out var failureResult))
        {
            if (failureResult.Error is not null)
            {
                return ImmutableArray.Create<Target>(new ErrorTarget(failureResult.Error));
            }

            return ImmutableArray<Target>.Empty;
        }
        var (source, destination, mapMethod, _, location) = successResult;

        if (!mapMethod.IsGenericMethod || mapMethod.TypeParameters.Length != 1)
        {
            var diagnostic = Diagnostics.MapMethodMustBeGeneric(location);

            return ImmutableArray.Create<Target>(new ErrorTarget(diagnostic));
        }

        var userMapTarget = new UserMapTarget(source, destination, location);

        if (!mapMethod.IsExtensionMethod)
        {
            var diagnostic = Diagnostics.MapMethodMustBeExtension(location);

            return ImmutableArray.Create<Target>(new ErrorTarget(diagnostic), userMapTarget);
        }

        if (mapMethod.ReturnsVoid)
        {
            var diagnostic = Diagnostics.MapMethodMustNotReturnVoid(location);

            return ImmutableArray.Create<Target>(new ErrorTarget(diagnostic), userMapTarget);
        }

        if (mapMethod.DeclaredAccessibility != Accessibility.Internal)
        {
            var diagnostic = Diagnostics.MapMethodMustBeInternal(location);

            return ImmutableArray.Create<Target>(new ErrorTarget(diagnostic), userMapTarget);
        }

        return ImmutableArray.Create<Target>(userMapTarget);
    }

    public static Target GetProjectionTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax projectMethodInvocation)
        {
            return Target.Empty;
        }

        var analyzeResult = SourceCodeAnalyzer.AnalyzeProjectionMethod(projectMethodInvocation, semanticModel, cancellationToken);
        if (!analyzeResult.IsSuccess(out var successResult, out var failureResult))
        {
            if (failureResult.Error is not null)
            {
                return new ErrorTarget(failureResult.Error);
            }

            return Target.Empty;
        }
        var (source, destination, _, _, location) = successResult;

        if (SourceCodeAnalyzer.IsTypesAreEquals(source, destination))
        {
            var diagnostic = Diagnostics.ProjectedTypesEquals(location);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(source, destination, semanticModel))
        {
            var diagnostic = Diagnostics.ProjectedTypesHasImplicitConversion(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreClasses(source, destination))
        {
            return new ProjectionTarget(source, destination, location);
        }

        var notSupportedDiagnostic = Diagnostics.ProjectionNotSupported(location, source, destination);
        return new ErrorTarget(notSupportedDiagnostic);
    }

    public static Target GetConfiguredProjectionTarget(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax configuredProjectionMethodInvocation)
        {
            return Target.Empty;
        }

        var analyzeResult = SourceCodeAnalyzer.AnalyzeConfiguredProjectionMethod(configuredProjectionMethodInvocation, semanticModel, cancellationToken);
        if (!analyzeResult.IsSuccess(out var successResult, out var failureResult))
        {
            if (failureResult.Error is not null)
            {
                return new ErrorTarget(failureResult.Error);
            }

            return Target.Empty;
        }
        var (source, destination, _, isSuccessOverloadResolution, location) = successResult;

        if (!SourceCodeAnalyzer.IsTypesAreClasses(source, destination))
        {
            var diagnostic = Diagnostics.ConfiguredProjectionNotSupported(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesAreEquals(source, destination))
        {
            var diagnostic = Diagnostics.ProjectedTypesEquals(location);

            return new ErrorTarget(diagnostic);
        }

        if (SourceCodeAnalyzer.IsTypesHasImplicitConversion(source, destination, semanticModel))
        {
            var diagnostic = Diagnostics.ProjectedTypesHasImplicitConversion(location, source, destination);

            return new ErrorTarget(diagnostic);
        }

        var arguments = configuredProjectionMethodInvocation.ArgumentList.Arguments;
        return new ConfiguredProjectionTarget(source, destination, location, arguments, isSuccessOverloadResolution);
    }
}
