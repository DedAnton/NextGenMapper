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
        private readonly TypeMapDesigner _classMapDesigner;
        private readonly DiagnosticReporter _diagnosticReporter;

        public CollectionMapDesigner(DiagnosticReporter diagnosticReporter, MapPlanner mapPlanner, SemanticModel semanticModel)
        {
            _classMapDesigner = new(diagnosticReporter, mapPlanner, semanticModel);
            _diagnosticReporter = diagnosticReporter;
        }

        public CollectionMapDesigner(DiagnosticReporter diagnosticReporter, TypeMapDesigner classMapDesigner)
        {
            _classMapDesigner = classMapDesigner;
            _diagnosticReporter = diagnosticReporter;
        }

        public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, Location mapLocation)
        {
            var collectionTypeFrom = GetCollectionType(from);
            var collectionTypeTo = GetCollectionType(to);
            if (collectionTypeFrom == CollectionType.Undefined)
            {
                _diagnosticReporter.ReportUndefinedCollectionTypeError(mapLocation);
                return new();
            }
            if (collectionTypeTo == CollectionType.Undefined)
            {
                _diagnosticReporter.ReportUndefinedCollectionTypeError(mapLocation);
                return new();
            }

            var elementTypeFrom = GetCollectionElementType(from);
            var elementTypeTo = GetCollectionElementType(to);

            var maps = new List<TypeMap>();

            maps.AddRange(_classMapDesigner.DesignMapsForPlanner(elementTypeFrom, elementTypeTo, mapLocation));
            maps.Add(new CollectionMap(from, to, elementTypeFrom, elementTypeTo, collectionTypeFrom, collectionTypeTo, mapLocation));

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

        private CollectionType GetCollectionType(ITypeSymbol collection) => collection switch
        {
            { TypeKind: TypeKind.Array } => CollectionType.Array,
            { OriginalDefinition.Name: LIST_NAME } list when list.OriginalDefinition.ToString() == LIST_FULL_NAME => CollectionType.List,
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } => CollectionType.IEnumerable,
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_ICollection_T } => CollectionType.ICollection,
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyCollection_T } => CollectionType.IReadOnlyCollection,
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IReadOnlyList_T } => CollectionType.IReadOnlyList,
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IList_T } => CollectionType.IList,
            _ => CollectionType.Undefined
        };
    }
}
