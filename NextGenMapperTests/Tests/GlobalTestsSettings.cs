using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace NextGenMapperTests.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.RegisterFileConverter<GeneratorDriverRunResult>(Convert);
        VerifySourceGenerators.Enable();

        VerifierSettings.DisableRequireUniquePrefix();
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontIgnoreEmptyCollections();
        VerifierSettings.AddExtraSettings(
        _ =>
        {
            _.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            _.NullValueHandling = NullValueHandling.Include;
            _.DefaultValueHandling = DefaultValueHandling.Include;
        });
    }

    static ConversionResult Convert(GeneratorDriverRunResult target, IReadOnlyDictionary<string, object> context)
    {
        var exceptions = new List<Exception>();
        var targets = new List<Target>();
        foreach (var result in target.Results)
        {
            if (result.Exception != null)
            {
                exceptions.Add(result.Exception);
            }

            var collection = result.GeneratedSources
                .Where(x => x.HintName is not "MapperExtensions.g.cs" and not "StartMapper.g.cs")
                .OrderBy(x => x.HintName)
                .Select(SourceToTarget);
            targets.AddRange(collection);
        }

        if (exceptions.Count == 1)
        {
            throw exceptions.First();
        }

        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }

        if (target.Diagnostics.Any())
        {
            var info = new
            {
                target.Diagnostics
            };
            return new(info, targets);
        }

        return new(null, targets);
    }

    static Target SourceToTarget(GeneratedSourceResult source)
    {
        var data = $@"//HintName: {source.HintName}
{source.SourceText}";
        return new("cs", data, Path.GetFileNameWithoutExtension(source.HintName));
    }
}