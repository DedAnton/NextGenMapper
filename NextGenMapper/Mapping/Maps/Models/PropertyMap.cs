namespace NextGenMapper.Mapping.Maps.Models;

internal readonly struct PropertyMap : IMap
{
    public PropertyMap(
        string sourceName,
        string destinationName,
        string sourceType,
        string destinationType,
        bool isSourceNullable,
        bool isDestinationNullable,
        bool isTypesEquals,
        bool hasImplicitConversion,
        string? userArgumentName = null)
    {
        SourceName = sourceName;
        DestinationName = destinationName;
        SourceType = sourceType;
        DestinationType = destinationType;
        IsSourceNullable = isSourceNullable;
        IsDestinationNullable = isDestinationNullable;
        IsTypesEquals = isTypesEquals;
        HasImplicitConversion = hasImplicitConversion;
        UserArgumentName = userArgumentName;
    }

    public string SourceName { get; }
    public string DestinationName { get; }
    public string SourceType { get; }
    public string DestinationType { get; }
    public bool IsSourceNullable { get; }
    public bool IsDestinationNullable { get; }
    public bool IsTypesEquals { get; }
    public bool HasImplicitConversion { get; }
    public string? UserArgumentName { get; }

    string IMap.Source => SourceType;
    string IMap.Destination => DestinationType;
}