namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            MappingWithInitialyzers.DefaultMapping();
            MappingWithInitialyzers.DefaultMappingWithIncludes();
            MappingWithInitialyzers.CustomExpressionMapping();
            MappingWithInitialyzers.CustomBlockMapping();
            MappingWithInitialyzers.CustomMappingForDefaultTypes();
            MappingWithInitialyzers.PartialExpressionMapping();
            MappingWithInitialyzers.PartialBlockMapping();
        }
    }
}
