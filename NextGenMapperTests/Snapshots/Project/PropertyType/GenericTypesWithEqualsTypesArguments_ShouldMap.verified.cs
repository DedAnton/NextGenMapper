//HintName: Mapper_ProjectionMaps.g.cs
#nullable enable
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination<int>> Project<To>(this IQueryable<Test.Source<int>> source)
            => source.Select(x => new Test.Destination<int>
            {
                Property = x.Property
            });
    }
}