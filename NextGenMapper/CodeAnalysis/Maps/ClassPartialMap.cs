using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialMap: TypeMap
    {
        public List<PropertyMap> InitializerProperties { get; }
        public List<ParameterMap> ConstructorProperties { get; }
        public List<StatementSyntax> CustomStatements { get; }
        public string ParameterName { get; }

        public ClassPartialMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<IMemberMap> properties, IEnumerable<StatementSyntax> customStatements, string customParameterName)
            : base(from, to)
        {
            InitializerProperties = properties.OfType<PropertyMap>().ToList();
            ConstructorProperties = properties.OfType<ParameterMap>().ToList();
            CustomStatements = customStatements.ToList();
            ParameterName = customParameterName;
        }
    }
}
