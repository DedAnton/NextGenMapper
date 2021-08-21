using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Immutable;

namespace NextGenMapper.CodeAnalysis.MapDesigners
{
    public class TypeCustomMapDesigner
    {
        public TypeCustomMapDesigner()
        { }

        public ImmutableArray<TypeMap> DesignTypeCustomMaps(CustomMapMethod customMapMethod) => ImmutableArray.Create<TypeMap>(new TypeCustomMap(customMapMethod));
    }
}
