using System;

namespace NextGenMapper.CodeSamples
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SampleMapperAttribute : Attribute
    {
        public SampleMapperAttribute()
        {

        }
    }
}
