using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace NetEscapades.EnumGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
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
}