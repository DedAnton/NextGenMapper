using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Benchmark.Utils;

internal static class SyntaxGenerator
{
    public static (MethodDeclarationSyntax Method, string SourceCode) GeneratePartialMapMethodAndSourceCode(int propertiesCount)
        => (GeneratePartialMapMethod(propertiesCount), GenerateSourceCode(propertiesCount));

    private static MethodDeclarationSyntax GeneratePartialMapMethod(int propertiesCount)
    {
        var nodes = new List<SyntaxNodeOrToken>();
        for(var i = 1; i <= propertiesCount * 2; i+=2)
        {
            nodes.Add(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName($"Property{i}{i+1}"),
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.AddExpression,
                                SyntaxFactory.IdentifierName($"Property{i}"),
                                SyntaxFactory.IdentifierName($"Property{i+1}"))));
            nodes.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
        }

        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.IdentifierName("UserDestination"),
            SyntaxFactory.Identifier("MyMap"))
        .WithModifiers(
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
        .WithParameterList(
            SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier("src"))
                    .WithType(
                        SyntaxFactory.IdentifierName("UserSource")))))
        .WithExpressionBody(
            SyntaxFactory.ArrowExpressionClause(
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("UserDestination"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression, 
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(nodes)))))
        .WithSemicolonToken(
            SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        .NormalizeWhitespace();
    }

    private static string GenerateSourceCode(int propertiesCount)
    {
        var fromProperties = new List<string>();
        var toProperties = new List<string>();
        for (int i = 1; i <= propertiesCount * 2; i+=2)
        {
            fromProperties.Add($"public int Property{i} {{ get; set; }}");
            fromProperties.Add($"public int Property{i+1} {{ get; set; }}");

            toProperties.Add($"public int Property{i}{i + 1} {{ get; set; }}");
        }

        return
$@"namespace Test;

public class Source
{{
    {string.Join("\r\n", fromProperties)}  
}}

public class Destination
{{
    {string.Join("\r\n", toProperties)}  
}}
";
    }
}