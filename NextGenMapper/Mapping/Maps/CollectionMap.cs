using NextGenMapper.Mapping.Maps.Models;

namespace NextGenMapper.Mapping.Maps;
internal readonly struct CollectionMap : IMap
{
    public CollectionMap(
        string source,
        string destination,
        CollectionKind sourceKind,
        CollectionKind destinationKind,
        string sourceItem,
        string destinationItem,
        bool isItemsEquals,
        bool isItemsHasImpicitConversion)
    {
        Source = source;
        Destination = destination;
        SourceKind = sourceKind;
        DestinationKind = destinationKind;
        SourceItem = sourceItem;
        DestinationItem = destinationItem;
        IsItemsEquals = isItemsEquals;
        IsItemsHasImpicitConversion = isItemsHasImpicitConversion;
    }

    public string Source { get; }
    public string Destination { get; }
    public CollectionKind SourceKind { get; }
    public CollectionKind DestinationKind { get; }
    public string SourceItem { get; }
    public string DestinationItem { get; }
    public bool IsItemsEquals { get; }
    public bool IsItemsHasImpicitConversion { get; }
}
