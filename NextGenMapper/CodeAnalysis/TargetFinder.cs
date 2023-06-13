using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace NextGenMapper.CodeAnalysis;

internal static class TargetFinder
{
    public static ImmutableArray<Target> GetUserMapTarget(
        SyntaxNode userMapMethodDeclaration,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (semanticModel.GetDeclaredSymbol(userMapMethodDeclaration, cancellationToken) is IMethodSymbol
            {
                IsAsync: false,
                MethodKind: MethodKind.Ordinary,
                IsDefinition: true,
                IsStatic: true,
                Name: "Map",
                ReturnType: not ITypeParameterSymbol,
                Parameters: [{ Type: { TypeKind: not TypeKind.Error } source }],
                ReturnType: ITypeSymbol { TypeKind: not TypeKind.Error } destination
            } method)
        {
            var location = userMapMethodDeclaration.GetLocation();
            var userMapTarget = new UserMapTarget(source, destination, location);

            return method switch
            {
                { TypeParameters: not [_] } => Concat(Diagnostics.MapMethodMustBeGeneric(location), target: null),
                { IsExtensionMethod: false } => Concat(Diagnostics.MapMethodMustBeExtension(location), userMapTarget),
                { ReturnsVoid: true } => Concat(Diagnostics.MapMethodMustNotReturnVoid(location), userMapTarget),
                { DeclaredAccessibility: not Accessibility.Internal } => Concat(Diagnostics.MapMethodMustBeInternal(location), userMapTarget),
                _ => ImmutableArray.Create<Target>(userMapTarget)
            };

            static ImmutableArray<Target> Concat(Diagnostic diagnostic, Target? target)
                => target is not null
                ? ImmutableArray.Create(new ErrorTarget(diagnostic), target)
                : ImmutableArray.Create<Target>(new ErrorTarget(diagnostic));
        }

        return ImmutableArray<Target>.Empty;
    }

    public static Target GetMapTarget(
        SyntaxNode mapMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var mapMethodInfo = CreateMapMethodInfo(mapMethodInvocation, semanticModel, cancellationToken);

        if (mapMethodInfo is not null
            && mapMethodInfo.IsMapMethod()
            && mapMethodInfo.Name == "Map"
            && mapMethodInfo.TryGetSource(cancellationToken, out var source)
            && mapMethodInfo.TryGetDestination(out var destination))
        {
            var target = new MapTarget(source, destination, MapTargetKind.Undefined, mapMethodInfo.Location, semanticModel);
            var validationResult = TargetValidator.Validate(target, semanticModel);

            return validationResult switch
            {
                { IsTypesAreEquals: true } => new ErrorTarget(Diagnostics.MappedTypesEquals(mapMethodInfo.Location)),
                { IsTypesHasImplicitConversion: true } => new ErrorTarget(Diagnostics.MappedTypesHasImplicitConversion(mapMethodInfo.Location, source, destination)),
                { IsTypesAreCollections: true } => target with { Kind = MapTargetKind.Collection },
                { IsTypesAreClasses: true } => target with { Kind = MapTargetKind.Class },
                { IsTypesAreEnums: true } => target with { Kind = MapTargetKind.Enum },
                _ => new ErrorTarget(Diagnostics.MappingNotSupported(mapMethodInfo.Location, source, destination))
            };
        }

        return Target.Empty;
    }

    public static Target GetConfiguredMapTarget(
        SyntaxNode configuredMapMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var mapMethodInfo = CreateMapMethodInfo(configuredMapMethodInvocation, semanticModel, cancellationToken);

        if (mapMethodInfo is not null
            && mapMethodInfo.IsMapMethod()
            && mapMethodInfo.Name == "MapWith"
            && mapMethodInfo.TryGetSource(cancellationToken, out var source)
            && mapMethodInfo.TryGetDestination(out var destination))
        {
            var target = new ConfiguredMapTarget(source, destination, mapMethodInfo);
            var validationResult = TargetValidator.Validate(target, semanticModel);

            return validationResult switch
            {
                { IsTypesAreClasses: true, IsTypesAreEquals: false, IsTypesHasImplicitConversion: false } => target,
                { IsTypesAreEquals: true } => new ErrorTarget(Diagnostics.MappedTypesEquals(mapMethodInfo.Location)),
                { IsTypesHasImplicitConversion: true } => new ErrorTarget(Diagnostics.MappedTypesHasImplicitConversion(mapMethodInfo.Location, source, destination)),
                _ => new ErrorTarget(Diagnostics.MapWithNotSupported(mapMethodInfo.Location, source, destination))
            };
        }

        return Target.Empty;
    }

