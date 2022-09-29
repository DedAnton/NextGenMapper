using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class CollectionMap : TypeMap
    {
        public ITypeSymbol ItemFrom { get; }
        public ITypeSymbol ItemTo { get; }
        public CollectionType CollectionFrom { get; }
        public CollectionType CollectionTo { get; }

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
}
