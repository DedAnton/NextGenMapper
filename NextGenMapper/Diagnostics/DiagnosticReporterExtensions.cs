using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    public static class DiagnosticReporterExtensions
    {
        public static void ReportCircularReferenceError(this DiagnosticReporter diagnosticReporter, IEnumerable<Location> locations, IEnumerable<ITypeSymbol> circularReferencedTypes)
        {
            var circularReferenceString = string.Join(" => ", circularReferencedTypes.Select(x => x.Name));
            var diagnostic = Diagnostic.Create(Diagnostics.CircularReferenceError, locations.FirstOrDefault(), locations, circularReferenceString);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportConstructorNotFoundError(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ConstructorNotFoundError, location, from, to);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportUndefinedCollectionTypeError(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.UndefinedCollectionTypeError, location);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportUnmappedEnumValueError(this DiagnosticReporter diagnosticReporter, IEnumerable<Location> locations, ITypeSymbol from, ITypeSymbol to, string notMappedValue)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.UnmappedEnumValueError, locations.FirstOrDefault(), locations, from, to, notMappedValue);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportMapWithMethodWithoutArgumentsError(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.MapWithMethodWithoutArgumentsError, location);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportToManyArgumentsForMapWithError(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ToManyArgumentsForMapWithError, location);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportMapWithNotSupportedForEnums(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.MapWithNotSupportedForEnums, location);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportMappingFunctionNotFound(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.MappingFunctionNotFound, location, from, to);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportMappingFunctionForPropertyNotFound(
            this DiagnosticReporter diagnosticReporter, 
            Location location, 
            ITypeSymbol fromContainedType,
            string fromProperty,
            ITypeSymbol fromType,
            ITypeSymbol toContainedType,
            string toProperty,
            ITypeSymbol toType)
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.MappingFunctionForPropertiesNotFound, 
                location, 
                fromContainedType, 
                fromProperty,
                fromType,
                toContainedType,
                toProperty,
                toType);

            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportMapWithBetterFunctionMemberNotFound(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.MapWithBetterFunctionMemberNotFound, location, from, to);
            diagnosticReporter.Report(diagnostic);
        }
    }
}
