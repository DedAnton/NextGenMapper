using NextGenMapper.Mapping.Maps.Models;

namespace NextGenMapper.CodeGeneration;
internal static class CodeGenerationHelper
{
    public static bool IsInterface(this CollectionKind type) => type is CollectionKind.IEnumerable or CollectionKind.ICollection
        or CollectionKind.IList or CollectionKind.IReadOnlyCollection or CollectionKind.IReadOnlyList;

    public static bool IsListInterface(this CollectionKind type)
        => type is CollectionKind.IReadOnlyCollection or CollectionKind.IReadOnlyList;

    public static bool IsArrayInterface(this CollectionKind type)
        => type is CollectionKind.IEnumerable or CollectionKind.ICollection or CollectionKind.IList;

    public static bool IsArray(this CollectionKind type) => type is CollectionKind.Array;

    public static bool IsList(this CollectionKind type) => type is CollectionKind.List;

    public static bool IsImmutableArray(this CollectionKind type) => type is CollectionKind.ImmutableArray;

    public static bool IsImmutableList(this CollectionKind type) => type is CollectionKind.ImmutableList;

    public static bool IsIImmutableList(this CollectionKind type) => type is CollectionKind.IImmutableList;
}
