using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace NextGenMapperTests
{
    public static class TestExtensions
    {
        public static IEnumerable<MetadataReference> GetReferences
            = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        public static Compilation CreateCompilation(this string source, string assemblyName)
            => CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp9)) },
            references: GetReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

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

        public static bool TestMapper(this Compilation compilation, out object source, out object destination, out string message, [CallerMemberName] string caller = "test")
        {
            var path = $@"..\..\..\Temp\{caller}.dll";
            var result = compilation.Emit(path);
            Assembly assembly = Assembly.LoadFrom(path);
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

            source = destination = message = null;
            return true;
        }

        public static bool IsFilteredEmpty(this IEnumerable<Diagnostic> diagnostics) => !diagnostics.Where(x => x.Id != "CS8019").Any();

        public static string GetObjectsString(object source, object destination, string message)
        {
            var sourceJson = JsonSerializer.Serialize(source, new JsonSerializerOptions{ WriteIndented = true });
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

        public static string GenerateSource(string classes, string validateFunction, string customMapping = "")
        {
            var source =
@"using NextGenMapper;
using System;
using System.Collections.Generic;
using System.Linq;

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
    [Mapper]
    public class CustomMapper
    {
"   + customMapping +
@"
    }
}
";
            return source;
        }
    }
}
