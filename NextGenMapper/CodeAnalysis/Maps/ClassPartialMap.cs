using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialMap : ClassMap
    {
        public ImmutableArray<StatementSyntax> CustomStatements { get; }
        public string ParameterName { get; }

        public ClassPartialMap(Type from, Type to, IEnumerable<MemberMap> properties, PartialMapMethod customMapMethod)
            : base(from, to, properties)
        {
            CustomStatements = customMapMethod.Statements;
            ParameterName = customMapMethod.Parameter.Name;
        }
    }
}
