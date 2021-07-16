using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper
{
    public class PartialMapping : CustomMapping
    {
        public PartialMapping(ITypeSymbol from, ITypeSymbol to, MethodDeclarationSyntax method)
            : base(from, to, method)
        { }
    }
}
