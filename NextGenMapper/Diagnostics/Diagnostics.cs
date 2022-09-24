using Microsoft.CodeAnalysis;
using System.Collections.Specialized;

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
    }
}
