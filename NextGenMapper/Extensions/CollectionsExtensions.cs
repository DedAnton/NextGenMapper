using System;
using System.Collections.Generic;

namespace NextGenMapper.Extensions
{
    public static class CollectionsExtensions
    {
        public static void ForEachIndex<T>(this List<T> list, Action<int, T> action)
        {
            for (var i = 0; i < list.Count; i++)
            {
                action(i, list[i]);
            }
        }
    }
}
