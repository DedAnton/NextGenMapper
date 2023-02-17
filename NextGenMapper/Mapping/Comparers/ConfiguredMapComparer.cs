using NextGenMapper.Mapping.Maps;
using System.Collections.Generic;

namespace NextGenMapper.Mapping.Comparers;

internal class ConfiguredMapComparer : IEqualityComparer<ConfiguredMap>
{
    public bool Equals(ConfiguredMap x, ConfiguredMap y)
    {
        if (x.Source != y.Source
            || x.Destination != y.Destination
            || x.UserArguments.Length != y.UserArguments.Length)
        {
            return false;
        }

        var spanX = x.UserArguments;
        var spanY = y.UserArguments;

        for (var i = 0; i < x.UserArguments.Length; i++)
        {
            if (spanX[i].Type != spanY[i].Type)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ConfiguredMap map)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + map.Source.GetHashCode();
            hash = hash * 23 + map.Destination.GetHashCode();
            foreach (var argument in map.UserArguments)
            {
                hash = hash * 23 + argument.Type.GetHashCode();
            }
            hash = hash * 23 + map.UserArguments.Length.GetHashCode();

            return hash;
        }
    }
}