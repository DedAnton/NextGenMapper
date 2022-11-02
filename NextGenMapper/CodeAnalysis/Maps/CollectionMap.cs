using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class CollectionMap : TypeMap
    {
        public ITypeSymbol ItemFrom { get; }
        public ITypeSymbol ItemTo { get; }
        public CollectionType CollectionFrom { get; }
        public CollectionType CollectionTo { get; }

        public bool IsItemsTypesEquals => SymbolEqualityComparer.Default.Equals(ItemFrom, ItemTo);

        public CollectionMap(
            ITypeSymbol from, 
            ITypeSymbol to, 
            ITypeSymbol itemFrom, 
            ITypeSymbol itemTo, 
            CollectionType collectionFrom, 
            CollectionType collectionTo, 
            Location mapLocaion)
            : base(from, to, mapLocaion)
        {
            ItemFrom = itemFrom;
            ItemTo = itemTo;
            CollectionFrom = collectionFrom;
            CollectionTo = collectionTo;
        }
    }

    public enum CollectionType
    {
        Undefined,
        Array,
        List,
        ICollection,
        IList,
        IEnumerable,
        IReadOnlyCollection,
        IReadOnlyList
    }

    public static class CollectionTypeExtensions
    {
        public static bool IsInterface(this CollectionType type) => type is CollectionType.IEnumerable or CollectionType.ICollection
            or CollectionType.IList or CollectionType.IReadOnlyCollection or CollectionType.IReadOnlyList;

        public static bool IsListInterface(this CollectionType type)
            => type is CollectionType.IReadOnlyCollection or CollectionType.IReadOnlyList;

        public static bool IsArrayInterface(this CollectionType type)
            => type is CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList;

        public static bool IsArray(this CollectionType type) => type is CollectionType.Array;

        public static bool IsList(this CollectionType type) => type is CollectionType.List;
    }
}
