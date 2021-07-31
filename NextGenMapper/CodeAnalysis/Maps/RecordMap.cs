using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class RecordMap : TypeMap
    {
        public RecordMap(ITypeSymbol from, ITypeSymbol to)
            : base(from, to)
        {
            
        }
    }
}
