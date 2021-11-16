using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class InitializeAndCleanup
    {
        [AssemblyInitialize]
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        public static void Initialize(TestContext context)
#pragma warning restore IDE0060
        {
            Directory.CreateDirectory(@"..\..\..\Temp");
        }
    }
}
