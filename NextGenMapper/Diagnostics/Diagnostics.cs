using Microsoft.CodeAnalysis;

namespace NextGenMapper
{
    public static class Diagnostics
    {
        public static readonly DiagnosticDescriptor CircularReferenceError = new(
            id: "NGM001",
            title: "Circular reference was found",
            messageFormat: "Circular reference {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ConstructorNotFoundError = new(
            id: "NGM002",
            title: "Constructor was not found",
            messageFormat: "Constructor for mapping from {0} to {1} was not found, make sure that the {1} has a public constructor whose parameters correspond to the properties of the {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UndefinedCollectionTypeError = new(
            id: "NGM003",
            title: "Mapped collection type was undefined",
            messageFormat: "Mapped collection type was undefined, supported collection types: Array, List<T>, ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnmappedEnumValueError = new(
            id: "NGM004",
            title: "Enum value was unmapped",
            messageFormat: "Value {0}.{2} cant be mapped to {1} enum, make sure that {2} has value with same name or constant value",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapWithMethodWithoutArgumentsError = new(
            id: "NGM005",
            title: "'MapWith' method without arguments",
            messageFormat: "Method 'MapWith' must be called at least with one argument. Pass arguments to 'MapWith' method or use 'Map' method.",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapWithArgumentMustBeNamed = new(
            id: "NGM006",
            title: "All arguments for method 'MapWith' must be named",
            messageFormat: "All arguments for method 'MapWith' must be named",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapWithNotSupportedForMapWith = new(
            id: "NGM007",
            title: "Not supported for 'MapWith' method",
            messageFormat: "Customized mapping from {0} to {1} with 'MapWith' not supported, 'MapWith' method supports only class to class mapping",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MappingFunctionNotFound = new(
            id: "NGM008",
            title: "Mapping function was not found",
            messageFormat: "Mapping function for mapping '{0}' to '{1}' was not found",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MappingFunctionForPropertiesNotFound = new(
            id: "NGM009",
            title: "Mapping function for properties was not found",
            messageFormat: "Mapping function for mapping '{0}.{1}' of type '{2}' to '{3}.{4}' of type '{5}' was not found",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateMapWithFunction = new(
            id: "NGM010",
            title: "Duplicate 'MapWith' function",
            messageFormat: "Can`t use two different custom mapping functions 'MapWith' with same signatures for mapping from {0} to {1}, to use multiple custom mapping functions, they must have a different number of parameters and/or their type",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor SuitablePropertyNotFoundInSource = new(
            id: "NGM011",
            title: "Suitable property for mapping was not found",
            messageFormat: "Source class {0} has no properties suitable for mapping to {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor SuitablePropertyNotFoundInDestination = new(
            id: "NGM012",
            title: "Suitable property for mapping was not found",
            messageFormat: "Destination class {1} has no properties suitable for mapping from {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoPropertyMatches = new(
            id: "NGM013",
            title: "No property matches",
            messageFormat: "None of the properties of class {0} match the properties of class {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor StructNotSupported = new(
            id: "NGM014",
            title: "Struct not supported",
            messageFormat: "Mapping from/to struct is not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MappedTypesEquals = new(
            id: "NGM015",
            title: "Mapped types are equals",
            messageFormat: "Types for mapping must not be equals",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor TypesKindsMismatch = new(
            id: "NGM016",
            title: "Types kinds does not match",
            messageFormat: "Error when mapping {0} to {1}, mapping from {2} to {3} is not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapMethodMustBeExtension = new(
            id: "NGM017",
            title: "Map method must be extension",
            messageFormat: "Custom map method must be extension method",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapMethodMustBeGeneric = new(
            id: "NGM018",
            title: "Map method must be generic with single type parameter",
            messageFormat: "Custom map method must be generic method with single type parameter",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapMethodMustNotReturnVoid = new(
            id: "NGM019",
            title: "Map method must not return void",
            messageFormat: "Custom map method must not return void, map method must return destination type",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor MapMethodMustBeInternal = new(
            id: "NGM020",
            title: "Map method must be internal",
            messageFormat: "Custom map method must have 'internal' access modifier",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
