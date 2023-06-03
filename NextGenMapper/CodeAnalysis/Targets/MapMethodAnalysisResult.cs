using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets;

internal record SuccessAnalysisResult(
    ITypeSymbol SourceType, 
    ITypeSymbol DestinationType, 
    IMethodSymbol MappingMethod, 
    bool IsSuccessOverloadResolution,
    Location Location);

internal record FailureAnalysisResult(Diagnostic? Error);

internal class MapMethodAnalysisResult
{
    private readonly bool _isSuccess = false;
    private readonly SuccessAnalysisResult? _successResult;
    private readonly FailureAnalysisResult? _failureResult;

    private MapMethodAnalysisResult(SuccessAnalysisResult successResult)
    {
        _isSuccess = true;
        _successResult = successResult;
    }

    private MapMethodAnalysisResult(FailureAnalysisResult failureResult)
    {
        _isSuccess = false;
        _failureResult = failureResult;
    }

    public static MapMethodAnalysisResult Success(
        ITypeSymbol sourceType, 
        ITypeSymbol destinationType, 
        IMethodSymbol mappingMethod,
        bool isSuccessOverloadResolution,
        Location location)
        => new(new SuccessAnalysisResult(sourceType, destinationType, mappingMethod, isSuccessOverloadResolution, location));

    public static MapMethodAnalysisResult Failure(Diagnostic? error = null)
        => new(new FailureAnalysisResult(error));

    public bool IsSuccess(
        [NotNullWhen(true)] out SuccessAnalysisResult? successResult, 
        [NotNullWhen(false)] out FailureAnalysisResult? failureResult)
    {
        if (_isSuccess)
        {
            successResult = _successResult!;
            failureResult = null;

            return true;
        }
        else 
        {
            successResult = null;
            failureResult = _failureResult!;

            return false;
        }
    }
}
