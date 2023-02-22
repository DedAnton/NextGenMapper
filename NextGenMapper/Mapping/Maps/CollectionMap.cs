using NextGenMapper.Mapping.Maps.Models;
using System;

namespace NextGenMapper.Mapping.Maps;
internal readonly struct CollectionMap : IMap, IEquatable<CollectionMap>
{
    public CollectionMap(
        string source,
        string destination,
        CollectionKind sourceKind,
        CollectionKind destinationKind,
        string sourceItem,
        string destinationItem,
        bool isSourceItemNullable,
        bool isDestinationItemNullable,
        bool isItemsEquals,
        bool isItemsHasImpicitConversion)
    {
        Source = source;
        Destination = destination;
        SourceKind = sourceKind;
        DestinationKind = destinationKind;
        SourceItem = sourceItem;
        DestinationItem = destinationItem;
        IsSourceItemNullable = isSourceItemNullable;
        IsDestinationItemNullable = isDestinationItemNullable;
        IsItemsEquals = isItemsEquals;
        IsItemsHasImpicitConversion = isItemsHasImpicitConversion;
    }

    public string Source { get; }
    public string Destination { get; }
    public CollectionKind SourceKind { get; }
    public CollectionKind DestinationKind { get; }
    public string SourceItem { get; }
    public string DestinationItem { get; }
    public bool IsSourceItemNullable { get; }
    public bool IsDestinationItemNullable { get; }
    public bool IsItemsEquals { get; }
    public bool IsItemsHasImpicitConversion { get; }

    public bool Equals(CollectionMap other) => Source == other.Source && Destination == other.Destination;
}
