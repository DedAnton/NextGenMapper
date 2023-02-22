using Microsoft.CodeAnalysis;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.Extensions;

internal static class IncrementalValueProviderExtensions
{
    public static IncrementalValueProvider<ImmutableArray<TSource>> Distinct<TSource>(
        this IncrementalValueProvider<ImmutableArray<TSource>> source,
        IEqualityComparer<TSource> equalityComparer)
        => source.Select((x, _) =>
        {
            if (x.Length < 2)
            {
                return x;
            }

            var hashSet = new HashSet<TSource>(x, equalityComparer);
            var newArray = new TSource[hashSet.Count];
            hashSet.CopyTo(newArray);

            return Unsafe.CastArrayToImmutableArray(ref newArray);
        });

    public static void ReportDiagnostics(this IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<Diagnostic> diagnostics)
        => context.RegisterSourceOutput(diagnostics, (sourceProductionContext, diagnostic) => sourceProductionContext.ReportDiagnostic(diagnostic));

    public static IncrementalValueProvider<ImmutableArray<TSource>> Concat<TSource>(this IncrementalValuesProvider<TSource> left, IncrementalValuesProvider<TSource> right)
        => left
        .Collect()
        .Combine(right.Collect())
        .Select(static (x, _) =>
        {
            if (x.Left.Length == 0)
            {
                return x.Right;
            }
            if (x.Right.Length == 0)
            {
                return x.Left;
            }

            var newArray = new TSource[x.Left.Length + x.Right.Length];
            x.Left.CopyTo(newArray);
            x.Right.CopyTo(newArray, x.Left.Length);

            return Unsafe.CastArrayToImmutableArray(ref newArray);
        });

    public static IncrementalValueProvider<ImmutableArray<TSource>> RemoveUserMaps<TSource>(
        this IncrementalValueProvider<ImmutableArray<TSource>> source,
        IncrementalValueProvider<HashSet<UserMap>> userMaps) where TSource : IMap
        => source
        .Combine(userMaps)
        .Select(static (x, ct) =>
        {
            var (maps, userMaps) = x;

            if (maps.Length == 0 || userMaps.Count == 0)
            {
                return maps;
            }

            ct.ThrowIfCancellationRequested();
            var filteredMaps = new ValueListBuilder<TSource>(maps.Length);
            for (var i = 0; i < maps.Length; i++)
            {
                var userMapFromMap = new UserMap(maps[i].Source, maps[i].Destination);
                if (!userMaps.Contains(userMapFromMap))
                {
                    filteredMaps.Append(maps[i]);
                }
            }

            return filteredMaps.ToImmutableArray();
        });
}

