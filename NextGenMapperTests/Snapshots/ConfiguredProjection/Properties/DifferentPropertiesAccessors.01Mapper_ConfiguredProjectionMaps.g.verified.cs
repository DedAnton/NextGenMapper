//HintName: Mapper_ConfiguredProjectionMaps.g.cs
#nullable enable
using System.Linq;

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
            Property4 = x.Property4,
            Property5 = x.Property5,
            ForMapWith = ForMapWith
        });
    }
}