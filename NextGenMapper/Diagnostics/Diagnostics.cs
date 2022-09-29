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

        public static readonly DiagnosticDescriptor ToManyArgumentsForMapWithError = new(
            id: "NGM006",
            title: "To many arguments for 'MapWith' method",
            messageFormat: "Method 'MapWith' must not pass all arguments for each parameter. At least one argument must not be passed",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapWithNotSupportedForEnums = new(
            id: "NGM007",
            title: "'MapWith' method not supported enums",
            messageFormat: "Method 'MapWith' must not be colled for enums mapping. For matching enums values set the integer value in enum\r\nExample:\r\nenum SourceEnum\r\n{\r\n    A,\r\n    B = 1\r\n}\r\nenum DestinationEnum\r\n{\r\n    A,\r\n    C = 1\r\n}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MappingFunctionNotFound = new(
            id: "NGM008",
            title: "Mapping function was not found",
            messageFormat: "Mapping function for mapping '{0}' to '{1}' was not found. Add custom mapping function for mapper\r\nExample:\r\n\r\nnamespace NextGenMapper;\r\n\r\ninternal static partial class Mapper\r\n{{\r\n    internal static {1} Map<To>(this {0} source) \r\n        => throw new NotImplementedException();\r\n}}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MappingFunctionForPropertiesNotFound = new(
            id: "NGM009",
            title: "Mapping function for properties was not found",
            messageFormat: "Mapping function for mapping '{0}.{1}' of type '{2}' to '{3}.{4}' of type '{5}' was not found. Add custom mapping function for mapper\r\nExample:\r\n\r\nnamespace NextGenMapper;\r\n\r\ninternal static partial class Mapper\r\n{{\r\n    internal static {5} Map<To>(this {2} source) \r\n        => throw new NotImplementedException();\r\n}}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MapWithBetterFunctionMemberNotFound = new(
            id: "NGM010",
            title: "Better function member can not be selected for 'MapWith'",
            messageFormat: "Can not map '{0}' to {1}. Better function member can not be selected for 'MapWith'. Multiple functions have the same signatures (number and type of parameters)",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
