using Microsoft.CodeAnalysis;
using NextGenMapper.Errors;
using System;

namespace NextGenMapper.Mapping;

internal readonly ref struct ConstructorForMapping
{
    public IMethodSymbol? ConstructorSymbol { get; }
    public ReadOnlySpan<Assignment> Assignments { get; }
    public MappingError? Error { get; }

    public ConstructorForMapping()
    {
        ConstructorSymbol = null;
        Assignments = ReadOnlySpan<Assignment>.Empty;
        Error = null;
    }

    public ConstructorForMapping(IMethodSymbol constructorMethodSymbol)
    {
        ConstructorSymbol = constructorMethodSymbol;
        Assignments = ReadOnlySpan<Assignment>.Empty;
        Error = null;
    }

    public ConstructorForMapping(IMethodSymbol constructorMethodSymbol, ReadOnlySpan<Assignment> assignments)
    {
        ConstructorSymbol = constructorMethodSymbol;
        Assignments = assignments;
        Error = null;
    }

    public ConstructorForMapping(MappingError error)
    {
        ConstructorSymbol = null;
        Assignments = ReadOnlySpan<Assignment>.Empty;
        Error = error;
    }

    public void Deconstruct(out IMethodSymbol? constructorSymbol, out ReadOnlySpan<Assignment> assignments, out MappingError? error)
    {
        constructorSymbol = ConstructorSymbol;
        assignments = Assignments;
        error = Error;
    }
}