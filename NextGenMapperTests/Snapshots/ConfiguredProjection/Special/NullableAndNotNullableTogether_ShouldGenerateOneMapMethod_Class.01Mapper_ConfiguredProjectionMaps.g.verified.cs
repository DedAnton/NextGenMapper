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
            byte ForMapWith1
        )
            => source.Select(x => new Test.Destination
        {
            Property = x.Property,
            ForMapWith1 = ForMapWith1
        });

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            short ForMapWith2
        )
            => source.Select(x => new Test.Destination
        {
            Property = x.Property,
            ForMapWith2 = ForMapWith2
        });

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            int ForMapWith3
        )
            => source.Select(x => new Test.Destination
        {
            Property = x.Property,
            ForMapWith3 = ForMapWith3
        });

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            long ForMapWith4
        )
            => source.Select(x => new Test.Destination
        {
            Property = x.Property,
            ForMapWith4 = ForMapWith4
        });
    }
}