    public static Target GetProjectionTarget(
        SyntaxNode projectionMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var mapMethodInfo = CreateMapMethodInfo(projectionMethodInvocation, semanticModel, cancellationToken);

        if (mapMethodInfo is not null
            && mapMethodInfo.IsProjectMethod()
            && mapMethodInfo.Name == "Project"
            && mapMethodInfo.TryGetSourceFromIQueryable(cancellationToken, out var source)
            && mapMethodInfo.TryGetDestination(out var destination))
        {
            var target = new ProjectionTarget(source, destination, mapMethodInfo.Location);
            var validationResult = TargetValidator.Validate(target, semanticModel);

            return validationResult switch
            {
                { IsTypesAreClasses: true, IsTypesAreEquals: false, IsTypesHasImplicitConversion: false } => target,
                { IsTypesAreEquals: true } => new ErrorTarget(Diagnostics.ProjectedTypesEquals(mapMethodInfo.Location)),
                { IsTypesHasImplicitConversion: true } => new ErrorTarget(Diagnostics.ProjectedTypesHasImplicitConversion(mapMethodInfo.Location, source, destination)),
                _ => new ErrorTarget(Diagnostics.ProjectionNotSupported(mapMethodInfo.Location, source, destination))
            };
        }

        if (SourceIsNotGenericIQueryable(mapMethodInfo, cancellationToken))
        {
            var diagnostic = Diagnostics.NonGenericIQueryableError(mapMethodInfo.Location);

            return new ErrorTarget(diagnostic);
        }

        return Target.Empty;
    }

    public static Target GetConfiguredProjectionTarget(
        SyntaxNode configuredProjectionMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var mapMethodInfo = CreateMapMethodInfo(configuredProjectionMethodInvocation, semanticModel, cancellationToken);

        if (mapMethodInfo is not null
            && mapMethodInfo.IsProjectMethod()
            && mapMethodInfo.Name == "ProjectWith"
            && mapMethodInfo.TryGetSourceFromIQueryable(cancellationToken, out var source)
            && mapMethodInfo.TryGetDestination(out var destination))
        {
            var target = new ConfiguredProjectionTarget(source, destination, mapMethodInfo);
            var validationResult = TargetValidator.Validate(target, semanticModel);

            return validationResult switch
            {
                { IsTypesAreClasses: true, IsTypesAreEquals: false, IsTypesHasImplicitConversion: false } => target,
                { IsTypesAreEquals: true } => new ErrorTarget(Diagnostics.ProjectedTypesEquals(mapMethodInfo.Location)),
                { IsTypesHasImplicitConversion: true } => new ErrorTarget(Diagnostics.ProjectedTypesHasImplicitConversion(mapMethodInfo.Location, source, destination)),
                _ => new ErrorTarget(Diagnostics.ConfiguredProjectionNotSupported(mapMethodInfo.Location, source, destination))
            };
        }

        if (SourceIsNotGenericIQueryable(mapMethodInfo, cancellationToken))
        {
            var diagnostic = Diagnostics.NonGenericIQueryableError(mapMethodInfo.Location);

            return new ErrorTarget(diagnostic);
        }

        return Target.Empty;
    }

    private static MapMethodInfo? CreateMapMethodInfo(
        SyntaxNode mapMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (mapMethodInvocation is not InvocationExpressionSyntax invocationExpression
            || invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return null;
        }

        var methodSymbolInfo = semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken);
        var methodSymbol = methodSymbolInfo.Symbol;
        var isSuccessOverloadResolution = methodSymbol != null;

        if (methodSymbol is null
            && methodSymbolInfo is { CandidateSymbols.Length: > 0, CandidateReason: CandidateReason.OverloadResolutionFailure }
            && invocationExpression.ArgumentList.Arguments.Count > 0)
        {
            methodSymbol = methodSymbolInfo.CandidateSymbols[0];
        }

        if (methodSymbol is IMethodSymbol method)
        {
            var location = invocationExpression.GetLocation();

            return new MapMethodInfo(method.Name, invocationExpression, memberAccessExpression, method, isSuccessOverloadResolution, location, semanticModel);
        }

