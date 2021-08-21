using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NextGenMapper.CodeAnalysis.Models
{
    public interface IMember
    {
        public string Name { get; }
        public Type Type { get; }
    }

    public class PartialConstructorMapMethod : MapMethod
    {
        public PartialConstructorMapMethod(
            ObjectCreationExpression objectCreationExpression, 
            ImmutableArray<ArgumentSyntax> arguments, 
            ImmutableArray<AssignmentExpressionSyntax> initializerExpressions,
            Parameter parameter,
            Type returnType)
            :base(parameter, returnType)
        {
            ObjectCreationExpression = objectCreationExpression;
            Arguments = arguments;
            InitializerExpressions = initializerExpressions;
        }

        public ObjectCreationExpression ObjectCreationExpression { get; }
        public ImmutableArray<ArgumentSyntax> Arguments { get; }
        public ImmutableArray<AssignmentExpressionSyntax> InitializerExpressions { get; }
    }

    public class PartialMapMethod : MapMethod
    {
        public PartialMapMethod(ObjectCreationExpression objectCreationExpression, ImmutableArray<StatementSyntax> statements, Parameter parameter, Type returnType)
            :base(parameter, returnType)
        {
            ObjectCreationExpression = objectCreationExpression;
            Statements = statements;
        }

        public ObjectCreationExpression ObjectCreationExpression { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
    }

    public class CustomMapMethod : MapMethod
    {
        public CustomMapMethod(MethodDeclarationSyntax syntax, Parameter parameter, Type returnType)
            :base(parameter, returnType)
        {
            Syntax = syntax;
        }

        public MethodDeclarationSyntax Syntax { get; }
    }

    public class MapMethod
    {
        public MapMethod(Parameter parameter, Type returnType)
        {
            Parameter = parameter;
            ReturnType = returnType;
        }

        public Parameter Parameter { get; }
        public Type ReturnType { get; }
    }

    public class Parameter : IMember
    {
        public Parameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }

    public class ObjectCreationExpression
    {
        public ObjectCreationExpression(Constructor constructor, ImmutableArray<string> initializerPropertiesNames)
        {
            Constructor = constructor;
            InitializerPropertiesNames = initializerPropertiesNames;
        }

        public Constructor Constructor { get; }
        public ImmutableArray<string> InitializerPropertiesNames { get; }
    }

    public class Property : IMember
    {
        public Property(string name, Type type, bool isReadOnly)
        {
            Name = name;
            Type = type;
            IsReadOnly = isReadOnly;
        }

        public string Name { get; }
        public Type Type { get; }
        public bool IsReadOnly { get; }
        public string TypeName => Type.Name;
    }

    public class Enum : Type
    {
        public Enum(
            ImmutableArray<EnumField> fields,
            string name)
            :base(name, false, SpecialType.System_Enum, new(), new())
        {
            Fields = fields;
        }

        public ImmutableArray<EnumField> Fields { get; }
    }

    public class EnumField : IMember
    {
        public EnumField(string name, Type type, bool hasValue, long? value)
        {
            Name = name;
            Type = type;
            HasValue = hasValue;
            Value = value;
        }

        public string Name { get; }
        public Type Type { get; }
        public bool HasValue { get; }
        public long? Value { get; }
    }

    public class Type : IEquatable<Type>
    {
        public Type(string name, bool isPrimitive, SpecialType specialType, ImmutableArray<Property> properties, ImmutableArray<Constructor> constructors)
        {
            Name = name;
            IsPrimitive = isPrimitive;
            SpecialType = specialType;
            Properties = properties;
            Constructors = constructors;
        }

        public string Name { get; }
        public bool IsPrimitive { get; }
        public SpecialType SpecialType { get; }
        public ImmutableArray<Property> Properties { get; }
        public ImmutableArray<Constructor> Constructors { get; }
        public override string ToString() => Name;

        public bool Equals(Type other) => Name == other.Name;
        public override bool Equals(object? obj) => obj is Type type && Equals(type);

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator ==(Type left, Type right) => left.Equals(right);
        public static bool operator !=(Type left, Type right) => !left.Equals(right);
    }

    public class Constructor
    {
        public Constructor(Type constructedType, ImmutableArray<Parameter> parameters)
        {
            ConstructedType = constructedType;
            Parameters = parameters;
        }

        public Type ConstructedType { get; }
        public ImmutableArray<Parameter> Parameters { get; }
    }

    public class Collection : Type
    {
        public Collection(
            Type elementType, 
            bool isArray, 
            string name,
            SpecialType specialType)
            :base(name, false, specialType, new(), new())
        {
            ElementType = elementType;
            IsArray = isArray;
        }

        public Type ElementType { get; }
        public bool IsArray { get; }
    }
}
