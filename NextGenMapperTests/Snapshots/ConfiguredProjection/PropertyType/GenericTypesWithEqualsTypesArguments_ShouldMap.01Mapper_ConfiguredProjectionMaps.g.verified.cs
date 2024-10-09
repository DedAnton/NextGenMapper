//HintName: Mapper_ConfiguredProjectionMaps.g.cs
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
            int ForMapWith
        )
            => source.Select(x => new Test.Destination<int>
        {
            Property = x.Property,
            ForMapWith = ForMapWith
        });
    }
}