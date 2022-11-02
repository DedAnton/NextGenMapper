using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public abstract class TypeMap
    {
        public string From { get; }
        public string To { get; }
        public ITypeSymbol FromType { get; }
        public ITypeSymbol ToType { get; }
        public Location MapLocation { get; }

        public TypeMap(ITypeSymbol from, ITypeSymbol to, Location mapLocation)
        {
            From = from.ToDisplayString(NullableFlowState.None);
            To = to.ToDisplayString(NullableFlowState.None);
            FromType = from;
            ToType = to;
            MapLocation = mapLocation;
        }
    }
}
