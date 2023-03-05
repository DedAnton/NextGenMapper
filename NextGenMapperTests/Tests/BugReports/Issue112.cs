namespace NextGenMapperTests.Tests.BugReports;

[TestClass]
public class Issue112 : SourceGeneratorVerifier
{
    public override string TestGroup => "BugReport";

    [TestMethod]
    public Task BugReport112_ShouldMap()
    {
        var source1 =
@"
using MapperTest;
using NextGenMapper;
using System.Collections.Generic;
using System.Linq;

namespace MapperTest
{
    public class ConfigKeyValueEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ConfigWithDict
    {
        public Dictionary<string, string> Configurations { get; set; }
    }

    public class ConfigWithList
    {
        public List<ConfigKeyValueEntry> Configurations { get; set; }
        public static ConfigWithList FromRowDefinition(ConfigWithDict configWithDict)
        {
            return configWithDict.Map<ConfigWithList>();
        }
    }
}

// ReSharper disable once CheckNamespace
namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static List<ConfigKeyValueEntry> Map<To>(this Dictionary<string, string> source)
        {
            return source.Select(t => new ConfigKeyValueEntry { Key = t.Key, Value = t.Value }).ToList();
        }
    }
}
";

        return VerifyOnly(source1);
    }
}
