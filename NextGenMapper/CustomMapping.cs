using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace NextGenMapper
{
    public class CustomMapping : Mapping
    {
        public ArrowExpressionClauseSyntax ExpressionBody { get; }
        public BlockSyntax Body { get; }
        public string ParameterName { get; }

        public CustomMapping(ITypeSymbol from, ITypeSymbol to, MethodDeclarationSyntax method)
            : base(from, to)
        {
            ExpressionBody = method.ExpressionBody;
            Body = method.Body;
            ParameterName = method.ParameterList.Parameters.SingleOrDefault()?.Identifier.Text 
                ?? throw new ArgumentException("method must contains one parameter");
        }
    }
}
