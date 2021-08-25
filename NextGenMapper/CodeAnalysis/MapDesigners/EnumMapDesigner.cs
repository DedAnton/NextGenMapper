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

        public EnumMapDesigner()
        {
        }

        public EnumMap DesignMapsForPlanner(Enum from, Enum to)
        {
            var valuesMappings = new List<MemberMap>();
            foreach (var fromField in from.Fields)
            {
                var byName = to.Fields.FirstOrDefault(x => x.Name.ToUpperInvariant() == fromField.Name.ToUpperInvariant());
                var byValue = to.Fields.FirstOrDefault(x => x.HasConstantValue && fromField.HasConstantValue 
                    && x.ConstantValue!.UnboxToLong() == fromField.ConstantValue!.UnboxToLong());
                var toField = byValue ?? byName;
                if (toField is not null)
                {
                    valuesMappings.Add(MemberMap.Field(from, fromField, to, toField));
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
