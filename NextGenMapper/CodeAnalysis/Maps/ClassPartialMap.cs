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
        public ArrowExpressionClauseSyntax? ExpressionBody { get; }
        public BlockSyntax? Body { get; }
        public MethodType MethodType { get; }
        public string ParameterName { get; }

        public ClassPartialMap(ITypeSymbol from, ITypeSymbol to, List<IMemberMap> properties, MethodDeclarationSyntax method)
            : base(from, to)
        {
            InitializerProperties = properties.OfType<PropertyMap>().ToList();
            ConstructorProperties = properties.OfType<ParameterMap>().ToList();
            ExpressionBody = method.ExpressionBody;
            Body = method.Body;
            MethodType = method.Body is null ? MethodType.Expression : MethodType.Block;
            ParameterName = method.ParameterList.Parameters.First().Identifier.Text;
        }
    }
}
