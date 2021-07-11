using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NextGenMapper
{
    class Mapping
    {
        public INamedTypeSymbol From { get; set; }
        public INamedTypeSymbol To { get; set; }
        public bool Reverse { get; set; }
        public Dictionary<string, string> TargetNames { get; set; } = new Dictionary<string, string>();
    }
}
