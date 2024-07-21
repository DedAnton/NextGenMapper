using Microsoft.CodeAnalysis;

namespace NextGenMapperTests;
internal class SourceException : Exception
{
    public SourceException(Diagnostic[] diagnostics)
        : base($"Source code compiled with errors ({diagnostics.Length}):\r\n\r\n"
            + string.Join("\r\n", diagnostics.Select(x => x.GetMessage())))
    {
        Diagnostics = diagnostics;
    }

    public Diagnostic[] Diagnostics { get; }
}

internal class NullableException : SourceException
{
    public NullableException(Diagnostic[] diagnostics) : base(diagnostics)
    {

    }
}
