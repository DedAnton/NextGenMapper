using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialMap : ClassMap
    {
        public List<StatementSyntax> CustomStatements { get; }
        public string ParameterName { get; }

        public ClassPartialMap(ITypeSymbol from, ITypeSymbol to, List<MemberMap> properties, List<StatementSyntax> customStatements, string customParameterName)
            : base(from, to, properties)
        {
            CustomStatements = customStatements;
            ParameterName = customParameterName;
        }
    }
}
