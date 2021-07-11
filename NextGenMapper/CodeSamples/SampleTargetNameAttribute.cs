using System;

namespace NextGenMapper.CodeSamples
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SampleTargetNameAttribute : System.Attribute
    {
        public string TargetName { get; set; }
        public SampleTargetNameAttribute(string targetName) => TargetName = targetName;
    }
}
