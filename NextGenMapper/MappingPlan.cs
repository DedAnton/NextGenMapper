using System.Collections.Generic;

namespace NextGenMapper
{
    public class MappingPlan
    {
        public List<TypeMapping> Mappings { get; } = new();
        public List<string> Usings { get; } = new();

        public MappingPlan() { }

        public MappingPlan(TypeMapping mapping, List<string> usings)
        {
            Mappings.Add(mapping);
            Usings = usings;
        }
    }
}
