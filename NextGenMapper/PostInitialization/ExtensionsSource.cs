namespace NextGenMapper.PostInitialization
{
    public static class ExtensionsSource
    {
        public static string Source =

@"using System;
using System.Collections.Generic;

namespace NextGenMapper.Extensions
{
    internal static class MapperExtensions
    {
        /// <summary>
        /// Do not use this method, for auto-generated mapper only!
        /// </summary>
        internal static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach(var item in source)
            {
                yield return selector(item);
            }
        }

        /// <summary>
        /// Do not use this method, for auto-generated mapper only!
        /// </summary>
        internal static List<T> ToList<T>(this IEnumerable<T> source) => new(source);

        /// <summary>
        /// Do not use this method, for auto-generated mapper only!
        /// </summary>
        internal static T[] ToArray<T>(this IEnumerable<T> source) => new List<T>(source).ToArray();
    }
}";
    }
}
