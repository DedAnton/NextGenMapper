using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.Models;

internal readonly struct UserMapMethodAnalysisResult
{
    public static UserMapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType, IMethodSymbol method)
        => new(true, sourceType, destinationType, method);
    public static UserMapMethodAnalysisResult Fail() => new(false, null, null, null);

    private UserMapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType, IMethodSymbol? method)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
        Method = method;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }
    public IMethodSymbol? Method { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType, out IMethodSymbol? method)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
        method = Method;
    }
}
