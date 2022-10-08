using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NextGenMapperTests;

public class SourceGeneratorVerifier : VerifyBase
{
    private const string TestNamespace = "Test";
    private const string TestClassName = "Program";
    private const string TestFunctionName = "RunTest";
    public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp10;

    public async Task VerifyAndRun(string source, [CallerMemberName] string caller = "test") => await VerifyAndRun(new[] { source }, caller);

    public async Task VerifyAndRun(string[] sources, [CallerMemberName] string caller = "test")
    {
        var compilation = CreateCompilation(sources, GetType().Name);
        var generator = new MapperGenerator();
        GeneratorDriver driver = CreateDriver(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        driver.GetRunResult();

        await Verify(driver).UseDirectory(Path.Combine("Snapshots", "Generator"));

        var functionResult = RunMappingFunction(outputCompilation, caller);

        await Verify(functionResult).UseDirectory(Path.Combine("Snapshots", "MapResult"));
    }

    private object? RunMappingFunction(Compilation compilation, string testFunctionName)
    {
        var path = Path.Combine("..", "..", "..", "Temp", $"{GetType().Name}_{testFunctionName}_{DateTimeOffset.Now.ToFileTime()}.dll");
        compilation.Emit(path);
        var assembly = Assembly.Load(File.ReadAllBytes(path));
        var type = assembly?.GetType($"{TestNamespace}.{TestClassName}");
        var methodInfo = type?.GetMethod(TestFunctionName);
        if (type is not null && methodInfo is not null)
        {
            try
            {
                var classInstance = Activator.CreateInstance(type, null);

                return methodInfo.Invoke(classInstance, Array.Empty<object>());
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(path);
            }
        }

        throw new Exception($"can not find type '{TestNamespace}.{TestClassName}' with method '{TestFunctionName}' in assembly '{compilation.AssemblyName}'");
    }

    private CSharpGeneratorDriver CreateDriver(params ISourceGenerator[] generators)
    {
        var parseOptions = CreateParseOptions();
        return CSharpGeneratorDriver.Create(generators, parseOptions: parseOptions);
    }

    private Compilation CreateCompilation(string[] sources, string assemblyName)
    {
        var references = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
            .Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(MethodImplAttribute).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location));

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

    private CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        return compilationOptions.WithSpecificDiagnosticOptions(
            compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    }

    private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }

    private CSharpParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion, DocumentationMode.Diagnose);
    }
}