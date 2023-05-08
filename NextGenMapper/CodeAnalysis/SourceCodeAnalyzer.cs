using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Targets.Models;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System.Linq;
using System.Threading;

namespace NextGenMapper.CodeAnalysis;

internal static class SourceCodeAnalyzer
{
    public static bool IsMapMethodInvocationSyntaxNode(SyntaxNode node)
    => node is InvocationExpressionSyntax 
    {
        Expression: MemberAccessExpressionSyntax
        {
            Name: GenericNameSyntax
            {
                Identifier.ValueText: StartMapperSource.MapMethodName
            }
        }
    };

    public static bool IsConfiguredMapMethodInvocationSynaxNode(SyntaxNode node)
    => node is InvocationExpressionSyntax
    {
        Expression: MemberAccessExpressionSyntax
        {
            Name: GenericNameSyntax
            {
                Identifier.ValueText: StartMapperSource.ConfiguredMapMethodName
            }
        }
    };

    public static bool IsProjectionMethodInvocationSyntaxNode(SyntaxNode node)
    => node is InvocationExpressionSyntax
    {
        Expression: MemberAccessExpressionSyntax
        {
            Name: GenericNameSyntax
            {
                Identifier.ValueText: StartMapperSource.ProjectionMethodName
            }
        }
    };

    public static bool IsConfiguredProjectionMethodInvocationSyntaxNode(SyntaxNode node)
    => node is InvocationExpressionSyntax
    {
        Expression: MemberAccessExpressionSyntax
        {
            Name: GenericNameSyntax
            {
                Identifier.ValueText: StartMapperSource.ConfiguredProjectionMethodName
            }
        }
    };

    public static bool IsUserMapMethodDeclarationSyntaxNode(SyntaxNode node)
    => node is MethodDeclarationSyntax
    {
        Identifier.ValueText: "Map",
        ParameterList.Parameters.Count: 1,
        Parent: ClassDeclarationSyntax
        {
            Arity: 0,
            Identifier.ValueText: "Mapper",
            Parent: NamespaceDeclarationSyntax
            {
                Name: IdentifierNameSyntax
                {
                    Identifier.ValueText: "NextGenMapper"
                }
            }
                or FileScopedNamespaceDeclarationSyntax
            {
                Name: IdentifierNameSyntax
                {
                    Identifier.ValueText: "NextGenMapper"
                }
            }
        }
    };

    public static MapMethodAnalysisResult AnalyzeMapMethod(
        InvocationExpressionSyntax mapMethodInvocation, 
        SemanticModel semanticModel, 
        CancellationToken cancellationToken)
    {
        if (mapMethodInvocation.Expression is MemberAccessExpressionSyntax memberAccessExpression
            && semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken).Symbol is IMethodSymbol
            {
                IsExtensionMethod: true,
                MethodKind: MethodKind.ReducedExtension
            } method
            && method.ReducedFrom?.ToDisplayString() == StartMapperSource.MapMethodFullName
            && semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type is ITypeSymbol
            {
                TypeKind: not TypeKind.Error
            } source
            && method.ReturnType is ITypeSymbol
            {
                TypeKind: not TypeKind.Error
            } destination)
        {
            return MapMethodAnalysisResult.Success(source, destination);
        }

