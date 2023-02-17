using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NextGenMapperTests;

static class VerifyExtensions
{
    public static SettingsTask UseMySettings(this SettingsTask settingsTask, string directory, string methodName, string? variant) => settingsTask
        .UseDirectory(directory)
        .UseFileName(variant is null ? methodName : $"{methodName}_{variant}")
        //.AutoVerify()
        ;
}

public abstract class SourceGeneratorVerifier : VerifyBase
{
    private protected const string TestNamespace = "Test";
    private protected const string TestClassName = "Program";
    private protected const string TestFunctionName = "RunTest";
    public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp10;
    public OutputKind OutputKind { get; set; } = OutputKind.DynamicallyLinkedLibrary;
    public abstract string TestGroup { get; }

    public async Task VerifyAndRun(string source, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null) 
        => await VerifyAndRun(new[] { source }, caller, ignoreSourceErrors, variant);

    public async Task VerifyAndRun(string[] sources, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null)
    {
        var generatorResults = RunGenerator(sources, out var sourceErrors, out var outputCompilation);

        if (!ignoreSourceErrors && sourceErrors.Length > 0)
        {
            if (sourceErrors.All(x => x.Id == "CS8604"))
            {
                throw new NullableException(sourceErrors);
            }

            throw new SourceException(sourceErrors);
        }
        var functionResult = RunMappingFunction(outputCompilation, caller);

        GetPath(out var generatorResultDirectory, out var mapResultDirectory);
        await Task.WhenAll(
            Verify(generatorResults).UseMySettings(generatorResultDirectory, caller, variant),
            Verify(functionResult).UseMySettings(mapResultDirectory, caller, variant));
    }

    public async Task VerifyOnly(string source, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null) 
        => await VerifyOnly(new[] { source }, caller, ignoreSourceErrors, variant);

    public async Task VerifyOnly(string[] sources, [CallerMemberName] string caller = "test", bool ignoreSourceErrors = false, string? variant = null)
    {
        var generatorResults = RunGenerator(sources, out var sourceErrors, out var _);
        if (!ignoreSourceErrors && sourceErrors.Length > 0)
        {
            if (sourceErrors.All(x => x.Id == "CS8604"))
            {
                throw new NullableException(sourceErrors);
            }

            throw new SourceException(sourceErrors);
        }

        GetPath(out var directory, out _);
        await Verify(generatorResults).UseMySettings(directory, caller, variant);
    }

    private protected void GetPath(out string generatorResult, out string mapResult)
    {
        generatorResult = Path.Combine("Snapshots", TestGroup, GetType().Name);
        mapResult = Path.Combine(generatorResult, "MapResult");
    }

    private protected GeneratorDriverRunResult RunGenerator(string[] sources, out Diagnostic[] sourceErrors, out Compilation outputCompilation, IIncrementalGenerator? mapperGenerator = null)
    {
        var compilation = CreateCompilation(sources, GetType().Name);
        mapperGenerator ??= new MapperGenerator();
        GeneratorDriver driver = CreateDriver(mapperGenerator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out var _);

        sourceErrors = outputCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();

        return driver.GetRunResult();
    }

    private protected object? RunMappingFunction(Compilation compilation, string testFunctionName)
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
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        throw new Exception($"can not find type '{TestNamespace}.{TestClassName}' with method '{TestFunctionName}' in assembly '{compilation.AssemblyName}'");
    }

    private protected CSharpGeneratorDriver CreateDriver(params IIncrementalGenerator[] generators)
    {
        //var parseOptions = CreateParseOptions();
        //return CSharpGeneratorDriver.Create(generators, parseOptions: parseOptions);
        return CSharpGeneratorDriver.Create(generators);
    }

    protected Compilation CreateCompilation(string[] sources, string assemblyName)
    {
        var references = Assembly.GetEntryAssembly()?
            .GetReferencedAssemblies()
            .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
            .Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(MethodImplAttribute).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(ImmutableList).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(TypesFromDllTest.EnumFromDll).Assembly.Location));

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

    private protected CompilationOptions CreateCompilationOptions()
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind);

        return compilationOptions
            //.WithNullableContextOptions(NullableContextOptions.Enable)
            .WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
    }

    private protected static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
    {
        string[] args = { "/warnaserror:nullable" };
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
        var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

        return nullableWarnings;
    }

    private protected CSharpParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion, DocumentationMode.Diagnose);
    }
}
