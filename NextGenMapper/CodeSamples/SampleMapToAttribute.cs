using System;

namespace NextGenMapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SampleMapToAttribute : Attribute
    {
        public Type TargetType { get; set; }

        public SampleMapToAttribute(Type targetType)
        {
            TargetType = targetType;
        }
    }
}
