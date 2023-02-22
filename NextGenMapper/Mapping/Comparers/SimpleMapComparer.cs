using NextGenMapper.Mapping.Maps;
using System.Collections.Generic;

namespace NextGenMapper.Mapping.Comparers;
internal class SimpleMapComparer : IEqualityComparer<IMap>
{
    public bool Equals(IMap x, IMap y) => x.Source == y.Source && x.Destination == y.Destination;

    public int GetHashCode(IMap obj) => obj.Source.GetHashCode() ^ obj.Destination.GetHashCode();
}
