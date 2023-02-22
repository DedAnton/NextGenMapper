using Microsoft.CodeAnalysis;
using System;

namespace NextGenMapper.Mapping;

internal readonly ref struct ConstructorForMapping
{
    public IMethodSymbol? ConstructorSymbol { get; }
    public ReadOnlySpan<Assignment> Assignments { get; }

    public ConstructorForMapping()
    {
        ConstructorSymbol = null;
        Assignments = Array.Empty<Assignment>();
    }

    public ConstructorForMapping(IMethodSymbol constructorMethodSymbol, ReadOnlySpan<Assignment> assignments)
    {
        ConstructorSymbol = constructorMethodSymbol;
        Assignments = assignments;
    }

    public void Deconstruct(out IMethodSymbol? constructorSymbol, out ReadOnlySpan<Assignment> assignments)
    {
        constructorSymbol = ConstructorSymbol;
        assignments = Assignments;
    }
}