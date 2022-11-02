namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    private void MapMethodDifinition(string from, string to)
    {
        _builder.Append($"        internal static {to} Map<To>(this {from} source)");
    }
}
