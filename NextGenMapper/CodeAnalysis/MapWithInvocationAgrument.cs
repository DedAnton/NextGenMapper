using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis;
public class MapWithInvocationAgrument
{
    public string Name { get; }
    public ITypeSymbol Type { get; }

    public MapWithInvocationAgrument(string name, ITypeSymbol type)
    {
        Name = name;
        Type = type;
    }
}
