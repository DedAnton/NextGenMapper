using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private const string attributeText = @"
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

        public void Execute(GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
                using System;
                namespace NGMapper
                {
                    public static class Mapper
                    {
                        public static void Map()
                        {
                            Console.WriteLine(""Hello from generated code!"");
                            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
                ");

            // using the context, get a list of syntax trees in the users compilation
            IEnumerable<SyntaxTree> syntaxTrees = context.Compilation.SyntaxTrees;

            // add the filepath of each tree to the class we're building
            foreach (SyntaxTree tree in syntaxTrees)
            {
                sourceBuilder.AppendLine($@"Console.WriteLine(@"" - {tree.Options.PreprocessorSymbolNames}"");");
            }

            // finish creating the source to inject
            sourceBuilder.Append(@"
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("helloWorldGenerated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }
    }
}
