namespace NextGenMapper.PostInitialization
{
    public static class StartMapperSource
    {
        public const string MapMethodName = "Map";
        public const string ConfiguredMapMethodName = "MapWith";
        public const string MapMethodFullName = "NextGenMapper.Mapper.Map<To>(object)";
        public const string MapWithMethodFullName = "NextGenMapper.Mapper.MapWith<To>(object)";

        public const string StartMapper =
@"using System;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");

        internal static To MapWith<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
    }
}";
    }
}
