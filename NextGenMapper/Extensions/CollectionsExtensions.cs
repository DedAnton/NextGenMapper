using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.Extensions
{
    public static class CollectionsExtensions
    {
        public static void ForEachIndex<T>(this List<T> list, Action<int, T> action)
        {
            for (var i = 0; i < list.Count(); i++)
            {
                action(i, list[i]);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach(var item in collection)
            {
                action(item);
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection) => !collection.Any();

        public static IEnumerable<T> Complement<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer) => first.Union(second, comparer).Except(second, comparer);

        public static IEnumerable<string> Complement(this IEnumerable<string> first, IEnumerable<string> second) => first.Union(second, StringComparer.InvariantCultureIgnoreCase).Except(second, StringComparer.InvariantCultureIgnoreCase);
        
        public static void AddIfNotNull<T>(this List<T> list, T? item)
        {
            if (item is not null)
            {
                list.Add(item);
            }
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> collectionWithNulls) => collectionWithNulls.OfType<T>(); 
    }
}
