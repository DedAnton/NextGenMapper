using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextGenMapper;

namespace NextGenMapperTests.NewTests;

[TestClass]
public class TestVerify : SourceGeneratorVerifier
{
    [TestMethod]
    public Task Test()
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
        public object RunTest()
        {
            var source = new Source { Name = ""Anton"", Birthday = new DateTime(1997, 05, 20) };

            return source.Map<Destination>();
        }
    }

    public class Source
    {
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
    }
}
";
        return VerifyAndRun(source);
    }
}
