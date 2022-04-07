using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class CollectionMapDesigner
    {
        private const string LIST_NAME = "List";
        private const string LIST_FULL_NAME = "System.Collections.Generic.List<T>";
        private readonly HashSet<SpecialType> _listInterfacesSpecislTypes = new()
        {
            SpecialType.System_Collections_Generic_ICollection_T,
            SpecialType.System_Collections_Generic_IEnumerable_T,
            SpecialType.System_Collections_Generic_IList_T,
            SpecialType.System_Collections_Generic_IReadOnlyCollection_T,
            SpecialType.System_Collections_Generic_IReadOnlyList_T
        };
        private readonly ClassMapDesigner _classMapDesigner;
        private readonly DiagnosticReporter _diagnosticReporter;

        public CollectionMapDesigner(DiagnosticReporter diagnosticReporter)
        {
            _classMapDesigner = new(diagnosticReporter);
            _diagnosticReporter = diagnosticReporter;
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
                _diagnosticReporter.ReportUndefinedCollectionTypeError(to.Locations);
                return new();
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
                INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments[0],
                //TODO: figure out how to normally handle such a case, display diagnostics and not fall down with an exception
                _ => throw new ArgumentOutOfRangeException($"Can`t get type of elements in collection {collection}")
            };
    }
}
