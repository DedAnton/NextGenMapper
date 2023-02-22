namespace NextGenMapper.Mapping.Maps.Models;

internal readonly struct EnumFieldMap
{
    public EnumFieldMap(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }

    public string Source { get; }
    public string Destination { get; }
}
