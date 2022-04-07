using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;

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
                //TODO: research: is this real case?
                throw new ArgumentException("enum must have declaration");
            }

            var fromFields = fromDeclaration.GetFields();
            var toFields = toDeclaration.GetFields();

            var valuesMappings = new List<MemberMap>();
            foreach (var fromField in fromFields)
            {
                EnumField? toField = null;
                foreach (var field in toFields)
                {
                    //field.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant()
                    if (field.Value != null && fromField.Value != null
                        && field.Value == fromField.Value)
                    {
                        toField = field;
                        break;
                    }
                }
                if (toField == null)
                {
                    foreach (var field in toFields)
                    {
                        //field.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant()
                        if (field.Name.Equals(fromField.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            toField = field;
                            break;
                        }
                    }
                }

                if (toField is not null)
                {
                    valuesMappings.Add(MemberMap.EnumField(from, fromField, to, toField));
                }
                else
                {
                    _diagnosticReporter.ReportUnmappedEnumValueError(from.Locations, from, to, fromField.Name);
                }
                //TODO: add warning diagnostic if 'to' has unmapped values (is this necessary?)
            }

            return new EnumMap(from, to, valuesMappings);
        }
    }
}
