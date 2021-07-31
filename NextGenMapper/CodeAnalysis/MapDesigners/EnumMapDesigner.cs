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
        private readonly MapPlanner _planner;

        public EnumMapDesigner(SemanticModel semanticModel, MapPlanner planner)
        {
            _semanticModel = semanticModel;
            _planner = planner;
        }

        public void DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        {
            var fromDeclaration = from.GetFirstDeclaration() as EnumDeclarationSyntax;
            var toDeclaration = to.GetFirstDeclaration() as EnumDeclarationSyntax;
            if (fromDeclaration is null || toDeclaration is null)
            {
                throw new ArgumentException("enum must have declaration");
            }
            var fromFields = fromDeclaration.GetFields(_semanticModel);
            var toFields = toDeclaration.GetFields(_semanticModel);

            var valuesMappings = new List<FieldMap>();
            foreach (var fromField in fromFields)
            {
                var type = EnumMapType.ByNameAndValue;

                var byName = toFields.FirstOrDefault(x => x.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant());
                var byValue = toFields.FirstOrDefault(x => x.HasConstantValue && fromField.HasConstantValue 
                    && x.ConstantValue.UnboxToLong() == fromField.ConstantValue.UnboxToLong());
                var toField = type switch
                {
                    EnumMapType.ByNameAndValue => byName ?? byValue,
                    EnumMapType.ByName => byName,
                    EnumMapType.ByValue => byValue,
                    _ => throw new ArgumentOutOfRangeException(nameof(EnumMapType))
                };
                if (toField is not null)
                {
                    valuesMappings.Add(new FieldMap(fromField, toField));
                }
                else
                {
                    //add diagnostic
                }
                //add diagnostic if 'to' has unmapped values 
            }

            _planner.AddCommonMap(new EnumMap(from, to, valuesMappings));
        }
    }
}
