using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.Models;

internal readonly struct MapMethodAnalysisResult
{
    public static MapMethodAnalysisResult Success(ITypeSymbol sourceType, ITypeSymbol destinationType) => new(true, sourceType, destinationType);
    public static MapMethodAnalysisResult Fail() => new(false, null, null);

    private MapMethodAnalysisResult(bool isSuccess, ITypeSymbol? sourceType, ITypeSymbol? destinationType)
    {
        IsSuccess = isSuccess;
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    public bool IsSuccess { get; }
    public ITypeSymbol? SourceType { get; }
    public ITypeSymbol? DestinationType { get; }

    public void Deconstruct(out bool isSuccess, out ITypeSymbol? sourceType, out ITypeSymbol? destinationType)
    {
        isSuccess = IsSuccess;
        sourceType = SourceType;
        destinationType = DestinationType;
    }
}
