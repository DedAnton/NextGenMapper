using NextGenMapper.Mapping.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextGenMapper.Mapping.Comparers;

internal class ConfiguredProjectionMapComparer : IEqualityComparer<ConfiguredProjectionMap>
{
    public bool Equals(ConfiguredProjectionMap x, ConfiguredProjectionMap y)
    {
        if (x.Source != y.Source
            || x.Destination != y.Destination
            || x.UserArguments.Length != y.UserArguments.Length)
        {
            return false;
        }

        var spanX = x.UserArguments.AsSpan();
        var spanY = y.UserArguments.AsSpan();

        for (var i = 0; i < x.UserArguments.Length; i++)
        {
            if (spanX[i].Type != spanY[i].Type)
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ConfiguredProjectionMap map)
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
