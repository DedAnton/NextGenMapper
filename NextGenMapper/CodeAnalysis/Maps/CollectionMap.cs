using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class CollectionMap : TypeMap
    {
        public Type ElementTypeFrom { get; }
        public Type ElementTypeTo { get; }
        public CollectionMapType CollectionMapType { get; }

        public CollectionMap(Collection from, Collection to, CollectionMapType collectionType)
            : base(from, to)
        {
            ElementTypeFrom = from.ElementType;
            ElementTypeTo = to.ElementType;
            CollectionMapType = collectionType;
        }
    }

    public enum CollectionMapType
    {
        ToList,
        ToArray,
        ToImmutableList,
        ToImmutableArray
    }
}
