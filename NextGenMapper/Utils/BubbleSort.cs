using System;
using System.Collections.Generic;

namespace NextGenMapper.Utils;

internal static class BubbleSort
{
    public static void SortSpan<T, C>(ref Span<T> span, C comparer) where C : IComparer<T>
    {
        var n = span.Length;
        bool swapRequired;
        for (int i = 0; i < n - 1; i++)
        {
            swapRequired = false;
            for (int j = 0; j < n - i - 1; j++)
                if (comparer.Compare(span[j], span[j + 1]) > 0)
                {
                    (span[j + 1], span[j]) = (span[j], span[j + 1]);
                    swapRequired = true;
                }
            if (swapRequired == false)
                break;
        }
    }
}