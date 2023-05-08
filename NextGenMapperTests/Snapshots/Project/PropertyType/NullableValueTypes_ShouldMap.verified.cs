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
                PropertyB = x.PropertyB
            });
    }
}