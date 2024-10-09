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
            int PropertyName = default!,
            int DerrivedPropertyA = default!,
            int ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            Expression<Func<Test.Source, int>> PropertyName = default!,
            Expression<Func<Test.Source, int>> DerrivedPropertyA = default!,
            Expression<Func<Test.Source, int>> ForMapWith = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}