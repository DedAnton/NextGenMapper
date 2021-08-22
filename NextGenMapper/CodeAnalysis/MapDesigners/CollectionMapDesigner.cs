using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class CollectionMapDesigner
    {
        private const string LIST_NAME = "List";
        private const string LIST_FULL_NAME = "System.Collections.Generic.List<T>";
        private readonly List<SpecialType> _listInterfacesSpecislTypes = new()
        {
            SpecialType.System_Collections_Generic_ICollection_T,
            SpecialType.System_Collections_Generic_IEnumerable_T,
            SpecialType.System_Collections_Generic_IList_T,
            SpecialType.System_Collections_Generic_IReadOnlyCollection_T,
            SpecialType.System_Collections_Generic_IReadOnlyList_T
        };
        private readonly ClassMapDesigner _classMapDesigner;

        public CollectionMapDesigner()
        {
            _classMapDesigner = new();
        }

        public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            var collectionType = to switch
            {
                { TypeKind: TypeKind.Array } => CollectionType.Array,
                { OriginalDefinition: { Name: LIST_NAME } } list when list.OriginalDefinition.ToString() == LIST_FULL_NAME => CollectionType.List,
                INamedTypeSymbol toInterface when _listInterfacesSpecislTypes.Contains(toInterface.OriginalDefinition.SpecialType) => CollectionType.List,
                _ => CollectionType.Undefined
            };
            if (collectionType == CollectionType.Undefined)
            {
                throw new ArgumentException($"Error when mappig {from} to {to}. Collection type was undefined. Use List<T> or interfaces implemented by List<T>");
            }

            var elementTypeFrom = GetCollectionElementType(from);
            var elementTypeTo = GetCollectionElementType(to);

            var maps = new List<TypeMap>();
            maps.AddRange(_classMapDesigner.DesignMapsForPlanner(elementTypeFrom, elementTypeTo));
            maps.Add(new CollectionMap(from, to, elementTypeFrom, elementTypeTo, collectionType));

            return maps;
        }

        private ITypeSymbol GetCollectionElementType(ITypeSymbol collection)
            => collection switch
            {
                IArrayTypeSymbol array => array.ElementType,
                INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments.Single(),
                _ => throw new ArgumentOutOfRangeException($"Can`t get type of elements in collection {collection}")
            };
    }
}
