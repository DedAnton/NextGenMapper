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
            int ForMapWith
        )
            => source.Select(x => new Test.Destination
        {
            PropertyA = x.PropertyA,
            PropertyB = x.PropertyB,
            ForMapWith = ForMapWith
        });
    }
}