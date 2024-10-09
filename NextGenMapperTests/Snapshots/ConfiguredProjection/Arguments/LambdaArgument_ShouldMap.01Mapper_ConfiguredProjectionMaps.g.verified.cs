//HintName: Mapper_ConfiguredProjectionMaps.g.cs
#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            Expression<Func<Test.Source, int>> ForMapWith1,
            Expression<Func<Test.Source, int>> ForMapWith2
        )
            => source.Select(x => new Test.Destination
        {
            Property1 = x.Property1,
            Property2 = x.Property2,
            ForMapWith1 = x.Property1 + 10,
            ForMapWith2 = x.Property2 * 3
        });
    }
}