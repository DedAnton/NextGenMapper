using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class CollectionMap : TypeMap
    {
        public ITypeSymbol ItemFrom { get; }
        public ITypeSymbol ItemTo { get; }
        public CollectionType CollectionType { get; }

        public CollectionMap(ITypeSymbol from, ITypeSymbol to, ITypeSymbol itemFrom, ITypeSymbol itemTo,  CollectionType collectionType)
            : base(from, to)
        {
            ItemFrom = itemFrom;
            ItemTo = itemTo;
            CollectionType = collectionType;
        }
    }

    public enum CollectionType
    {
        Undefined,
        List,
        Array
    }
}
