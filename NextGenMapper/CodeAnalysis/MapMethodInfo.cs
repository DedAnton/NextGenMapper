using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;

public record MapMethodInfo(
    string Name,
    InvocationExpressionSyntax InvocationSyntax,
    MemberAccessExpressionSyntax InvocationMemberAccessSyntax,
    IMethodSymbol MethodSymbol,
    bool IsSuccessOverloadResolution,
    Location Location,
    SemanticModel SemanticModel);
