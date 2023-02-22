using Microsoft.CodeAnalysis;

namespace NextGenMapper.CodeAnalysis.Targets.MapTargets;

internal readonly struct ErrorMapTarget
{
    public ErrorMapTarget(Diagnostic diagnostic)
    {
        Diagnostic = diagnostic;
    }

    public Diagnostic Diagnostic { get; }
}
