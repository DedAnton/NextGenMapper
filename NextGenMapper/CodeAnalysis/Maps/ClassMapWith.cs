using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps;

public class ClassMapWithStub : TypeMap
{
    public ParameterDescriptor[] Parameters { get; }

    public ClassMapWithStub(
        ITypeSymbol from,
        ITypeSymbol to,
        ParameterDescriptor[] parameters,
        Location mapLocaion)
        : base(from, to, mapLocaion)
    {
        Parameters = parameters;
    }
}

public class ClassMapWith : ClassMap
{
    public MapWithInvocationAgrument[] Arguments { get; }
    public List<ParameterDescriptor> Parameters { get; }

    public ClassMapWith(
        ITypeSymbol from,
        ITypeSymbol to, 
        IEnumerable<MemberMap> properties, 
        MapWithInvocationAgrument[] arguments, 
        List<ParameterDescriptor> parameters,
        Location mapLocaion)
        : base(from, to, properties, mapLocaion)
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