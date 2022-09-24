using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps;

public class ClassMapWith : ClassMap
{
    public IPropertySymbol[] PublicPropertiesForParameters { get; }
    public List<MapWithInvocationAgrument> Arguments { get; }

    public ClassMapWith(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, List<MapWithInvocationAgrument> arguments)
        : base(from, to, properties)
    {
        PublicPropertiesForParameters = to.GetPublicProperties().ToArray();
        Arguments = arguments;
    }
}
