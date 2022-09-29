using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public abstract class TypeMap
    {
        public ITypeSymbol From { get; }
        public ITypeSymbol To { get; }
        public Location MapLocation { get; }

        public TypeMap(ITypeSymbol from, ITypeSymbol to, Location mapLocation)
        {
            From = from;
            To = to;
            MapLocation = mapLocation;
        }
    }
}
