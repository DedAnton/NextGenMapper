using MapperSample.MappingWithConstructor;
using MapperSample.MappingWithInitialyzers;
using System.Collections.Generic;

namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //MappingsWithInitialyzers.DefaultMapping();
            //MappingsWithInitialyzers.DefaultMappingWithIncludes();
            //MappingsWithInitialyzers.CustomExpressionMapping();
            //MappingsWithInitialyzers.CustomBlockMapping();
            //MappingsWithInitialyzers.CustomMappingForDefaultTypes();
            //MappingsWithInitialyzers.PartialExpressionMapping();
            //MappingsWithInitialyzers.PartialBlockMapping();

            //MappingsWithConstructor.CommonWithFullConstructor();
            //MappingsWithConstructor.CommonChooseRightConstructor();
            //MappingsWithConstructor.CommonMappingWithIncludes();
            //MappingsWithConstructor.PartialExpressionMapping();
            MappingsWithConstructor.PartialWhenCustomInitializers();
            //MappingsWithConstructor.PartialWhithFullConstructorWhenCustomConstructor();

            //MappingsWithConstructor.PartialWithJustOneConstructor();
        }
    }
}
