using NextGenMapper.CodeAnalysis.Models;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class CollectionMap : TypeMap
    {
        public Type ElementFromType { get; }
        public Type ElementToType { get; }
        public CollectionType MapToCollectionType { get; }

        public CollectionMap(Collection from, Collection to)
            : base(from.ElementType, to.ElementType)
        {
            ElementFromType = from.ElementType;
            ElementToType = from.ElementType;
            MapToCollectionType = to.IsArray ? CollectionType.Array : CollectionType.List;
        }
    }

    public enum CollectionType
    {
        List,
        Array
    }
}
