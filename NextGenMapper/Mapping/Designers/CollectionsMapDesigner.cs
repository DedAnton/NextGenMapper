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
    private const string LIST_NAME = "List";
    private const string LIST_FULL_NAME = "System.Collections.Generic.List<T>";

    private const string IMMUTABLE_ARRAY_NAME = "ImmutableArray";
    private const string IMMUTABLE_ARRAY_FULL_NAME = "System.Collections.Immutable.ImmutableArray<T>";

    private const string IMMUTABLE_LIST_NAME = "ImmutableList";
    private const string IMMUTABLE_LIST_FULL_NAME = "System.Collections.Immutable.ImmutableList<T>";

    private const string I_IMMUTABLE_LIST_NAME = "IImmutableList";
    private const string I_IMMUTABLE_LIST_FULL_NAME = "System.Collections.Immutable.IImmutableList<T>";

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
        var destinationItemType = GetCollectionItemType(destination);

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
            isTypesHasImplicitConversion);
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

    private static ITypeSymbol GetCollectionItemType(ITypeSymbol collection)
        => collection switch
        {
            IArrayTypeSymbol array => array.ElementType,
            INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments[0],
            //TODO: figure out how to normally handle such a case, display diagnostics and not fall down with an exception
            _ => throw new ArgumentOutOfRangeException($"Can`t get type of elements in collection {collection}")
        };

    private static CollectionKind GetCollectionKind(ITypeSymbol collection) => collection switch
    {
        { TypeKind: TypeKind.Array } => CollectionKind.Array,
        { OriginalDefinition.Name: LIST_NAME } list when list.OriginalDefinition.ToString() == LIST_FULL_NAME => CollectionKind.List,
        { OriginalDefinition.Name: IMMUTABLE_ARRAY_NAME } immutableArray when immutableArray.OriginalDefinition.ToString() == IMMUTABLE_ARRAY_FULL_NAME => CollectionKind.ImmutableArray,
        { OriginalDefinition.Name: IMMUTABLE_LIST_NAME } immutableList when immutableList.OriginalDefinition.ToString() == IMMUTABLE_LIST_FULL_NAME => CollectionKind.ImmutableList,
        { OriginalDefinition.Name: I_IMMUTABLE_LIST_NAME } iImmutableList when iImmutableList.OriginalDefinition.ToString() == I_IMMUTABLE_LIST_FULL_NAME => CollectionKind.IImmutableList,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } => CollectionKind.IEnumerable,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => CollectionKind.ICollection,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T } => CollectionKind.IReadOnlyCollection,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyList_T } => CollectionKind.IReadOnlyList,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IList_T } => CollectionKind.IList,
        _ => CollectionKind.Undefined
    };
}
