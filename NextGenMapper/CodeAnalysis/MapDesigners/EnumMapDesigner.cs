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
        private readonly SemanticModel _semanticModel;

        public EnumMapDesigner(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public EnumMap DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            if (from.GetFirstDeclaration() is not EnumDeclarationSyntax fromDeclaration 
                || to.GetFirstDeclaration() is not EnumDeclarationSyntax toDeclaration)
            {
                throw new ArgumentException("enum must have declaration");
            }
            var fromFields = fromDeclaration.GetFields(_semanticModel);
            var toFields = toDeclaration.GetFields(_semanticModel);

            var valuesMappings = new List<MemberMap>();
            foreach (var fromField in fromFields)
            {
                var byName = toFields.FirstOrDefault(x => x.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant());
                var byValue = toFields.FirstOrDefault(x => x.HasConstantValue && fromField.HasConstantValue 
                    && x.ConstantValue.UnboxToLong() == fromField.ConstantValue.UnboxToLong());
                var toField = byValue ?? byName;
                if (toField is not null)
                {
                    valuesMappings.Add(MemberMap.Field(fromField, toField));
                }
                else
                {
                    //add diagnostic
                }
                //add diagnostic if 'to' has unmapped values 
            }

            return new EnumMap(from, to, valuesMappings);
        }
    }
}
