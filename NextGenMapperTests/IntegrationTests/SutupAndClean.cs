using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace NextGenMapperTests.IntegrationTests
{
    [TestClass]
    public class InitializeAndCleanup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Directory.CreateDirectory(@"..\..\..\Temp");
        }
    }
}