        return null;
    }

    private static bool IsMapMethod(this MapMethodInfo mapMethodInfo)
        => mapMethodInfo.MethodSymbol is IMethodSymbol
        {
            Name: "Map" or "MapWith",
            IsExtensionMethod: true,
            MethodKind: MethodKind.ReducedExtension,
            ReceiverType.SpecialType: SpecialType.System_Object,
            ContainingNamespace.Name: "NextGenMapper",
            ContainingType.Name: "Mapper",
            TypeParameters: [{ Name: "To" }]
        };

    private static bool IsProjectMethod(this MapMethodInfo mapMethodInfo)
    {
        if (mapMethodInfo.MethodSymbol is IMethodSymbol
            {
                Name: "Project" or "ProjectWith",
                IsExtensionMethod: true,
                MethodKind: MethodKind.ReducedExtension,
                ContainingNamespace.Name: "NextGenMapper",
                ContainingType.Name: "Mapper",
                TypeParameters: [{ Name: "To" }],
                ReceiverType: { }
            } method)
        {
            if (IsIQueryable(method.ReceiverType))
            {
                return true;
            }

            foreach (var @interface in method.ReceiverType.AllInterfaces)
            {
                if (IsIQueryable(@interface))
                {
                    return true;
                }
            }
        }

        return false;

        static bool IsIQueryable(ITypeSymbol type) => type is INamedTypeSymbol
        {
            Name: "IQueryable",
            ContainingNamespace:
            {
                Name: "Linq",
                ContainingNamespace.Name: "System"
            }
        };
    }

    private static bool TryGetSource(
        this MapMethodInfo mapMethodInfo,
        CancellationToken cancellationToken,
        [NotNullWhen(true)] out ITypeSymbol? source)
    {
        if (mapMethodInfo.SemanticModel.GetTypeInfo(mapMethodInfo.InvocationMemberAccessSyntax.Expression, cancellationToken).Type
                is ITypeSymbol { TypeKind: not TypeKind.Error } sourceType)
        {
            source = sourceType;
            return true;
        }

        source = null;
        return false;
    }

    private static bool TryGetDestination(this MapMethodInfo mapMethodInfo, [NotNullWhen(true)] out ITypeSymbol? destination)
    {
        if (mapMethodInfo.MethodSymbol.ReturnType is ITypeSymbol { TypeKind: not TypeKind.Error } destinationType)
        {
            destination = destinationType;
            return true;
        }

        destination = null;
        return false;
    }

    private static bool TryGetSourceFromIQueryable(
        this MapMethodInfo mapMethodInfo,
        CancellationToken cancellationToken,
        [NotNullWhen(true)] out ITypeSymbol? source)
    {
        if (mapMethodInfo.SemanticModel
            .GetTypeInfo(mapMethodInfo.InvocationMemberAccessSyntax.Expression, cancellationToken).Type is INamedTypeSymbol
            {
                TypeKind: not TypeKind.Error,
                Name: "IQueryable",
                TypeArguments: [ITypeSymbol { TypeKind: not TypeKind.Error } sourceType],
                ContainingNamespace:
                {
                    Name: "Linq",
                    ContainingNamespace.Name: "System"
                }
            })
        {
            source = sourceType;
            return true;
        }

        source = null;
        return false;
    }

    private static bool SourceIsNotGenericIQueryable([NotNullWhen(true)] MapMethodInfo? mapMethodInfo, CancellationToken cancellationToken)
    {
        var sourceType = mapMethodInfo?.SemanticModel
            .GetTypeInfo(mapMethodInfo.InvocationMemberAccessSyntax.Expression, cancellationToken).Type;
        if (sourceType is null || sourceType.TypeKind == TypeKind.Error)
        {
            return false;
        }

        if (IsIQueryable(sourceType))
        {
            return true;
        }

        foreach (var @interface in sourceType.AllInterfaces)
        {
            if (IsIQueryable(@interface))
            {
                return true;
            }
        }

        static bool IsIQueryable(ITypeSymbol type) => type is INamedTypeSymbol
        {
            IsGenericType: false,
            Name: "IQueryable",
            ContainingNamespace:
            {
                Name: "Linq",
                ContainingNamespace.Name: "System"
            }
        };

        return false;
    }
}