        return MapMethodAnalysisResult.Fail();
    }

    public static ConfiguredMapMethodAnalysisResult AnalyzeConfiguredMapMethod(
        InvocationExpressionSyntax configuredMapMethodInvocation, 
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (configuredMapMethodInvocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return ConfiguredMapMethodAnalysisResult.Fail();
        }

        var invocationMethodSymbolInfo = semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken);
        var invocationMethodSymbol = invocationMethodSymbolInfo.Symbol;

        var isCompleteMethod = false;
        if (invocationMethodSymbol is null
            && invocationMethodSymbolInfo.CandidateSymbols.Length == 1
            && invocationMethodSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure
            && configuredMapMethodInvocation.ArgumentList.Arguments.Count > 0)
        {
            invocationMethodSymbol = invocationMethodSymbolInfo.CandidateSymbols[0];
            isCompleteMethod = true;
        }

        if (invocationMethodSymbol is null)
        {
            return ConfiguredMapMethodAnalysisResult.Fail();
        }

        if (invocationMethodSymbol is IMethodSymbol method
            && method.IsExtensionMethod
            && method.MethodKind == MethodKind.ReducedExtension
            && method.ReducedFrom?.ToDisplayString() == StartMapperSource.MapWithMethodFullName
            && semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type is ITypeSymbol { TypeKind: not TypeKind.Error } source
            && method.ReturnType is ITypeSymbol { TypeKind: not TypeKind.Error } destination)
        {
            return ConfiguredMapMethodAnalysisResult.Success(source, destination, isCompleteMethod);
        }

        return ConfiguredMapMethodAnalysisResult.Fail();
    }

    public static UserMapMethodAnalysisResult AnalyzeUserMapMethod(
        MethodDeclarationSyntax userMapMethodDeclaration, 
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
                Parameters.Length: 1,
                ReturnType: not ITypeParameterSymbol
            } method
            && method.Parameters[0].Type is ITypeSymbol { TypeKind: not TypeKind.Error } source
            && method.ReturnType is ITypeSymbol { TypeKind: not TypeKind.Error } destination)
        {
            return UserMapMethodAnalysisResult.Success(source, destination, method);
        }

        return UserMapMethodAnalysisResult.Fail();
    }

    public static MapMethodAnalysisResult AnalyzeProjectionMethod(
        InvocationExpressionSyntax mapMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (mapMethodInvocation.Expression is MemberAccessExpressionSyntax memberAccessExpression
            && semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken).Symbol is IMethodSymbol
            {
                IsExtensionMethod: true,
                MethodKind: MethodKind.ReducedExtension
            } method
            && method.ReducedFrom?.ToDisplayString() == StartMapperSource.ProjectionMethodFullName
            && semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type is INamedTypeSymbol
            {
                TypeKind: not TypeKind.Error,
                IsGenericType: true,
                Arity: 1,
                MetadataName: "IQueryable`1"
            } sourceQueryable
            && sourceQueryable.IsQueryable()
            && sourceQueryable.TypeArguments[0] is { TypeKind: not TypeKind.Error} source
            && method.ReturnType is ITypeSymbol { TypeKind: not TypeKind.Error } destination)
        {
            return MapMethodAnalysisResult.Success(source, destination);
        }

        return MapMethodAnalysisResult.Fail();
    }

    public static ConfiguredMapMethodAnalysisResult AnalyzeConfiguredProjectionMethod(
        InvocationExpressionSyntax configuredMapMethodInvocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (configuredMapMethodInvocation.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return ConfiguredMapMethodAnalysisResult.Fail();
        }

        var invocationMethodSymbolInfo = semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken);
        var invocationMethodSymbol = invocationMethodSymbolInfo.Symbol;

        var isCompleteMethod = false;
        if (invocationMethodSymbol is null
            && invocationMethodSymbolInfo.CandidateSymbols.Length == 1
            && invocationMethodSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure
            && configuredMapMethodInvocation.ArgumentList.Arguments.Count > 0)
        {
            invocationMethodSymbol = invocationMethodSymbolInfo.CandidateSymbols[0];
            isCompleteMethod = true;
        }

        if (invocationMethodSymbol is null)
        {
            return ConfiguredMapMethodAnalysisResult.Fail();
        }

        if (invocationMethodSymbol is IMethodSymbol method
            && method.IsExtensionMethod
            && method.MethodKind == MethodKind.ReducedExtension
            && method.ReducedFrom?.ToDisplayString() == StartMapperSource.ConfiguredProjectionMethodFullName
            && semanticModel.GetTypeInfo(memberAccessExpression.Expression, cancellationToken).Type is INamedTypeSymbol
            {
                TypeKind: not TypeKind.Error,
                IsGenericType: true,
                Arity: 1,
                MetadataName: "IQueryable`1"
            } sourceQueryable
            && sourceQueryable.IsQueryable()
            && sourceQueryable.TypeArguments[0] is { TypeKind: not TypeKind.Error } source
            && method.ReturnType is ITypeSymbol { TypeKind: not TypeKind.Error } destination)
        {
            return ConfiguredMapMethodAnalysisResult.Success(source, destination, isCompleteMethod);
        }

        return ConfiguredMapMethodAnalysisResult.Fail();
    }

    public static bool IsTypesAreEquals(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => SymbolEqualityComparer.Default.Equals(sourceType, destinationType);

    public static bool IsTypesHasImplicitConversion(ITypeSymbol sourceType, ITypeSymbol destinationType, SemanticModel semanticModel)
        => semanticModel.Compilation.HasImplicitConversion(sourceType, destinationType);

    public static bool IsTypesAreEnums(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.TypeKind == TypeKind.Enum && destinationType.TypeKind == TypeKind.Enum;

    public static bool IsTypesAreCollections(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.IsCollection() && destinationType.IsCollection();

    public static bool IsQueryable(this ITypeSymbol type)
    {
        const string IQueryableDefinition = "System.Linq.IQueryable<T>";

        if (type.OriginalDefinition.ToDisplayString() == IQueryableDefinition)
        {
            return true;
        }

        foreach (var @interface in type.AllInterfaces)
        {
            if (@interface.OriginalDefinition.ToDisplayString() == IQueryableDefinition)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCollection(this ITypeSymbol type)
    {
        //TODO: check all other collections special types 
        if (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            return true;
        }

        foreach (var @interface in type.AllInterfaces)
        {
            if (@interface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsTypesAreClasses(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.TypeKind == TypeKind.Class && destinationType.TypeKind == TypeKind.Class;

    public static bool IsNamedArgument(this ArgumentSyntax argument) => argument.NameColon?.Name.Identifier.ValueText is not null;
}