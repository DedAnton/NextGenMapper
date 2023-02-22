namespace NextGenMapper.Mapping.Maps.Models;

internal readonly struct EnumField
{
    public EnumField(string identifier, long? value)
    {
        Identifier = identifier;
        Value = value;
    }

    public string Identifier { get; }
    public long? Value { get; }
}
