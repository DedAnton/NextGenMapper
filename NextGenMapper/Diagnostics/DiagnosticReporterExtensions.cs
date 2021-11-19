using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper
{
    public static class DiagnosticReporterExtensions
    {
        public static void ReportCircularReferenceError(this DiagnosticReporter diagnosticReporter, ImmutableArray<Location> locations, IEnumerable<ITypeSymbol> circularReferencedTypes)
        {
            var circularReferenceString = string.Join(" => ", circularReferencedTypes.Select(x => x.Name));
            var diagnostic = Diagnostic.Create(Diagnostics.CircularReferenceError, locations.FirstOrDefault(), locations, circularReferenceString);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportConstructorNotFoundError(this DiagnosticReporter diagnosticReporter, ImmutableArray<Location> locations, ITypeSymbol from, ITypeSymbol to)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ConstructorNotFoundError, locations.FirstOrDefault(), locations, from, to);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportObjectCreationExpressionNotFoundError(this DiagnosticReporter diagnosticReporter, Location location, ITypeSymbol from, ITypeSymbol to)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ObjectCreationExpressionNotFoundError, location, additionalLocations: null, from, to);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportUndefinedCollectionTypeError(this DiagnosticReporter diagnosticReporter, ImmutableArray<Location> locations)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.UndefinedCollectionTypeError, locations.FirstOrDefault(), locations);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportUnmappedEnumValueError(this DiagnosticReporter diagnosticReporter, ImmutableArray<Location> locations, ITypeSymbol from, ITypeSymbol to, string notMappedValue)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.UnmappedEnumValueError, locations.FirstOrDefault(), locations, from, to, notMappedValue);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportParameterNotFoundError(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ParameterNotFoundError, location, additionalLocations: null);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportReturnTypeNotFoundError(this DiagnosticReporter diagnosticReporter, Location location)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.ReturnTypeNotFoundError, location, additionalLocations: null);
            diagnosticReporter.Report(diagnostic);
        }

        public static void ReportPropertyAndParameterHasDifferentNamesError(this DiagnosticReporter diagnosticReporter, ImmutableArray<Location> locations, ITypeSymbol destinationType, string propertyName)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.PropertyAndParameterHasDifferentNamesError, locations.FirstOrDefault(), locations, destinationType, propertyName);
            diagnosticReporter.Report(diagnostic);
        }
    }
}
