using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public interface IMember
    {
        public string Name { get; }
        public Type Type { get; }
    }

    public record Type(string Name, SpecialType SpecialType)
    {
        public List<Property> Properties { get; set; } = new();
        public List<Constructor> Constructors { get; set; } = new();
        public sealed override string ToString() => Name;
    }

    public record Property(string Name, Type Type, bool IsReadOnly) : IMember;
    
    public record Constructor(List<Parameter> Parameters);

    public record Parameter (string Name, Type Type) : IMember;

    public record Enum(string Name, List<EnumField> Fields) : Type(Name, SpecialType.System_Enum);

    public record EnumField(string Name, bool HasConstantValue, long? ConstantValue);

    public record Collection(string Name, SpecialType SpecialType, Type ElementType, CollectionType CollectionType) : Type(Name, SpecialType);

    public enum CollectionType
    {
        IEnumerable_T,
        List_T,
        IList_T,
        ICollection_T,
        IReadOnlyCollection_T,
        IReadOnlyList_T,
        Array,
        IImmutableList_T,
        ImmutableList_T,
        IImmutableArray_T,
        ImmutableArray_T
    }


    public class SyntaxAnalyzer
    {
        private readonly Dictionary<string, Type> _buildedTypes = new();

        public Type BuildType(ITypeSymbol typeSymbol)
        {
            var type = new Type(typeSymbol.ToDisplayString(), typeSymbol.SpecialType);
            _buildedTypes.Add(type.Name, type);

            var properties = typeSymbol.GetPublicProperties().Select(x => BuildProperty(x)).ToList();
            var construtors = typeSymbol.GetPublicConstructors().Select(x => BuildConstructor(x)).ToList();
            type.Properties = properties;
            type.Constructors = construtors;

            return type;
        }

        public Enum BuildEnum(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            if (typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not EnumDeclarationSyntax enumDeclaration)
            {
                //TODO: diagnostics
                throw new System.ArgumentException("enum must have declaration");
            }
            //TODO: try to get better drcision without invocating GetSymbol for every field
            var fields = enumDeclaration.GetFields(semanticModel)
                .Select(x => new EnumField(x.Name, x.HasConstantValue, x.ConstantValue?.UnboxToLong())).ToList();

            return new(typeSymbol.ToDisplayString(), fields);
        }

        public Collection BuildCollection(ITypeSymbol typeSymbol)
        {
            var collectionType = typeSymbol switch
            {
                { TypeKind: TypeKind.Array } => CollectionType.Array,
                { } when typeSymbol.IsGenericEnumerable() => CollectionType.List_T,
                _ => throw new System.NotImplementedException()
            };

            var elementTypeSymbol = typeSymbol switch
            {
                IArrayTypeSymbol array => array.ElementType,
                INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments.Single(),
                _ => throw new System.ArgumentOutOfRangeException($"Can`t get type of elements in collection {typeSymbol}")
            };

            return new(typeSymbol.ToDisplayString(), typeSymbol.SpecialType, BuildType(elementTypeSymbol), collectionType);
        }

        private Property BuildProperty(IPropertySymbol propertySymbol)
        {
            if (_buildedTypes.TryGetValue(propertySymbol.Type.ToDisplayString(), out var type))
            {
                return new(propertySymbol.Name, type, propertySymbol.IsReadOnly);
            }

            return new (propertySymbol.Name, BuildType(propertySymbol.Type), propertySymbol.IsReadOnly);
        }

        private Constructor BuildConstructor(IMethodSymbol constructorSymbol)
        {
            var parameters = constructorSymbol.Parameters.Select(x => BuildParameter(x)).ToList();

            return new(parameters);
        }

        private Parameter BuildParameter(IParameterSymbol parameterSymbol)
        {
            if (_buildedTypes.TryGetValue(parameterSymbol.Type.ToDisplayString(), out var type))
            {
                return new(parameterSymbol.Name, type);
            }

            return new(parameterSymbol.Name, BuildType(parameterSymbol.Type));
        }
    }
}
