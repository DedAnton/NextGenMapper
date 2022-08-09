namespace NextGenMapper.PostInitialization
{
    public static class StartMapperSource
    {
        public static string FunctionFullName { get; set; } = "NextGenMapper.Mapper.Map<To>(object)";
        public static string StartMapper { get; set; } =
@"using System;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        public static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
    }
}";
    }
}
