using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper
{
    public class MappingPlanner
    {
        public List<MappingPlan> MappingPlans { get; } = new();

        public MappingPlan CommonMappingPlan => MappingPlans.SingleOrDefault(x => x.Mappings.All(x => x.Type == MappingType.Common));
        public List<MappingPlan> CustomMappingPlans 
            => MappingPlans.Where(x => x.Mappings.Count > 0 && x.Mappings.All(y => y.Type != MappingType.Common)).ToList();

        public void AddMapping(TypeMapping mapping, List<string> usings = null)
        {
            usings ??= new();

            if (mapping.Type == MappingType.Common)
            {
                AddCommonMapping(mapping);
            } 
            else
            {
                AddCustomMapping(mapping, usings);
            }
        }

        private void AddCommonMapping(TypeMapping mapping)
        {
            var hasAlready = MappingPlans.SelectMany(x => x.Mappings).Contains(mapping);

            if (!hasAlready)
            {
                if (CommonMappingPlan == null)
                {
                    MappingPlans.Add(new MappingPlan());
                }
                CommonMappingPlan?.Mappings.Add(mapping);
            }
        }

        private void AddCustomMapping(TypeMapping mapping, List<string> usings)
        {
            var hasAlready = CustomMappingPlans.SelectMany(x => x.Mappings).Contains(mapping);

            if (!hasAlready)
            {
                var customMappingPlan = CustomMappingPlans.SingleOrDefault(x => x.Usings.SequenceEqual(usings));
                if (customMappingPlan == null)
                {
                    MappingPlans.Add(new MappingPlan(mapping, usings));
                }
                else
                {
                    customMappingPlan.Mappings.Add(mapping);
                }

                CommonMappingPlan?.Mappings.Remove(mapping);
            }
        }
    }
}
