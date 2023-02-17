using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps.Models;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct Map
{
    public ClassMap ClassMap { get; }
    public CollectionMap CollectionMap { get; }
    public EnumMap EnumMap { get; }
    public ConfiguredMap ConfiguredMap { get; }
    public UserMap UserMap { get; }
    public ErrorMap ErrorMap { get; }
    public PotentialErrorMap PotentialErrorMap { get; }
    public MapType Type { get; }

    public static Map Class(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties)
        => new(MapType.ClassMap, classMap: new ClassMap(source, destination, constructorProperties, initializerProperties));

    public static Map Collection(
        string source,
        string destination,
        CollectionKind sourceKind,
        CollectionKind destinationKind,
        string sourceItem,
        string destinationItem,
        bool isItemsEquals,
        bool isItemsHasImpicitConversion)
        => new(MapType.CollectionMap, collectionMap: new CollectionMap(
            source,
            destination,
            sourceKind,
            destinationKind,
            sourceItem,
            destinationItem,
            isItemsEquals,
            isItemsHasImpicitConversion));

    public static Map Enum(string source, string destination, ImmutableArray<EnumFieldMap> fields)
        => new(MapType.EnumMap, enumMap: new EnumMap(source, destination, fields));

    public static Map Configured(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties,
        ImmutableArray<NameTypePair> arguments,
        ImmutableArray<ConfiguredMapMockMethod> mockMethods)
        => new(MapType.ConfiguredMap, configuredMap: new ConfiguredMap(
            source,
            destination,
            constructorProperties,
            initializerProperties,
            arguments,
            mockMethods));

    public static Map User(string source, string destination)
        => new(MapType.UserMap, userMap: new UserMap(source, destination));

    public static Map Error(ITypeSymbol source, ITypeSymbol destination, Diagnostic diagnostic)
        => new(MapType.Error, errorMap: new ErrorMap(source.ToNotNullableString(), destination.ToNotNullableString(), diagnostic));

    public static Map PotentialError(ITypeSymbol source, ITypeSymbol destination, Diagnostic diagnostic)
        => new(
            MapType.PotentialError,
            potentialErrorMap: new PotentialErrorMap(source.ToNotNullableString(), destination.ToNotNullableString(), diagnostic));

    private Map(
        MapType type,
        ClassMap classMap = default,
        CollectionMap collectionMap = default,
        EnumMap enumMap = default,
        ConfiguredMap configuredMap = default,
        UserMap userMap = default,
        ErrorMap errorMap = default,
        PotentialErrorMap potentialErrorMap = default)
    {
        ClassMap = classMap;
        CollectionMap = collectionMap;
        EnumMap = enumMap;
        ConfiguredMap = configuredMap;
        UserMap = userMap;
        ErrorMap = errorMap;
        PotentialErrorMap = potentialErrorMap;
        Type = type;
    }
}
