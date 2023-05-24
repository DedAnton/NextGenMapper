namespace NextGenMapper.Errors;

internal class MultipleInitializationError : MappingError
{
    public string ParameterName { get; }
    public string[] InitializedProperties { get; }
    public string InitializedPropertiesString { get; }

    public MultipleInitializationError(
        string parameterName,
        string[] initializedProperties)
        : base($"Constructor parameter {parameterName} initialize multiple properties'")
    {
        ParameterName = parameterName;
        InitializedProperties = initializedProperties;
        InitializedPropertiesString = string.Join(", ", initializedProperties);
    }
}
