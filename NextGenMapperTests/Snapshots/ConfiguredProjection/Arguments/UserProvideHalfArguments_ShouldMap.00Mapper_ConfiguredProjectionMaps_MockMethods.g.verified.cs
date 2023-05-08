//HintName: Mapper_ConfiguredProjectionMaps_MockMethods.g.cs
#nullable enable
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            int Property1 = default!,
            int Property2 = default!,
            int ForMapWith1 = default!,
            int ForMapWith2 = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}