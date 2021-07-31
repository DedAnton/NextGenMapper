using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class ClassPartialConstructorMap : TypeMap
    {
        public MethodDeclarationSyntax Method { get; }

        public ClassPartialConstructorMap(ITypeSymbol from, ITypeSymbol to, MethodDeclarationSyntax method)
            : base(from, to)
        {
            Method = method;
        }
    }
}
