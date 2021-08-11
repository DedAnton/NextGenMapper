namespace NextGenMapper.PostInitialization
{
    public static class Annotations
    {
        public const string MapperAttributeText = @"
using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MapperAttribute : Attribute
    {
        public MapperAttribute()
        {

        }
    }
}
";

        public const string MapperAttributeFullName = "NextGenMapper.MapperAttribute";
        public const string MapperAttributeName = "MapperAttribute";
        public const string MapperAttributeShortName = "Mapper";

        public const string PartialAttributeText =
@"using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PartialAttribute : Attribute
    {
        public PartialAttribute() { }
    }
}";
        public const string PartialAttributeName = "NextGenMapper.PartialAttribute";
    }
}
