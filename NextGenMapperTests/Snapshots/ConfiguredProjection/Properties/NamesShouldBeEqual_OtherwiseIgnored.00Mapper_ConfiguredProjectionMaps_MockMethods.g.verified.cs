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
            int SamePropertyName = default!,
            int DifferentPropertyNameB = default!,
            int DifferentPropertyNameC = default!,
            int DifferentPropertyNameD = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            Expression<Func<Test.Source, int>> SamePropertyName = default!,
            Expression<Func<Test.Source, int>> DifferentPropertyNameB = default!,
            Expression<Func<Test.Source, int>> DifferentPropertyNameC = default!,
            Expression<Func<Test.Source, int>> DifferentPropertyNameD = default!,
            Expression<Func<Test.Source, int>> ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}