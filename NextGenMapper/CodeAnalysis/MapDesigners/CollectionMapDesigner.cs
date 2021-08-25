using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public List<TypeMap> DesignMapsForPlanner(Collection from, Collection to)
        {
            var maps = new List<TypeMap>();
            maps.AddRange(_classMapDesigner.DesignMapsForPlanner(from.ElementType, to.ElementType));

            var mapType = to.CollectionType switch
            {
                CollectionType.Array => CollectionMapType.ToArray,
                CollectionType.IEnumerable_T or CollectionType.ICollection_T or CollectionType.IList_T
                or CollectionType.List_T or CollectionType.IReadOnlyCollection_T or CollectionType.IReadOnlyList_T => CollectionMapType.ToList,
                CollectionType.ImmutableArray_T or CollectionType.IImmutableArray_T => CollectionMapType.ToImmutableArray,
                CollectionType.ImmutableList_T or CollectionType.IImmutableList_T => CollectionMapType.ToImmutableList,
                _ => throw new ArgumentOutOfRangeException(nameof(to), $"Collection type {to.CollectionType} not supported")
            };
            maps.Add(new CollectionMap(from, to, mapType));

            return maps;
        }

        //private ITypeSymbol GetCollectionElementType(ITypeSymbol collection)
        //    => collection switch
        //    {
        //        IArrayTypeSymbol array => array.ElementType,
        //        INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments.Single(),
        //        _ => throw new ArgumentOutOfRangeException($"Can`t get type of elements in collection {collection}")
        //    };
    }
}
