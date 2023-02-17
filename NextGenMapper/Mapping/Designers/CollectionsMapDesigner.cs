using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Designers;
internal static partial class MapDesigner
{
    private const string LIST_NAME = "List";
    private const string LIST_FULL_NAME = "System.Collections.Generic.List<T>";

    private static MapsList DesignCollectionsMap(
        ITypeSymbol source,
        ITypeSymbol destination,
        Location location,
        SemanticModel semanticModel)
    {
        var sourceKind = GetCollectionKind(source);
        var destinationKind = GetCollectionKind(destination);
        if (sourceKind == CollectionKind.Undefined || destinationKind == CollectionKind.Undefined)
        {
            var diagnostic = Diagnostics.UndefinedCollectionTypeError(location);

            return MapsList.Create(Map.Error(source, destination, diagnostic));
        }

        var sourceItemType = GetCollectionItemType(source);
        var destinationItemType = GetCollectionItemType(destination);

        var maps = new MapsList();
        var map = Map.Collection(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            sourceKind,
            destinationKind,
            sourceItemType.ToString(),
            destinationItemType.ToString(),
            SourceCodeAnalyzer.IsTypesAreEquals(sourceItemType, destinationItemType),
            SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceItemType, destinationItemType, semanticModel));
        maps.AddFirst(map);

        if (!map.CollectionMap.IsItemsEquals && !map.CollectionMap.IsItemsHasImpicitConversion)
        {
            //TODO: check situation when collection type contains property with same collection (potential circular reference)
            var collectionsElementsTypesMaps = MapDesigner.DesignMaps(sourceItemType, destinationItemType, location, semanticModel, ImmutableList<ITypeSymbol>.Empty);
            maps.Append(collectionsElementsTypesMaps);
        }

        return maps;
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
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } => CollectionKind.IEnumerable,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => CollectionKind.ICollection,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T } => CollectionKind.IReadOnlyCollection,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyList_T } => CollectionKind.IReadOnlyList,
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IList_T } => CollectionKind.IList,
        _ => CollectionKind.Undefined
    };
}
