//HintName: Mapper_ConfiguredProjectionMaps_MockMethods.g.cs
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
            int Property = default!,
            byte ForMapWith1 = default!,
            short ForMapWith2 = default!,
            int ForMapWith3 = default!,
            long ForMapWith4 = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            Expression<Func<Test.Source, int>> Property = default!,
            Expression<Func<Test.Source, byte>> ForMapWith1 = default!,
            Expression<Func<Test.Source, short>> ForMapWith2 = default!,
            Expression<Func<Test.Source, int>> ForMapWith3 = default!,
            Expression<Func<Test.Source, long>> ForMapWith4 = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}