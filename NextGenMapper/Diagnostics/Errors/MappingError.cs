namespace NextGenMapper.Errors;

internal class MappingError
{
    public string Message { get; }

    public MappingError(string message)
    {
        Message = message;
    }
}
