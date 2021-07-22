﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.Extensions
{
    public static class CollectionsExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) => collection.ToList().ForEach(x => action(x));

        public static bool IsEmpty<T>(this IEnumerable<T> collection) => !collection.Any();

        public static IEnumerable<T> Complement<T>(this IEnumerable<T> first, IEnumerable<T> second) => first.Union(second).Except(second);

        public static void AddIfNotNull<T>(this List<T> list, T item)
        {
            if (item is not null)
            {
                list.Add(item);
            }
        }
    }
}