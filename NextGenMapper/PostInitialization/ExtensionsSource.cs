﻿namespace NextGenMapper.PostInitialization
{
    public static class ExtensionsSource
    {
        public static string Source { get; set; } =

@"using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NextGenMapper.Extensions
{
    internal static class MapperExtensions
    {
        /// <summary>
        /// Do not use this method, for auto-generated mapper only!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSpan<TSource>(this IEnumerable<TSource> source, out ReadOnlySpan<TSource> span)
        {
            bool result = true;
            if (source.GetType() == typeof(TSource[]))
            {
                span = Unsafe.As<TSource[]>(source);
            }
            #if NET5_0_OR_GREATER
            else if (source.GetType() == typeof(List<TSource>))
            {
                span = CollectionsMarshal.AsSpan(Unsafe.As<List<TSource>>(source));
            }
            #endif
            else
            {
                span = default;
                result = false;
            }

            return result;
        }
    }
}";
    }
}
