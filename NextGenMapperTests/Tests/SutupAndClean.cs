namespace NextGenMapperTests.Tests;

[TestClass]
public class SetupAndCleanup
{
    [AssemblyInitialize]
#pragma warning disable IDE0060
    public static void Initialize(TestContext context)
#pragma warning restore IDE0060
    {
        Directory.CreateDirectory(Path.Combine("..", "..", "..", "Temp"));
    }
}
