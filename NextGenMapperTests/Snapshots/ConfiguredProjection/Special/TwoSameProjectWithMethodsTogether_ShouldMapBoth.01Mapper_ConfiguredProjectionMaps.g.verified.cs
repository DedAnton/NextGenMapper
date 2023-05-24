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
            int ForMapWith1
        )
            => source.Select(x => new Test.Destination
        {
            ForMapWith1 = ForMapWith1
        });
    }
}