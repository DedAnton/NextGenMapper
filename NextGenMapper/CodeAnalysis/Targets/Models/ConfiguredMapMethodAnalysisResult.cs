using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.Models;

internal readonly struct ConfiguredMapMethodAnalysisResult
{
    public static ConfiguredMapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType, bool isCompleteMethod) => new(true, sourceType, destinationType, isCompleteMethod);
    public static ConfiguredMapMethodAnalysisResult Fail() => new(false, null, null, false);

    private ConfiguredMapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType, bool isCompleteMethod)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
        IsCompleteMethod = isCompleteMethod;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }
    public bool IsCompleteMethod { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType, out bool isCompleteMethod)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
        isCompleteMethod = IsCompleteMethod;
    }
}
