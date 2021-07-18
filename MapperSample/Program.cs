using MapperSample.MappingWithInitialyzers;

namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            MappingsWithInitialyzers.DefaultMapping();
            MappingsWithInitialyzers.DefaultMappingWithIncludes();
            MappingsWithInitialyzers.CustomExpressionMapping();
            MappingsWithInitialyzers.CustomBlockMapping();
            MappingsWithInitialyzers.CustomMappingForDefaultTypes();
            MappingsWithInitialyzers.PartialExpressionMapping();
            MappingsWithInitialyzers.PartialBlockMapping();
        }
    }
}
