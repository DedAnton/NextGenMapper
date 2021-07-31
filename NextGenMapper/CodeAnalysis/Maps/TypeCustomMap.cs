using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class TypeCustomMap : TypeMap
    {
        public ArrowExpressionClauseSyntax? ExpressionBody { get; }
        public BlockSyntax? Body { get; }
        public MethodType MethodType { get; }
        public string ParameterName { get; }

        public TypeCustomMap(ITypeSymbol from, ITypeSymbol to, MethodDeclarationSyntax method)
            :base(from, to)
        {
            ExpressionBody = method.ExpressionBody;
            Body = method.Body;
            MethodType = method.Body is null ? MethodType.Expression : MethodType.Block;
            ParameterName = method.ParameterList.Parameters.First().Identifier.Text;
        }
    }
}
