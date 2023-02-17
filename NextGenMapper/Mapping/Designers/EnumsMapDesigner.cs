using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Linq;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    private static Map DesignEnumsMap(ITypeSymbol source, ITypeSymbol destination, Location location)
    {
        var sourceFields = GetFields(source);
        var destinationFields = GetFields(destination);

        var fieldsMaps = new EnumFieldMap[sourceFields.Length];
        for (int i = 0; i < sourceFields.Length; i++)
        {
            var destinationFieldIdentifier = FindField(destinationFields, sourceFields[i].Identifier, sourceFields[i].Value);
            if (destinationFieldIdentifier is null)
            {
                var diagnostic = Diagnostics.UnmappedEnumValue(location, source, destination, sourceFields[i].Identifier);
                
                return Map.Error(source, destination, diagnostic);
            }

            fieldsMaps[i] = new EnumFieldMap(sourceFields[i].Identifier, destinationFieldIdentifier);
            //TODO: add warning diagnostic if 'to' has unmapped values (is this necessary?)
        }
        var immutableFieldsMap = Unsafe.CastArrayToImmutableArray(ref fieldsMaps);

        return Map.Enum(source.ToNotNullableString(), destination.ToNotNullableString(), immutableFieldsMap);
    }

    private static Span<EnumField> GetFields(ITypeSymbol enumTypeSymbol)
    {
        if (SourceCodeAnalyzer.FindFirstLocationSyntaxNode(enumTypeSymbol) is EnumDeclarationSyntax sourceDeclaration)
        {
            return GetFields(sourceDeclaration);
        }
        else
        {
            //TODO: refactoring
            //this is really a real case, because when we map types from dll, we don't have access to the syntax
            return enumTypeSymbol.GetMembers().OfType<IFieldSymbol>().Select(x => new EnumField(x.Name, x.ConstantValue?.UnboxToLong())).ToArray();
        }
    }

    private static Span<EnumField> GetFields(EnumDeclarationSyntax enumDeclaration)
    {
        var fields = new EnumField[enumDeclaration.Members.Count];
        for (int i = 0; i < enumDeclaration.Members.Count; i++)
        {
            fields[i] = new EnumField(
                enumDeclaration.Members[i].Identifier.ValueText,
                enumDeclaration.Members[i].EqualsValue?.Value?.As<LiteralExpressionSyntax>()?.Token.Value?.UnboxToLong());
        }

        return fields;
    }

    private static string? FindField(Span<EnumField> fields, string identifier, long? value)
    {
        foreach (var field in fields)
        {
            if (field.Identifier == identifier)
            {
                return field.Identifier;
            }

            if (value is not null
                && field.Value == value)
            {
                return field.Identifier;
            }
        }

        return null;
    }
}
