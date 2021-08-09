using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialMap : ClassMap
    {
        public List<StatementSyntax> CustomStatements { get; }
        public string ParameterName { get; }

        public ClassPartialMap(ITypeSymbol from, ITypeSymbol to, IEnumerable<MemberMap> properties, IEnumerable<StatementSyntax> customStatements, string customParameterName)
            : base(from, to, properties)
        {
            CustomStatements = customStatements.ToList();
            ParameterName = customParameterName;
        }
    }
}
