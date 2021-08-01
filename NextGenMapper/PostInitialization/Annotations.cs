namespace NextGenMapper.PostInitialization
{
    public static class Annotations
    {
        public const string MapToAttributeText = @"
using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapToAttribute : Attribute
    {
        public Type TargetType { get; set; }

        public MapToAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}
";

        public const string MapToAttributeName = "NextGenMapper.MapToAttribute";

        public const string MapReverseAttributeText = @"
using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapReverseAttribute : Attribute
    {
        public MapReverseAttribute() { }
    }
}
";
        public const string MapReverseAttributeName = "NextGenMapper.MapReverseAttribute";

        public const string TargetNameAttributeText = @"
using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TargetNameAttribute : System.Attribute
    {
        public string TargetName { get; set; }
        public TargetNameAttribute(string targetName) => TargetName = targetName;
    }
}
";

        public const string TargetNameAttributeName = "NextGenMapper.TargetNameAttribute";

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

        public const string MapperAttributeName = "NextGenMapper.MapperAttribute";

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
