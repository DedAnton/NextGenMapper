//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static MapperTest.ConfigWithList Map<To>(this MapperTest.ConfigWithDict source) => new MapperTest.ConfigWithList()
        {
            Configurations = source.Configurations.Map<System.Collections.Generic.List<MapperTest.ConfigKeyValueEntry>>()
        };
    }
}