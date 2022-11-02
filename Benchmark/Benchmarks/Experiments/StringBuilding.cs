using NextGenMapper.CodeGeneration;
using NextGenMapper.Utils;
using System.Text;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class StringBuilding
{
    private readonly string _value = int.MaxValue.ToString();

    [Benchmark]
    public string Interpolation() => $"value: {_value}";

    [Benchmark]
    public string StringBuilder()
    {
        var sb = new StringBuilder();
        sb.Append("value: ");
        sb.Append(_value);

        return sb.ToString();
    }

    [Benchmark]
    public string ValueStringBuilder()
    {
        var sb = new ValueStringBuilder();
        sb.Append("value: ");
        sb.Append(_value);

        return sb.ToString();
    }

    [Benchmark]
    public string Interpolation10() => $"value: {_value}, value: {_value}, value: {_value}, value: {_value}, value1: {_value}, value: {_value}, value: {_value}, value: {_value}, value: {_value}, value: {_value}";

    [Benchmark]
    public string ValueStringBuilder10()
    {
        var sb = new ValueStringBuilder();
        sb.Append("value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        sb.Append(",value: ");
        sb.Append(_value);
        return sb.ToString();
    }
}
