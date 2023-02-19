using Benchmark.Utils;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace Benchmark.Benchmarks
{
    public class OldBigBanchmark
    {
        public static void Run(int n)
        {
            var syntaxTrees = new List<SyntaxTree>();
            for (var i = 1; i <= n; i++)
            {
                var source = Source.GetSourceCode(i);
                var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions());
                syntaxTrees.Add(syntaxTree);
            }
            var compilation = CSharpCompilation.Create(
                assemblyName: "benchmark",
                syntaxTrees: syntaxTrees,
                references: new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var driver = CSharpGeneratorDriver.Create(new MapperGenerator());

            var sw = new Stopwatch();
            sw.Start();

            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

            var elapsed = sw.Elapsed;

            Console.WriteLine(elapsed);
        }
    }
}
