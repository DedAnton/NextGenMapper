using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Benchmark.Utils;
internal class CompilationHelper
{
    internal static Compilation CreateCompilation(string[] sources, string assemblyName)
    {
        var references = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
            .Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(MethodImplAttribute).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(ImmutableList).Assembly.Location));

        if (references is null)
        {
            throw new Exception("Can not find references for test assembly");
        }

        var compilationOptions = CreateCompilationOptions();
        var compilation = CSharpCompilation.Create(assemblyName: assemblyName)
            .WithOptions(compilationOptions)
            .AddReferences(references);

        var parseOptions = CreateParseOptions();
        foreach (var source in sources)
        {
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
        }

        return compilation;
    }

    internal static CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        return compilationOptions.WithSpecificDiagnosticOptions(
            compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    }

    internal static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }

    internal static CSharpParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion.CSharp10, DocumentationMode.Diagnose);
    }
}
