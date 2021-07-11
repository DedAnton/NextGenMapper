using System;
using System.Collections.Generic;
using System.Text;

namespace NextGenMapper
{
    public static class Annotations
    {
        public const string mapToAttributeText = @"
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

        public const string mapToAttributeName = "NextGenMapper.MapToAttribute";

        public const string mapReverseAttributeText = @"
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
        public const string mapReverseAttributeName = "NextGenMapper.MapReverseAttribute";

        public const string targetNameAttributeText = @"
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

        public const string targetNameAttributeName = "NextGenMapper.TargetNameAttribute";
    }
}
