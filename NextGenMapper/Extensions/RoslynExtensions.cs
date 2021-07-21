using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static ISymbol GetSymbol(this SemanticModel semanticModel, SyntaxNode node)
            => semanticModel.GetSymbolInfo(node).Symbol;

        public static T As<T>(this ISymbol symbol) where T : ISymbol => symbol is T tSymbol ? tSymbol : default;

        public static ITypeSymbol GetTypeSymbol(this SemanticModel semanticModel, TypeSyntax type) 
            => semanticModel.GetSymbol(type).As<ITypeSymbol>();

        public static IMethodSymbol GetMethodSymbol(this SemanticModel semanticModel, ExpressionSyntax expression)
            => semanticModel.GetSymbol(expression).As<IMethodSymbol>();

        public static IReadOnlyList<IMethodSymbol> GetPublicConstructors(this ITypeSymbol type)
            => type.As<INamedTypeSymbol>()?.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public).ToList() ?? new();

        public static IReadOnlyList<IMethodSymbol> OrderByParametersDesc(this IEnumerable<IMethodSymbol> methods) 
            => methods.OrderByDescending(x => x.Parameters.Count()).ToList();

        public static List<string> GetParametersNames(this IMethodSymbol method) => method.Parameters.Select(x => x.Name).ToList();

        public static IParameterSymbol FindParameter(this IMethodSymbol method, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase) 
            => method?.Parameters.FirstOrDefault(x => x.Name.Equals(name, comparision));
        public static List<string> ToUpperInvariant(this IEnumerable<string> strings) => strings.Select(x => x.ToUpperInvariant()).ToList();

        public static List<IPropertySymbol> GetProperties(this ITypeSymbol type)
            => type.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol).ToList();

        public static IPropertySymbol FindSettableProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetSettableProperties().FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static IPropertySymbol FindProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetProperties().FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static List<IPropertySymbol> GetSettableProperties(this ITypeSymbol type) => type.GetProperties().Where(x => !x.IsReadOnly).ToList();

        public static List<string> GetPropertiesNames(this ITypeSymbol type) => type.GetProperties().Select(x => x.Name).ToList();

        public static T As<T>(this SyntaxNode node) where T : SyntaxNode => node is T tNode ? tNode : default;

        public static List<string> GetUsings(this SyntaxNode node)
            => node.Ancestors().OfType<CompilationUnitSyntax>().Single().Usings.Select(x => x.ToString()).ToList();

        public static T GetFirstDeclaration<T>(this ISymbol symbol) where T : SyntaxNode 
            => symbol.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax().As<T>();

        public static SyntaxNode GetSyntaxNode(this ISymbol symbol)
            => symbol.Locations.FirstOrDefault() is { } location ? location.SourceTree?.GetRoot().FindNode(location.SourceSpan) : null;

        public static string GetLeftAssigmentIdentifierName(this AssignmentExpressionSyntax assignmentExpression)
            => assignmentExpression.Left.As<IdentifierNameSyntax>()?.Identifier.Text;

        public static string GetRightAssigmentIdentifierName(this AssignmentExpressionSyntax assignmentExpression)
            => assignmentExpression.Right.As<IdentifierNameSyntax>()?.Identifier.Text;

        public static T GetExpression<T>(this BaseMethodDeclarationSyntax method) where T : ExpressionSyntax
            => method?.ExpressionBody?.Expression.As<T>();

        public static List<StatementSyntax> GetStatements(this BaseMethodDeclarationSyntax method)
            => method?.Body?.Statements.ToList() ?? new();

        public static ReturnStatementSyntax GetReturnStatement(this BaseMethodDeclarationSyntax method)
            => method?.GetStatements().OfType<ReturnStatementSyntax>().FirstOrDefault();

        public static bool IsPrivitive(this ITypeSymbol type) => (int)type.SpecialType is int and >= 7 and <= 20;
    }
}
