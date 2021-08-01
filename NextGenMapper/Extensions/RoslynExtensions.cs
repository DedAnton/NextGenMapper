using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.Extensions
{
    public static class RoslynExtensions
    {
        public static bool IsGenericEnumerable(this ITypeSymbol type) =>
            type.AllInterfaces.Any(x => x.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            || type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T;

        public static List<IFieldSymbol> GetFields(this EnumDeclarationSyntax enumDeclaration, SemanticModel semanticModel)
            => enumDeclaration.Members.Select(x => semanticModel.GetDeclaredSymbol(x)).OfType<IFieldSymbol>().ToList();

        public static List<string> GetInitializersLeft(this ObjectCreationExpressionSyntax node)
            => node.Initializer?
            .Expressions
            .OfType<AssignmentExpressionSyntax>()
            .Select(x => x.Left.As<IdentifierNameSyntax>()?.Identifier.ValueText)
            .OfType<string>()
            .ToList() ?? new List<string>();

        public static bool HasSingleParameterWithType(this MethodDeclarationSyntax node)
            => node.ParameterList.Parameters.SingleOrDefault() is ParameterSyntax parameter
            && parameter?.Type is not null;

        public static List<MethodDeclarationSyntax> GetMethodsDeclarations(this ClassDeclarationSyntax node)
            => node.Members.Where(x => x.Kind() == SyntaxKind.MethodDeclaration).Cast<MethodDeclarationSyntax>().ToList();

        public static bool HasAttribute(this ISymbol? symbol, string attributeFullName)
            => symbol?.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == attributeFullName) ?? false;

        public static ISymbol? GetSymbol(this SemanticModel semanticModel, SyntaxNode node)
            => semanticModel.GetSymbolInfo(node).Symbol;

        public static T? As<T>(this ISymbol? symbol) where T : ISymbol => symbol is T tSymbol ? tSymbol : default;

        public static ITypeSymbol? GetTypeSymbol(this SemanticModel semanticModel, TypeSyntax type)
            => semanticModel.GetSymbol(type).As<ITypeSymbol>();

        public static IMethodSymbol? GetMethodSymbol(this SemanticModel semanticModel, ExpressionSyntax expression)
            => semanticModel.GetSymbol(expression).As<IMethodSymbol>();

        public static IReadOnlyList<IMethodSymbol> GetPublicConstructors(this ITypeSymbol type)
            => type.As<INamedTypeSymbol>()?.Constructors.Where(x => x.DeclaredAccessibility == Accessibility.Public).ToList() ?? new();

        public static IReadOnlyList<IMethodSymbol> OrderByParametersDesc(this IEnumerable<IMethodSymbol> methods) 
            => methods.OrderByDescending(x => x.Parameters.Count()).ToList();

        public static List<string> GetParametersNames(this IMethodSymbol method) => method.Parameters.Select(x => x.Name).ToList();

        public static IParameterSymbol? FindParameter(this IMethodSymbol method, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase) 
            => method?.Parameters.FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static List<IPropertySymbol> GetProperties(this ITypeSymbol type)
            => type.GetMembers().OfType<IPropertySymbol>()
            .Where(x => x.CanBeReferencedByName && x.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        public static IPropertySymbol? FindSettableProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetSettableProperties().FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static IPropertySymbol? FindProperty(this ITypeSymbol type, string name, StringComparison comparision = StringComparison.InvariantCultureIgnoreCase)
            => type?.GetProperties().FirstOrDefault(x => x.Name.Equals(name, comparision));

        public static List<IPropertySymbol> GetSettableProperties(this ITypeSymbol type) => type.GetProperties().Where(x => !x.IsReadOnly).ToList();

        public static List<string> GetPropertiesNames(this ITypeSymbol type) => type.GetProperties().Select(x => x.Name).ToList();

        public static T? As<T>(this SyntaxNode node) where T : SyntaxNode => node is T tNode ? tNode : default;

        public static List<string> GetUsings(this SyntaxNode node)
            => node.Ancestors().OfType<CompilationUnitSyntax>().Single().Usings.Select(x => x.ToString()).ToList();

        public static string GetNamespace(this SyntaxNode node)
            => node.Ancestors().OfType<NamespaceDeclarationSyntax>().Single().Name.ToString();

        public static SyntaxNode? GetFirstDeclaration(this ISymbol symbol)
            => symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        public static string? GetLeftAssigmentIdentifierName(this AssignmentExpressionSyntax assignmentExpression)
            => assignmentExpression.Left.As<IdentifierNameSyntax>()?.Identifier.Text;

        public static string? GetRightAssigmentIdentifierName(this AssignmentExpressionSyntax assignmentExpression)
            => assignmentExpression.Right.As<IdentifierNameSyntax>()?.Identifier.Text;

        public static T? GetExpression<T>(this BaseMethodDeclarationSyntax method) where T : ExpressionSyntax
            => method.ExpressionBody?.Expression.As<T>();

        public static List<StatementSyntax> GetStatements(this BaseMethodDeclarationSyntax method)
            => method.Body?.Statements.ToList() ?? new();

        public static ReturnStatementSyntax GetReturnStatement(this BaseMethodDeclarationSyntax method)
            => method.GetStatements().OfType<ReturnStatementSyntax>().Single();

        public static bool IsPrivitive(this ITypeSymbol type) => (int)type.SpecialType is int and >= 7 and <= 20;

        public static bool IsDefaultLiteralExpression(this ArgumentSyntax argument)
            => argument.Expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression;
    }
}
