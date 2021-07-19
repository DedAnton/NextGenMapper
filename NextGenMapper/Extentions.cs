using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    public static class Extentions
    {

        public static bool HasAttribute(this ISymbol symbol, string attributeFullName)
            => symbol?.GetAttributes().Any(x => x.AttributeClass.ToDisplayString() == attributeFullName) ?? false;

        public static ISymbol GetDeclaredSymbol(this GeneratorSyntaxContext context, SyntaxNode node)
            => context.SemanticModel.GetDeclaredSymbol(node);
        
        public static ISymbol GetSymbol(this GeneratorSyntaxContext context, SyntaxNode node)
            => context.SemanticModel.GetSymbolInfo(node).Symbol;

        public static ITypeSymbol GetTypeSymbol(this GeneratorSyntaxContext context, TypeSyntax node)
            => context.GetSymbol(node) as ITypeSymbol;

        public static List<IPropertySymbol> GetProperties(this ITypeSymbol type)
            => type.GetMembers().Where(x => x.Kind == SymbolKind.Property).Select(x => x as IPropertySymbol).ToList();

        public static string LeadingSpace(this string source, int addLeadingSpacesCount)
        {
            var splittedLines = source.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var strBuilder = new StringBuilder();
            foreach(var line in splittedLines)
            {
                if (addLeadingSpacesCount > 0)
                {
                    strBuilder.AppendLine(line.PadLeft(line.Length + addLeadingSpacesCount));
                }
                else
                {
                    var removeCount = Math.Abs(addLeadingSpacesCount);
                    if (removeCount < line.Length)
                    {
                        var trimmed = line.Substring(0, removeCount).TrimStart();
                        strBuilder.AppendLine(trimmed + line.Substring(removeCount));
                    }
                    else
                    {
                        strBuilder.AppendLine(line.TrimStart());
                    }
                }
            }

            return strBuilder.ToString();
        }

        public static string RemoveLeadingSpace(this string source, int count) => source.LeadingSpace(count * -1);

        public static MethodDeclarationSyntax ThrowIfNotValid(this MethodDeclarationSyntax method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (method.ParameterList.Parameters.SingleOrDefault() == null)
            {
                throw new ArgumentException("Custom mapper must have one parameter");
            }

            return method;
        }

        public static T ThrowIfNull<T>(this T argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException();
            }

            return argument;
        }

        public static bool HasDefaultConstructor(this ITypeSymbol type)
            => type is INamedTypeSymbol namedType
            && namedType.Constructors
            .Any(x => x.DeclaredAccessibility == Accessibility.Public && x.Parameters.Count() == 0);

        public static List<MethodDeclarationSyntax> GetMethodsDeclarations(this ClassDeclarationSyntax node)
            => node.Members.Where(x => x.Kind() == SyntaxKind.MethodDeclaration).Select(x => x as MethodDeclarationSyntax).ToList();

        public static bool IsStartMapperInvocation(this InvocationExpressionSyntax node, GeneratorSyntaxContext context)
            => context.GetSymbol(node.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom.ToDisplayString() == StartMapperSource.FunctionFullName;

        public static List<string> GetInitializersLeft(this ObjectCreationExpressionSyntax node)
            => node
            .Initializer?
            .Expressions
            .Select(x => ((x as AssignmentExpressionSyntax)
                .Left as IdentifierNameSyntax)
                .Identifier
                .ValueText)
            .ToList() ?? new();

        public static List<UsingDirectiveSyntax> GetUsings(this ClassDeclarationSyntax node)
            => (node.Parent.Parent as CompilationUnitSyntax).Usings.ToList();

        public static bool HasOneParameter(this MethodDeclarationSyntax node)
            => node.ParameterList.Parameters.SingleOrDefault() != null;

        public static ParameterSyntax GetSingleParameter(this MethodDeclarationSyntax node)
            => node.ParameterList.Parameters.Single();
    }
}
