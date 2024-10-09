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
            int Property1,
            int Property2,
            int Property3,
            int Property4
        )
            => source.Select(x => new Test.Destination
        {
            Property1 = Property1,
            Property2 = Property2,
            Property3 = Property3,
            Property4 = Property4
        });
    }
}