namespace NextGenMapper.Mapping;

internal readonly struct Assignment
{
    public Assignment(string property, string parameter)
    {
        Property = property;
        Parameter = parameter;
    }

    public string Property { get; }
    public string Parameter { get; }
}
