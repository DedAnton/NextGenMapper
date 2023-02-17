namespace NextGenMapper.Mapping.Maps;

internal readonly struct UserMap : IMap
{
    public UserMap(
        string source,
        string destination)
    {
        Source = source;
        Destination = destination;
    }

    public string Source { get; }
    public string Destination { get; }
}