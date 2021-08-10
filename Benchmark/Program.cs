using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Benchmark
{
    class Program
    {
        // 1    - 1.2225642
        // 10   - 1.3913829
        // 100  - 2.0936369
        // 1000 - 13.1445635
        static void Main(string[] args)
        {
            var syntaxTrees = new List<SyntaxTree>();
            for(var i = 1; i <= 1000; i++)
            {
                var source = Source.GetSourceCode(i);
                var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9));
                syntaxTrees.Add(syntaxTree);
            }
            var compilation = CSharpCompilation.Create(
                assemblyName: "benchmark",
                syntaxTrees: syntaxTrees,
                references: new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var driver = CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(new MapperGenerator()),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null);

            var sw = new Stopwatch();
            sw.Start();

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var diagnostics);

            var elapsed = sw.Elapsed;

            Console.WriteLine(elapsed);
        }
    }
}
