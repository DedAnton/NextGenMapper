namespace NextGenMapper
{
    public static class MappersText
    {
        public const string StartMapper = 
@"using System;

namespace NextGenMapper
{
    public static partial class Mapper
    {
        public static TTo Map<TTo>(this object source) => throw new InvalidOperationException();
    }
}";

        public const string MapperBegin =
@"namespace NextGenMapper
{
    public static partial class Mapper
    {
";

        public const string MapperEnd =
@"    }
}";
    }
}
