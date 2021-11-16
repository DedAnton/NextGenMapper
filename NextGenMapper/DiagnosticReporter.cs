using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper
{
    public class DiagnosticReporter
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public void Report(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);

        public List<Diagnostic> GetDiagnostics() => _diagnostics;
    }
}
