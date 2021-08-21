using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class TypeCustomMap : TypeMap
    {
        public ArrowExpressionClauseSyntax? ExpressionBody { get; }
        public BlockSyntax? Body { get; }
        public MethodType MethodType { get; }
        public string ParameterName { get; }

        public TypeCustomMap(CustomMapMethod cutomMapMethod)
            :base(cutomMapMethod.Parameter.Type, cutomMapMethod.ReturnType)
        {
            ExpressionBody = cutomMapMethod.Syntax.ExpressionBody;
            Body = cutomMapMethod.Syntax.Body;
            MethodType = ExpressionBody != null ? MethodType.Expression : MethodType.Block;
            ParameterName = cutomMapMethod.Parameter.Name;
        }
    }
}
