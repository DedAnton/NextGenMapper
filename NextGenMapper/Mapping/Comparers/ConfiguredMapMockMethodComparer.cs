using NextGenMapper.Mapping.Maps.Models;
using System.Collections.Generic;

namespace NextGenMapper.Mapping.Comparers;
internal class ConfiguredMapMockMethodComparer : IEqualityComparer<ConfiguredMapMockMethod>
{
    public bool Equals(ConfiguredMapMockMethod x, ConfiguredMapMockMethod y)
    {
        if (x.Source != y.Source
            || x.Destination != y.Destination
            || x.Parameters.Length != y.Parameters.Length)
        {
            return false;
        }

        var spanX = x.Parameters;
        var spanY = y.Parameters;

        for (var i = 0; i < x.Parameters.Length; i++)
        {
            if (spanX[i].Type != spanY[i].Type)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ConfiguredMapMockMethod map)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + map.Source.GetHashCode();
            hash = hash * 23 + map.Destination.GetHashCode();
            foreach (var argument in map.Parameters)
            {
                hash = hash * 23 + argument.Type.GetHashCode();
            }
            hash = hash * 23 + map.Parameters.Length.GetHashCode();

            return hash;
        }
    }
}
