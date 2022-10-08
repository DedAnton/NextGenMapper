using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace NextGenMapperTests
{
    public static class TestExtensions
    {
        public static Compilation CreateCompilation(this string source, string assemblyName)
            => CreateCompilation(new[] { source }, assemblyName);

        public static Compilation CreateCompilation(string[] sources, string assemblyName)
        {
            var references = Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
                .Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(MethodImplAttribute).Assembly.Location))
                .Append(MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location));

            var compilation = CSharpCompilation.Create(assemblyName: assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references);

            foreach(var source in sources)
            {
                compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)));
            }

            return compilation;
        }

        public static GeneratorDriver CreateDriver(this Compilation compilation, params ISourceGenerator[] generators) => CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generators),
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
            optionsProvider: null
        );

        public static Compilation RunGenerators(
            this string sourceCode,
            out ImmutableArray<Diagnostic> diagnostics,
            [CallerMemberName] string caller = "test",
            params ISourceGenerator[] generators)
        {
            var compilation = sourceCode.CreateCompilation(caller);
            compilation.CreateDriver(generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);

            return updatedCompilation;
        }

        public static Compilation RunGenerators(
            string[] sources,
            out ImmutableArray<Diagnostic> diagnostics,
            [CallerMemberName] string caller = "test",
            params ISourceGenerator[] generators)
        {
            var compilation = CreateCompilation(sources, caller);
            compilation.CreateDriver(generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);

            return updatedCompilation;
        }

        public static bool TestMapper(this Compilation compilation, out object source, out object destination, out string message, [CallerMemberName] string caller = "test")
        {
            var path = Path.Combine("..", "..", "..", "Temp", $"{caller}_{DateTimeOffset.Now.ToFileTime()}.dll");
            compilation.Emit(path);
            Assembly assembly = Assembly.Load(File.ReadAllBytes(path));
            Type type = assembly.GetType("Test.Program");
            MethodInfo methodInfo = type.GetMethod("TestMethod");
            var classInstance = Activator.CreateInstance(type, null);

            try
            {
                methodInfo.Invoke(classInstance, Array.Empty<object>());
            }
            catch (Exception ex)
            {
                source = ex.InnerException?.GetType().GetProperty("MapSource")?.GetValue(ex.InnerException);
                destination = ex.InnerException?.GetType().GetProperty("MapDestination")?.GetValue(ex.InnerException);
                message = ex.InnerException?.Message;

                return false;
            }
            finally
            {
                File.Delete(path);
            }

            source = destination = message = null;
            return true;
        }

        public static bool IsFilteredEmpty(this IEnumerable<Diagnostic> diagnostics) => !diagnostics.Where(x => x.Id != "CS8019").Any();

        public static string GetObjectsString(object source, object destination, string message)
        {
            var sourceJson = JsonSerializer.Serialize(source, new JsonSerializerOptions { WriteIndented = true });
            var destinationJson = JsonSerializer.Serialize(destination, new JsonSerializerOptions { WriteIndented = true });

            var sourceLines = sourceJson.Split("\r\n");

            var maxLength = sourceLines.Max(x => x.Length);
            sourceLines = sourceLines.Select(x => x.PadRight(maxLength)).ToArray();
            var centerIndex = sourceLines.Length / 2;
            sourceLines[centerIndex] = sourceLines[centerIndex] + "   =>   ";
            sourceLines = sourceLines.Select(x => x.PadRight(maxLength + 8)).ToArray();

            var destinationLines = destinationJson.Split("\r\n");
            var maxLinesCount = sourceLines.Length > destinationLines.Length ? sourceLines.Length : destinationLines.Length;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine().AppendLine().AppendLine(message);
            for (var i = 0; i < maxLinesCount; i++)
            {
                var line = $"{sourceLines.ToList().ElementAtOrDefault(i)}{destinationLines.ToList().ElementAtOrDefault(i)}";
                stringBuilder.AppendLine(line);
            }

            return stringBuilder.ToString();
        }

        public static string PrintDiagnostics(this IEnumerable<Diagnostic> diagnostics, string name)
        {
            var diagnostincsString = string.Concat(diagnostics.Select(x => $"{x}\r\n"));

            return $"\r\n{name}\r\n{diagnostincsString}";
        }

        public static string GenerateSource(string classes, string validateFunction, string customMapper = null)
        {
            var source =
@"using NextGenMapper;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Test
{
    public class Program
    {
        public void TestMethod()
        {
"
            + validateFunction +
@"
        }
    }

    public class MapFailedException : System.Exception 
    {
        public object MapSource { get; set; }
        public object MapDestination { get; set; }

        public MapFailedException(object source, object destination) 
            : base()
        {
            MapSource = source;
            MapDestination = destination;
        }
    }
"
    + classes +
@"
}
";
            return customMapper is null ? source : source + customMapper;
        }
    }
}
