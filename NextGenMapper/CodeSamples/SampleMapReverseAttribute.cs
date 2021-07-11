using System;

namespace NextGenMapper.CodeSamples
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapReverseAttribute : Attribute
    {
        public MapReverseAttribute() { }
    }
}
