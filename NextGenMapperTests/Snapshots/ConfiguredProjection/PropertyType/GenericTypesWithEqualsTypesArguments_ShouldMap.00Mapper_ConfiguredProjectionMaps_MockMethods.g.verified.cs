//HintName: Mapper_ConfiguredProjectionMaps_MockMethods.g.cs
#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination<int>> ProjectWith<To>
        (
            this IQueryable<Test.Source<int>> source,
            int Property = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static IQueryable<Test.Destination<int>> ProjectWith<To>
        (
            this IQueryable<Test.Source<int>> source,
            Expression<Func<Test.Source<int>, int>> Property = default!,
            Expression<Func<Test.Source<int>, int>> ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}