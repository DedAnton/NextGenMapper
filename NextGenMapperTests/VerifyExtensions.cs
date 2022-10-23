namespace NextGenMapperTests;

internal static partial class VerifyExtensions
{
    public static SettingsTask UseGeneratorResultSettings(this SettingsTask settingsTask, string directory)
        => settingsTask.UseMySettings(directory);

    public static SettingsTask UseMapResultSettings(this SettingsTask settingsTask, string directory)
        => settingsTask.UseMySettings(Path.Combine(directory, "MapResult"));
}
