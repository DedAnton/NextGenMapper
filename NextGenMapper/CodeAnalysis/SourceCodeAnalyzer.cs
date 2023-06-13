using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NextGenMapper.CodeAnalysis;

internal static class SourceCodeAnalyzer
{
    public static bool IsTypesAreEquals(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => SymbolEqualityComparer.Default.Equals(sourceType, destinationType);

    public static bool IsTypesHasImplicitConversion(ITypeSymbol sourceType, ITypeSymbol destinationType, SemanticModel semanticModel)
        => semanticModel.Compilation.HasImplicitConversion(sourceType, destinationType);

    public static bool IsTypesAreEnums(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.TypeKind == TypeKind.Enum && destinationType.TypeKind == TypeKind.Enum;

    public static bool IsTypesAreCollections(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.IsCollection() && destinationType.IsCollection();

    private static bool IsCollection(this ITypeSymbol type)
    {
        if (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            return true;
        }

        foreach (var @interface in type.AllInterfaces)
        {
            if (@interface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsTypesAreClasses(ITypeSymbol sourceType, ITypeSymbol destinationType)
        => sourceType.TypeKind == TypeKind.Class && destinationType.TypeKind == TypeKind.Class;

    public static bool IsNamedArgument(this ArgumentSyntax argument) => argument.NameColon?.Name.Identifier.ValueText is not null;

    public static bool IsAllArgumentsNamed(this SeparatedSyntaxList<ArgumentSyntax> argumentList)
    {
        foreach (var argument in argumentList)
        {
            if (!argument.IsNamedArgument())
            {
                return false;
            }
        }

        return true;
    }
}