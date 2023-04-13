//HintName: Mapper_ProjectionMaps.g.cs
#nullable enable
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Destination> Project<To>(this IQueryable<Source> source)
            => source.Select(x => new Destination
            {
                Property = x.Property
            });
    }
}