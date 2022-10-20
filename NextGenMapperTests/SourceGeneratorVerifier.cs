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
    public OutputKind OutputKind { get; set; } = OutputKind.DynamicallyLinkedLibrary;

    public async Task VerifyAndRun(string source, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null) 
        => await VerifyAndRun(new[] { source }, caller, ignoreSourceErrors, variant);

    public async Task VerifyAndRun(string[] sources, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null)
    {
        var generatorResults = RunGenerator(sources, out var sourceErrors, out var outputCompilation);
        if (!ignoreSourceErrors && sourceErrors.Length > 0)
        {
            throw new SourceException(sourceErrors);
        }

        var generatorPath = variant != null
            ? Path.Combine("Snapshots", caller, variant, "Generator")
            : Path.Combine("Snapshots", caller, "Generator");

        var mapResultPath = variant != null
            ? Path.Combine("Snapshots", caller, variant, "MapResult")
            : Path.Combine("Snapshots", caller, "MapResult");

        var functionResult = RunMappingFunction(outputCompilation, caller);

        await Task.WhenAll(
            Verify(generatorResults).UseDirectory(generatorPath),
            Verify(functionResult).UseDirectory(mapResultPath));
    }

    public async Task VerifyOnly(string source, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null) 
        => await VerifyOnly(new[] { source }, caller, ignoreSourceErrors, variant);

    public async Task VerifyOnly(string[] sources, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null)
    {
        var generatorResults = RunGenerator(sources, out var sourceErrors, out var _);
        if (!ignoreSourceErrors && sourceErrors.Length > 0)
        {
            throw new SourceException(sourceErrors);
        }

        var generatorPath = variant != null
            ? Path.Combine("Snapshots", caller, variant, "Generator")
            : Path.Combine("Snapshots", caller, "Generator");

        await Verify(generatorResults).UseDirectory(generatorPath);
    }

    private GeneratorDriverRunResult RunGenerator(string[] sources, out Diagnostic[] sourceErrors, out Compilation outputCompilation)
    {
        var compilation = CreateCompilation(sources, GetType().Name);
        var generator = new MapperGenerator();
        GeneratorDriver driver = CreateDriver(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out var _);

        sourceErrors = outputCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();

        return driver.GetRunResult();
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

    private CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind);

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