using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{ 
    private static void DesignCollectionsMap(
        ITypeSymbol source,
        ITypeSymbol destination,
        Location location,
        SemanticModel semanticModel,
        ImmutableList<ITypeSymbol> referencesHistory,
        ref ValueListBuilder<Map> maps,
        CancellationToken cancellationToken)
    {
        referencesHistory = referencesHistory.Add(source);

        var sourceKind = GetCollectionKind(source);
        var destinationKind = GetCollectionKind(destination);
        if (sourceKind == CollectionKind.Undefined || destinationKind == CollectionKind.Undefined)
        {
            var diagnostic = Diagnostics.UndefinedCollectionTypeError(location);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }

        var sourceItemType = GetCollectionItemType(source);
        if (sourceItemType is null)
        {
            var diagnostic = Diagnostics.CollectionItemTypeNotFoundError(location, source);
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }
        var destinationItemType = GetCollectionItemType(destination);
        if (destinationItemType is null)
        {
            var diagnostic = Diagnostics.CollectionItemTypeNotFoundError(location, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }

        var isTypeEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceItemType, destinationItemType);
        var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceItemType, destinationItemType, semanticModel);
        var map = Map.Collection(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            sourceKind,
            destinationKind,
            sourceItemType.ToNotNullableString(),
            destinationItemType.ToNotNullableString(),
            sourceItemType.NullableAnnotation == NullableAnnotation.Annotated,
            destinationItemType.NullableAnnotation == NullableAnnotation.Annotated,
            isTypeEquals,
            isTypesHasImplicitConversion,
            location);
        maps.Append(map);

        if (DesignersHelper.IsPotentialNullReference(sourceItemType, destinationItemType))
        {
            var diagnostic = Diagnostics.PossibleNullReference(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }

        if (!isTypeEquals && !isTypesHasImplicitConversion)
        {
            DesignMaps(sourceItemType, destinationItemType, location, semanticModel, referencesHistory, ref maps, cancellationToken);
        }
    }

    private static ITypeSymbol? GetCollectionItemType(ITypeSymbol collection)
        => collection switch
        {
            IArrayTypeSymbol array => array.ElementType,
            INamedTypeSymbol { TypeArguments: [var itemType] } => itemType,
            _ => null
        };

    private static CollectionKind GetCollectionKind(ITypeSymbol collection) => collection switch
    {
        { TypeKind: TypeKind.Array } => CollectionKind.Array,
        { Name: "List", ContainingNamespace: { Name: "Generic", ContainingNamespace: { Name: "Collections", ContainingNamespace.Name: "System" } } } => CollectionKind.List,
        { Name: "ImmutableArray", ContainingNamespace: { Name: "Immutable", ContainingNamespace: { Name: "Collections", ContainingNamespace.Name: "System" } } } => CollectionKind.ImmutableArray,
        { Name: "ImmutableList", ContainingNamespace: { Name: "Immutable", ContainingNamespace: { Name: "Collections", ContainingNamespace.Name: "System" } } } => CollectionKind.ImmutableList,
        { Name: "IImmutableList", ContainingNamespace: { Name: "Immutable", ContainingNamespace: { Name: "Collections", ContainingNamespace.Name: "System" } } } => CollectionKind.IImmutableList,
        { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } => CollectionKind.IEnumerable,
        { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => CollectionKind.ICollection,
        { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T } => CollectionKind.IReadOnlyCollection,
        { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyList_T } => CollectionKind.IReadOnlyList,
        { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IList_T } => CollectionKind.IList,
        _ => CollectionKind.Undefined
    };
}
