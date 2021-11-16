using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class EnumMapDesigner
    {
        private readonly DiagnosticReporter _diagnosticReporter;

        public EnumMapDesigner(DiagnosticReporter diagnosticReporter)
        {
            _diagnosticReporter = diagnosticReporter;
        }

        public EnumMap DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            if (from.GetFirstDeclaration() is not EnumDeclarationSyntax fromDeclaration
                || to.GetFirstDeclaration() is not EnumDeclarationSyntax toDeclaration)
            {
                //TODO: research: do this real case?
                throw new ArgumentException("enum must have declaration");
            }
            var fromFields = fromDeclaration.GetFields();
            var toFields = toDeclaration.GetFields();

            var valuesMappings = new List<MemberMap>();
            foreach (var fromField in fromFields)
            {
                var byName = toFields.FirstOrDefault(x => x.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant());
                var byValue = toFields.FirstOrDefault(x => x.Value != null && fromField.Value != null
                    && x.Value == fromField.Value);
                var toField = byValue ?? byName;
                if (toField is not null)
                {
                    valuesMappings.Add(MemberMap.EnumField(from, fromField, to, toField));
                }
                else
                {
                    _diagnosticReporter.ReportUnmappedEnumValueError(from.Locations, from, to, fromField.Name);
                }
                //TODO: add warning diagnostic if 'to' has unmapped values 
            }

            return new EnumMap(from, to, valuesMappings);
        }
    }
}
