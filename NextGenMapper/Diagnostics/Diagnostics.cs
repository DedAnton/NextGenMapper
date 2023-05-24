using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    public static class Diagnostics
    {
        public static Diagnostic CircularReferenceError(Location location, IEnumerable<ITypeSymbol> circularReferencedTypes)
        {
            var circularReferencePath = string.Join(" => ", circularReferencedTypes.Select(x => x.ToDisplayString()));

            return Diagnostic.Create(CircularReferenceErrorDiscriptor, location, circularReferencePath);
        }
        public static readonly DiagnosticDescriptor CircularReferenceErrorDiscriptor = new(
            id: "NGM001",
            title: "Circular reference was found",
            messageFormat: "Circular reference {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ConstructorNotFoundError(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(ConstructorNotFoundErrorDiscriptor, location, from, to);
        public static readonly DiagnosticDescriptor ConstructorNotFoundErrorDiscriptor = new(
            id: "NGM002",
            title: "Constructor was not found",
            messageFormat: "Constructor for mapping from {0} to {1} was not found, make sure that the {1} has a public constructor whose parameters correspond to the properties of the {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic UndefinedCollectionTypeError(Location location)
            => Diagnostic.Create(UndefinedCollectionTypeErrorDescriptor, location);
        public static readonly DiagnosticDescriptor UndefinedCollectionTypeErrorDescriptor = new(
            id: "NGM003",
            title: "Mapped collection type was undefined",
            messageFormat: "Mapped collection type was undefined, supported collection types: T[], List<T>, ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ImmutableList<T>, ImmutableArray<T>, IImmutableList<T>",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic UnmappedEnumValue(Location location, ITypeSymbol from, ITypeSymbol to, string notMappedValue)
            => Diagnostic.Create(UnmappedEnumValueDescriptor, location, from, to, notMappedValue);
        public static readonly DiagnosticDescriptor UnmappedEnumValueDescriptor = new(
            id: "NGM004",
            title: "Enum value was unmapped",
            messageFormat: "Enum value {0}.{2} can not be mapped to {1}, make sure that {1} has value with same name or constant value",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic MapWithMethodWithoutArgumentsError(Location location)
            => Diagnostic.Create(MapWithMethodWithoutArgumentsErrorDescriptor, location);
        public static readonly DiagnosticDescriptor MapWithMethodWithoutArgumentsErrorDescriptor = new(
            id: "NGM005",
            title: "'MapWith' method without arguments",
            messageFormat: "Method 'MapWith' must be called at least with one argument. Pass arguments to 'MapWith' method or use 'Map' method.",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapWithArgumentMustBeNamed(Location location)
            => Diagnostic.Create(MapWithArgumentMustBeNamedDescriptor, location);
        public static readonly DiagnosticDescriptor MapWithArgumentMustBeNamedDescriptor = new(
            id: "NGM006",
            title: "All arguments for method 'MapWith' must be named",
            messageFormat: "All arguments for method 'MapWith' must be named",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapWithNotSupportedForMapWith(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(MapWithNotSupportedForMapWithDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor MapWithNotSupportedForMapWithDescriptor = new(
            id: "NGM007",
            title: "Not supported for 'MapWith' method",
            messageFormat: "Customized mapping from {0} to {1} with 'MapWith' not supported, 'MapWith' method supports only class to class mapping",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MappingFunctionNotFound(Location location, string from, string to)
            => Diagnostic.Create(MappingFunctionNotFoundDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor MappingFunctionNotFoundDescriptor = new(
            id: "NGM008",
            title: "Mapping function was not found",
            messageFormat: "Mapping function for mapping '{0}' to '{1}' was not found",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MappingFunctionForPropertiesNotFound(
            Location location,
            string fromContainedType,
            string fromProperty,
            string fromType,
            string toContainedType,
            string toProperty,
            string toType)
            => Diagnostic.Create(
                MappingFunctionForPropertiesNotFoundDescriptor, 
                location, 
                fromContainedType, 
                fromProperty, 
                fromType, 
                toContainedType, 
                toProperty, 
                toType);
        public static readonly DiagnosticDescriptor MappingFunctionForPropertiesNotFoundDescriptor = new(
            id: "NGM009",
            title: "Mapping function for properties was not found",
            messageFormat: "Mapping function for mapping '{0}.{1}' of type '{2}' to '{3}.{4}' of type '{5}' was not found",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic DuplicateMapWithFunction(Location location, string from, string to)
            => Diagnostic.Create(DuplicateMapWithFunctionDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor DuplicateMapWithFunctionDescriptor = new(
            id: "NGM010",
            title: "Duplicate 'MapWith' function",
            messageFormat: "Can`t use two different custom mapping functions 'MapWith' with same signatures for mapping from {0} to {1}, to use multiple custom mapping functions, they must have a different number of parameters and/or their type",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic SuitablePropertyNotFoundInSource(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(SuitablePropertyNotFoundInSourceDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor SuitablePropertyNotFoundInSourceDescriptor = new(
            id: "NGM011",
            title: "Suitable property for mapping was not found",
            messageFormat: "Source class {0} has no properties suitable for mapping to {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic SuitablePropertyNotFoundInDestination(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(SuitablePropertyNotFoundInDestinationDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor SuitablePropertyNotFoundInDestinationDescriptor = new(
            id: "NGM012",
            title: "Suitable property for mapping was not found",
            messageFormat: "Destination class {1} has no properties suitable for mapping from {0}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic NoPropertyMatches(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(NoPropertyMatchesDiscriptor, location, from, to);
        public static readonly DiagnosticDescriptor NoPropertyMatchesDiscriptor = new(
            id: "NGM013",
            title: "No property matches",
            messageFormat: "None of the properties of class {0} match the properties of class {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic StructNotSupported(Location location) 
            => Diagnostic.Create(StructNotSupportedDescriptor, location);
        public static readonly DiagnosticDescriptor StructNotSupportedDescriptor = new(
            id: "NGM014",
            title: "Struct not supported",
            messageFormat: "Mapping from/to struct is not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic MappedTypesEquals(Location location) 
            => Diagnostic.Create(MappedTypesEqualsDescriptor, location);
        public static readonly DiagnosticDescriptor MappedTypesEqualsDescriptor = new(
            id: "NGM015",
            title: "Mapped types are equals",
            messageFormat: "Types for mapping must not be equals",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic TypesKindsMismatch(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(TypesKindsMismatchDescriptor, location, from, to, from.TypeKind, to.TypeKind);
        public static readonly DiagnosticDescriptor TypesKindsMismatchDescriptor = new(
            id: "NGM016",
            title: "Types kinds does not match",
            messageFormat: "Error when mapping {0} to {1}, mapping from {2} to {3} is not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapMethodMustBeExtension(Location location)
            => Diagnostic.Create(MapMethodMustBeExtensionDescriptor, location);
        public static readonly DiagnosticDescriptor MapMethodMustBeExtensionDescriptor = new(
            id: "NGM017",
            title: "Map method must be extension",
            messageFormat: "Custom map method must be extension method",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapMethodMustBeGeneric(Location location)
            => Diagnostic.Create(MapMethodMustBeGenericDescriptor, location);
        public static readonly DiagnosticDescriptor MapMethodMustBeGenericDescriptor = new(
            id: "NGM018",
            title: "Map method must be generic with single type parameter",
            messageFormat: "Custom map method must be generic method with single type parameter",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapMethodMustNotReturnVoid(Location location)
            => Diagnostic.Create(MapMethodMustNotReturnVoidDescriptor, location);
        public static readonly DiagnosticDescriptor MapMethodMustNotReturnVoidDescriptor = new(
            id: "NGM019",
            title: "Map method must not return void",
            messageFormat: "Custom map method must not return void, map method must return destination type",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapMethodMustBeInternal(Location location)
            => Diagnostic.Create(MapMethodMustBeInternalDescriptor, location);
        public static readonly DiagnosticDescriptor MapMethodMustBeInternalDescriptor = new(
            id: "NGM020",
            title: "Map method must be internal",
            messageFormat: "Custom map method must have 'internal' access modifier",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic MappedTypesHasImplicitConversion(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(MappedTypesHasImplicitConversionDescriptor, location, from, to);
        private static readonly DiagnosticDescriptor MappedTypesHasImplicitConversionDescriptor = new(
            id: "NGM021",
            title: "Mapped types has implicit conversion",
            messageFormat: "Mapped types has implicit conversion from {0} to {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public static Diagnostic PossiblePropertyNullReference(
            Location location, 
            ITypeSymbol fromContainedType,
            string fromProperty,
            ITypeSymbol fromType,
            ITypeSymbol toContainedType,
            string toProperty,
            ITypeSymbol toType)
            => Diagnostic.Create(PossiblePropertyNullReferenceDescriptor, location, fromContainedType, fromProperty, fromType, toContainedType, toProperty, toType);
        public static readonly DiagnosticDescriptor PossiblePropertyNullReferenceDescriptor = new(
            id: "NGM022",
            title: "Possible null reference",
            messageFormat: "Possible null reference exception when mapping '{0}.{1}' of type '{2}' to '{3}.{4}' of type '{5}'",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic PossibleNullReference(
            Location location,
            ITypeSymbol source,
            ITypeSymbol destination)
            => Diagnostic.Create(PossibleNullReferenceDescriptor, location, source, destination);
        public static readonly DiagnosticDescriptor PossibleNullReferenceDescriptor = new(
            id: "NGM023",
            title: "Possible null reference",
            messageFormat: "Possible null reference exception when mapping '{0}' to {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MapperInternalError(Location location, System.Exception exception)
            => Diagnostic.Create(MapperInternalErrorDescriptor, location, exception.GetType().Name, exception.Message);
        public static readonly DiagnosticDescriptor MapperInternalErrorDescriptor = new(
            id: "NGM024",
            title: "Mapped internal error",
            messageFormat: "An error occurred while mapping, this is an internal mapper error that was not your fault, please create an issue on github, exception type: {0}, message: {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MappingNotSupported(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(MappingNotSupportedDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor MappingNotSupportedDescriptor = new(
            id: "NGM025",
            title: "Mapping not supported",
            messageFormat: "Mapping from {0} to {1} not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic PropertiesTypesMustBeEquals(Location location, IPropertySymbol fromProperty, IPropertySymbol toProperty)
            => Diagnostic.Create(PropertiesTypesMustBeEqualsDescriptor, location, fromProperty.ContainingType, fromProperty.Name, fromProperty.Type, toProperty.ContainingType, toProperty.Name, toProperty.Type);
        public static readonly DiagnosticDescriptor PropertiesTypesMustBeEqualsDescriptor = new(
            id: "NGM026",
            title: "Properties types must be equal for projection",
            messageFormat: "Projection from '{0}.{1}' of type '{2}' to '{3}.{4}' of type '{5}' is not possible. Types must be equal",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ProjectWithMethodWithoutArgumentsError(Location location)
            => Diagnostic.Create(ProjectWithMethodWithoutArgumentsErrorDescriptor, location);
        public static readonly DiagnosticDescriptor ProjectWithMethodWithoutArgumentsErrorDescriptor = new(
            id: "NGM027",
            title: "'ProjectWith' method without arguments",
            messageFormat: "Method 'ProjectWith' must be called at least with one argument. Pass arguments to 'ProjectWith' method or use 'Project' method.",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ProjectWithArgumentMustBeNamed(Location location)
            => Diagnostic.Create(ProjectWithArgumentMustBeNamedDescriptor, location);
        public static readonly DiagnosticDescriptor ProjectWithArgumentMustBeNamedDescriptor = new(
            id: "NGM028",
            title: "All arguments for method 'ProjectWith' must be named",
            messageFormat: "All arguments for method 'ProjectWith' must be named",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic DuplicateProjectWithFunction(Location location, string from, string to)
            => Diagnostic.Create(DuplicateProjectWithFunctionDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor DuplicateProjectWithFunctionDescriptor = new(
            id: "NGM029",
            title: "Duplicate 'ProjectWith' function",
            messageFormat: "Can`t use two different custom mapping functions 'ProjectWith' with same signatures for mapping from {0} to {1}, to use multiple custom mapping functions, they must have a different number of parameters and/or their type",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic CollectionItemTypeNotFoundError(Location location, ITypeSymbol collectionType)
            => Diagnostic.Create(CollectionItemTypeNotFoundErrorDescriptor, location, collectionType);
        public static readonly DiagnosticDescriptor CollectionItemTypeNotFoundErrorDescriptor = new(
            id: "NGM030",
            title: "Collection item type was not found",
            messageFormat: "Can not find item type for collection '{0}'",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic UnsupportedEnumType(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(UnsupportedEnumTypeDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor UnsupportedEnumTypeDescriptor = new(
            id: "NGM031",
            title: "Enum has unsupported underlying type",
            messageFormat: "Can not map from '{0}' to '{1}'. Supported underlying types for enum: sbyte, byte, short, ushort, int, uint, long",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic DefaultConstructorNotFoundError(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(DefaultConstructorNotFoundErrorDiscriptor, location, from, to);
        public static readonly DiagnosticDescriptor DefaultConstructorNotFoundErrorDiscriptor = new(
            id: "NGM032",
            title: "Default constructor was not found for projection",
            messageFormat: "Default constructor for projection from {0} to {1} was not found, make sure that the {1} has a public default constructor (constructor without parameters)",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ProjectedTypesEquals(Location location)
            => Diagnostic.Create(ProjectedTypesEqualsDescriptor, location);
        public static readonly DiagnosticDescriptor ProjectedTypesEqualsDescriptor = new(
            id: "NGM033",
            title: "Projected types are equals",
            messageFormat: "Types for projection must not be equals",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ProjectedTypesHasImplicitConversion(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(ProjectedTypesHasImplicitConversionDescriptor, location, from, to);
        private static readonly DiagnosticDescriptor ProjectedTypesHasImplicitConversionDescriptor = new(
            id: "NGM034",
            title: "Projected types has implicit conversion",
            messageFormat: "Projected types has implicit conversion from {0} to {1}",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ProjectionNotSupported(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(ProjectionNotSupportedDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor ProjectionNotSupportedDescriptor = new(
            id: "NGM035",
            title: "Projection not supported",
            messageFormat: "Projection from {0} to {1} not supported",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic ConfiguredProjectionNotSupported(Location location, ITypeSymbol from, ITypeSymbol to)
            => Diagnostic.Create(ConfiguredProjectionNotSupportedDescriptor, location, from, to);
        public static readonly DiagnosticDescriptor ConfiguredProjectionNotSupportedDescriptor = new(
            id: "NGM036",
            title: "Configured projection not supported",
            messageFormat: "Configured projection from {0} to {1} with 'ProjectionWith' not supported. 'ProjectionWith' method supports only class to class projection",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic PropertyNotFoundForCoonfiguredMappingArgument(Location location, ITypeSymbol from, ITypeSymbol to, string name)
            => Diagnostic.Create(PropertyNotFoundForCoonfiguredMappingArgumentDescriptor, location, from, to, name);
        public static readonly DiagnosticDescriptor PropertyNotFoundForCoonfiguredMappingArgumentDescriptor = new(
            id: "NGM037",
            title: "Property was not found for configured mapping argument",
            messageFormat: "Error when mapping '{0}' to '{1}'. Method 'MapWith' does not contains parameter '{2}'. Property named '{2}' was not found in type '{1}'",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic PropertyNotFoundForCoonfiguredProjectionArgument(Location location, ITypeSymbol from, ITypeSymbol to, string name)
            => Diagnostic.Create(PropertyNotFoundForCoonfiguredProjectionArgumentDescriptor, location, from, to, name);
        public static readonly DiagnosticDescriptor PropertyNotFoundForCoonfiguredProjectionArgumentDescriptor = new(
            id: "NGM038",
            title: "Property was not found for configured projection argument",
            messageFormat: "Error when projecting '{0}' to '{1}'. Method 'MapWith' does not contains parameter '{2}'. Property named '{2}' was not found in type '{1}'",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static Diagnostic MultipleInitializationError(
            Location location, 
            ITypeSymbol from, 
            ITypeSymbol to, 
            string parameterName, 
            string initializedPropertiesList)
            => Diagnostic.Create(MultipleInitializationErrorDescriptor, location, from, to, parameterName, initializedPropertiesList);
        public static readonly DiagnosticDescriptor MultipleInitializationErrorDescriptor = new(
            id: "NGM039",
            title: "Constructor parameter initialize multiple properties",
            messageFormat: "Constructor of type '{1}' has parameter '{2}' that initialize multiple properties: {3}. This is not allowed because it may lead to unexpected behavior",
            category: "NextGenMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
