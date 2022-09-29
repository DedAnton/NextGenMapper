using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps;

public class ClassMapWith : ClassMap
{
    public List<MapWithInvocationAgrument> Arguments { get; }
    public List<ParameterDescriptor> Parameters { get; }

    public ClassMapWith(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, List<MapWithInvocationAgrument> arguments, List<ParameterDescriptor> parameters)
        : base(from, to, properties)
    {
        Arguments = arguments;
        Parameters = parameters;
    }
}

public class ParameterDescriptor
{
    public string Name { get; } 
    public ITypeSymbol Type { get; }

    public ParameterDescriptor(string name, ITypeSymbol type)
    {
        Name = name;
        Type = type;
    }
}