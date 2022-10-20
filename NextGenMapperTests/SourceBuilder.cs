using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGenMapperTests;

internal class CompilationUnitSourceBuilder
{
    private List<string> Usings { get; } = new List<string>();
}

internal class ClassSourceBuilder
{
    private int _currentConstructor = -1;

    private string Name { get; set; } = "DefaultClassName";
    private List<string> TypeArguments { get; } = new();
    private List<Property> Properties { get; } = new();
    private List<Constructor> Constructors { get; } = new();

    public ClassSourceBuilder WithName(string name)
    {
        Name = name;

        return this;
    }

    public ClassSourceBuilder WithTypeArgument(string type)
    {
        TypeArguments.Add(type);

        return this;
    }

    public ClassSourceBuilder WithProperty(
        AccessModifier accessModifier,
        string type, string name,
        ReadAccessor readAccessor = ReadAccessor.None,
        WriteAccessor writeAccessor = WriteAccessor.None,
        bool isAutoProperty = true)
    {
        var property = new Property(accessModifier, type, name, readAccessor, writeAccessor, isAutoProperty);
        Properties.Add(property);

        return this;
    }

    public ClassSourceBuilder WithReadonlyProperty(
        AccessModifier accessModifier,
        string type, 
        string name,
        bool isAutoProperty = true)
    {
        var property = new Property(accessModifier, type, name, ReadAccessor.Get, WriteAccessor.None, isAutoProperty);
        Properties.Add(property);

        return this;
    }

    public ClassSourceBuilder WithProperties(
        int count,
        Func<int, AccessModifier> accessModifier,
        Func<int, string> type, 
        Func<int, string> name,
        Func<int, ReadAccessor> readAccessor,
        Func<int, WriteAccessor> writeAccessor)
    {
        for (var i = 0; i < count; i++)
        {
            var property = new Property(accessModifier(i), type(i), name(i), readAccessor(i), writeAccessor(i));
            Properties.Add(property);
        }

        return this;
    }

    public ClassSourceBuilder WithConstructor(AccessModifier accessModifier = AccessModifier.Public)
    {
        var constructor = new Constructor(accessModifier, new());
        Constructors.Add(constructor);
        _currentConstructor++;

        return this;
    }

    public ClassSourceBuilder WithConstructorParamter(string type, string name, string? initializedPropertyName)
    {
        var parameter = new Parameter(type, name, initializedPropertyName);
        Constructors[_currentConstructor].Parameters.Add(parameter);

        return this;
    }

    public ClassSourceBuilder WithConstructorParamters(int count, Func<int, string> type, Func<int, string> name, Func<int, string?> initializedPropertyName)
    {
        for (var i = 0; i < count; i++)
        {
            var parameter = new Parameter(type(i), name(i), initializedPropertyName(i));
            Constructors[_currentConstructor].Parameters.Add(parameter);
        }

        return this;
    }
}

public record Parameter(string Type, string Name, string? InitializedPropertyName);

public record Constructor(
    AccessModifier AccessModifier,
    List<Parameter> Parameters);

public record Property(
    AccessModifier AccessModifier, 
    string Type, string Name, 
    ReadAccessor ReadAccessor = ReadAccessor.None, 
    WriteAccessor WriteAccessor = WriteAccessor.None, 
    bool IsAutoProperty = true);

public enum AccessModifier
{
    Public,
    Private,
    Protected,
    Internal,
    ProtectedInternal,
    PrivateProtected
}

public enum ReadAccessor
{
    None,
    Get
}

public enum WriteAccessor
{
    None,
    Set,
    Init
}