//using Microsoft.CodeAnalysis;
//using System.Collections.Generic;
//using System.Linq;

//namespace NextGenMapper
//{
//    public static class DiagnosticReporterExtensions
//    {
//        public static void ReportCircularReferenceError(this DiagnosticReporter diagnosticReporter, Location location, IEnumerable<ITypeSymbol> circularReferencedTypes)
//        {
//            var diagnostic = Diagnostics.CircularReferenceError(location, circularReferencedTypes);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportConstructorNotFoundError(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.ConstructorNotFoundError(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportUndefinedCollectionTypeError(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostics.UndefinedCollectionTypeError(location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportUnmappedEnumValueError(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to, string notMappedValue)
//        {
//            var diagnostic = Diagnostics.UnmappedEnumValue(location, from, to, notMappedValue);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMapWithMethodWithoutArgumentsError(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.MapWithMethodWithoutArgumentsError, location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMapWithArgumentMustBeNamed(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.MapWithArgumentMustBeNamed, location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportNotSupportetForMapWith(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.MapWithNotSupportedForMapWith(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMappingFunctionNotFound(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.MappingFunctionNotFound, location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMappingFunctionForPropertyNotFound(
//            this DiagnosticReporter diagnosticReporter, 
//            Location location, 
//            ITypeSymbol fromContainedType,
//            string fromProperty,
//            ITypeSymbol fromType,
//            ITypeSymbol toContainedType,
//            string toProperty,
//            ITypeSymbol toType)
//        {
//            var diagnostic = Diagnostic.Create(
//                Diagnostics.MappingFunctionForPropertiesNotFound, 
//                location, 
//                fromContainedType, 
//                fromProperty,
//                fromType,
//                toContainedType,
//                toProperty,
//                toType);

//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportDuplicateMapWithFunction(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.DuplicateMapWithFunction, location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportSuitablePropertyNotFoundInSource(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.SuitablePropertyNotFoundInSource(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportSuitablePropertyNotFoundInDestination(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.SuitablePropertyNotFoundInDestination(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportNoPropertyMatches(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.NoPropertyMatches(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportStructNotSupported(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.StructNotSupportedDescriptor, location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMappedTypesEquals(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostics.MappedTypesEquals(location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportTypesKindsMismatch(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.TypesKindsMismatch, location, from, to, from.TypeKind, to.TypeKind);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMapMethodMustBeExtension(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostics.MapMethodMustBeExtension(location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMapMethodMustBeGeneric(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostics.MapMethodMustBeGeneric(location);
//            diagnosticReporter.Report(diagnostic);
//        }
        
//        public static void ReportMapMethodMustNotReturnVoid(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostics.MapMethodMustNotReturnVoid(location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMapMethodMustBeInternal(this DiagnosticReporter diagnosticReporter, Location location)
//        {
//            var diagnostic = Diagnostic.Create(Diagnostics.MapMethodMustBeInternal, location);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportMappedTypesHasImplicitConversion(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
//        {
//            var diagnostic = Diagnostics.MappedTypesHasImplicitConversion(location, from, to);
//            diagnosticReporter.Report(diagnostic);
//        }

//        public static void ReportPossibleNullReference(
//            this DiagnosticReporter diagnosticReporter,
//            Location location,
//            ITypeSymbol fromContainedType,
//            string fromProperty,
//            ITypeSymbol fromType,
//            ITypeSymbol toContainedType,
//            string toProperty,
//            ITypeSymbol toType)
//        {
//            var diagnostic = Diagnostics.PossibleNullReference(
//                location,
//                fromContainedType,
//                fromProperty,
//                fromType,
//                toContainedType,
//                toProperty,
//                toType);

//            diagnosticReporter.Report(diagnostic);
//        }
//    }
//}
