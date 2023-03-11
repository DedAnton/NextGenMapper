using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps.Models;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct Map : IEquatable<Map>
{
    public ClassMap ClassMap { get; }
    public CollectionMap CollectionMap { get; }
    public EnumMap EnumMap { get; }
    public ConfiguredMap ConfiguredMap { get; }
    public UserMap UserMap { get; }
    public ProjectionMap ProjectionMap { get; }
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
        bool isSourceItemNullable,
        bool isDestinationItemNullable,
        bool isItemsEquals,
        bool isItemsHasImpicitConversion)
        => new(MapType.CollectionMap, collectionMap: new CollectionMap(
            source,
            destination,
            sourceKind,
            destinationKind,
            sourceItem,
            destinationItem,
            isSourceItemNullable,
            isDestinationItemNullable,
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
        ImmutableArray<ConfiguredMapMockMethod> mockMethods,
        bool isSuccess)
        => new(MapType.ConfiguredMap, configuredMap: new ConfiguredMap(
            source,
            destination,
            constructorProperties,
            initializerProperties,
            arguments,
            mockMethods,
            isSuccess));

    public static Map User(string source, string destination)
        => new(MapType.UserMap, userMap: new UserMap(source, destination));

    public static Map Projection(
        string source,
        string destination,
        ImmutableArray<PropertyMap> properties)
        => new(MapType.ProjectionMap, projectionMap: new ProjectionMap(source, destination, properties));

    public static Map Error(ITypeSymbol source, ITypeSymbol destination, Diagnostic diagnostic)
        => new(MapType.Error, errorMap: new ErrorMap(source.ToNotNullableString(), destination.ToNotNullableString(), diagnostic));

    public static Map PotentialError(ITypeSymbol source, ITypeSymbol destination, Diagnostic diagnostic)
        => new(
            MapType.PotentialError,
            potentialErrorMap: new PotentialErrorMap(source.ToNotNullableString(), destination.ToNotNullableString(), diagnostic));

    public bool Equals(Map other)
        => Type == other.Type
        && Type switch
        {
            MapType.Error => ErrorMap.Equals(other.ErrorMap),
            MapType.PotentialError => PotentialErrorMap.Equals(other.PotentialErrorMap),
            MapType.ClassMap => ClassMap.Equals(other.ClassMap),
            MapType.CollectionMap => CollectionMap.Equals(other.CollectionMap),
            MapType.EnumMap => EnumMap.Equals(other.EnumMap),
            MapType.ConfiguredMap => ConfiguredMap.Equals(other.ConfiguredMap),
            MapType.UserMap => UserMap.Equals(other.UserMap),
            _ => throw new NotImplementedException()
        };

    private Map(
        MapType type,
        ClassMap classMap = default,
        CollectionMap collectionMap = default,
        EnumMap enumMap = default,
        ConfiguredMap configuredMap = default,
        UserMap userMap = default,
        ProjectionMap projectionMap = default,
        ErrorMap errorMap = default,
        PotentialErrorMap potentialErrorMap = default)
    {
        ClassMap = classMap;
        CollectionMap = collectionMap;
        EnumMap = enumMap;
        ConfiguredMap = configuredMap;
        UserMap = userMap;
        ProjectionMap = projectionMap;
        ErrorMap = errorMap;
        PotentialErrorMap = potentialErrorMap;
        Type = type;
    }
}
