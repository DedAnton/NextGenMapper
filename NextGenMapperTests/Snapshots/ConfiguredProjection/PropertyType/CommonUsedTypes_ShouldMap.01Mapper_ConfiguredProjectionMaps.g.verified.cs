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
            Property1 = x.Property1,
            Property2 = x.Property2,
            Property3 = x.Property3,
            Property4 = x.Property4,
            Property5 = x.Property5,
            Property6 = x.Property6,
            Property7 = x.Property7,
            Property8 = x.Property8,
            Property9 = x.Property9,
            Property10 = x.Property10,
            Property11 = x.Property11,
            Property12 = x.Property12,
            Property13 = x.Property13,
            Property14 = x.Property14,
            Property15 = x.Property15,
            Property16 = x.Property16,
            Property17 = x.Property17,
            Property18 = x.Property18,
            Property19 = x.Property19,
            Property20 = x.Property20,
            ForMapWith = ForMapWith
        });
    }
}