using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class EnumMapDesigner
    {
        public EnumMapDesigner()
        { }

        public ImmutableArray<TypeMap> DesignEnumMaps(Enum from, Enum to)
        {
            var valuesMappings = new List<MemberMap>();
            foreach(var toField in to.Fields)
            {
                var byName = from.Fields.FirstOrDefault(x => x.Name.ToUpperInvariant() == toField.Name.ToUpperInvariant());
                var byValue = from.Fields.FirstOrDefault(x => x.HasValue && toField.HasValue && x.Value == toField.Value);
                var fromField = byValue ?? byName;

                if (fromField != null)
                {
                    valuesMappings.Add(new EnumFieldMap(fromField, toField));
                }
                else
                {
                    //TODO: add diagnostic
                }
                //TODO: add diagnostic if 'from' has unmapped values 
            }

            return ImmutableArray.Create<TypeMap>(new EnumMap(from, to, valuesMappings));
        }

        //public void DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to)
        //{
        //    var fromDeclaration = from.GetFirstDeclaration() as EnumDeclarationSyntax;
        //    var toDeclaration = to.GetFirstDeclaration() as EnumDeclarationSyntax;
        //    if (fromDeclaration is null || toDeclaration is null)
        //    {
        //        throw new ArgumentException("enum must have declaration");
        //    }
        //    var fromFields = fromDeclaration.GetFields(_semanticModel);
        //    var toFields = toDeclaration.GetFields(_semanticModel);

        //    var valuesMappings = new List<MemberMap>();
        //    foreach (var fromField in fromFields)
        //    {
        //        var byName = toFields.FirstOrDefault(x => x.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant());
        //        var byValue = toFields.FirstOrDefault(x => x.HasConstantValue && fromField.HasConstantValue 
        //            && x.ConstantValue.UnboxToLong() == fromField.ConstantValue.UnboxToLong());
        //        var toField = byValue ?? byName;
        //        if (toField is not null)
        //        {
        //            valuesMappings.Add(MemberMap.Field(fromField, toField));
        //        }
        //        else
        //        {
        //            //add diagnostic
        //        }
        //        //add diagnostic if 'to' has unmapped values 
        //    }

        //    _planner.AddCommonMap(new EnumMap(from, to, valuesMappings));
        //}
    }
}
