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

        public static readonly DiagnosticDescriptor ObjectCreationExpressionNotFoundError = new(
            id: "NGM003",
            title: "Object creation expression was not found",
            messageFormat: "Object creation expression was not found in custom mapping method ({0} -> {1}), custom mapping method must contains single return statement with constructor invocation, like \"return new {1}()\"",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UndefinedCollectionTypeError = new(
            id: "NGM004",
            title: "Mapped collection type was undefined",
            messageFormat: "Mapped collection type was undefined, supported collection types: Array, List<T>, ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnmappedEnumValueError = new(
            id: "NGM005",
            title: "Enum value was unmapped",
            messageFormat: "Value {0}.{2} cant be mapped to {1} enum, make sure that {2} has value with same name or constant value",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ParameterNotFoundError = new(
            id: "NGM006",
            title: "Custom mapping method must have single parameter",
            messageFormat: "Custom mapping method must have single parameter with type of mapping source",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor ReturnTypeNotFoundError = new(
            id: "NGM007",
            title: "Custom mapping method must have return type",
            messageFormat: "Custom mapping method must have return type with type of mapping destination and can`t be void",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
