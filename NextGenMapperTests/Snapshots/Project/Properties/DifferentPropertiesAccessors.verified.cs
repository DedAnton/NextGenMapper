//HintName: Mapper_ProjectionMaps.g.cs
#nullable enable
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination> Project<To>(this IQueryable<Test.Source> source)
            => source.Select(x => new Test.Destination
            {
                Property1 = x.Property1,
                Property4 = x.Property4,
                Property5 = x.Property5
            });
    }
}