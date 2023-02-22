namespace NextGenMapper.Mapping.Maps.Models;

internal readonly struct NameTypePair
{
    public NameTypePair(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public string Type { get; }
}
