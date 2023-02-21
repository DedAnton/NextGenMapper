using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    private static void DesignEnumsMap(ITypeSymbol source, ITypeSymbol destination, Location location, ref ValueListBuilder<Map> maps)
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
                maps.Append(Map.Error(source, destination, diagnostic));

                return;
            }

            fieldsMaps[i] = new EnumFieldMap(sourceFields[i].Identifier, destinationFieldIdentifier);
            //TODO: add warning diagnostic if 'to' has unmapped values (is this necessary?)
        }
        var immutableFieldsMap = Unsafe.CastArrayToImmutableArray(ref fieldsMaps);

        var map = Map.Enum(source.ToNotNullableString(), destination.ToNotNullableString(), immutableFieldsMap);
        maps.Append(map);
    }

    private static ReadOnlySpan<EnumField> GetFields(ITypeSymbol enumTypeSymbol)
    {
        if (enumTypeSymbol.GetFirstDeclarationSyntax() is EnumDeclarationSyntax sourceDeclaration)
        {
            return GetFieldsFromSyntax(sourceDeclaration);
        }
        else
        {
            //this is a case, because when we map types from dll, we don't have access to the syntax
            return GetFieldsFromSymbol(enumTypeSymbol);
        }
    }

    private static ReadOnlySpan<EnumField> GetFieldsFromSyntax(EnumDeclarationSyntax enumDeclaration)
    {
        var fields = new ValueListBuilder<EnumField>(enumDeclaration.Members.Count);
        for (int i = 0; i < enumDeclaration.Members.Count; i++)
        {
            var enumField = new EnumField(
                enumDeclaration.Members[i].Identifier.ValueText,
                UnboxToLong((enumDeclaration.Members[i].EqualsValue?.Value as LiteralExpressionSyntax)?.Token.Value));
            fields.Append(enumField);
        }

        return fields.AsSpan();
    }

    private static ReadOnlySpan<EnumField> GetFieldsFromSymbol(ITypeSymbol enumType)
    {
        var members = enumType.GetMembers().AsSpan();
        var fields = new ValueListBuilder<EnumField>(members.Length);
        foreach(var member in members)
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var enumField = new EnumField(fieldSymbol.Name, UnboxToLong(fieldSymbol.ConstantValue));
                fields.Append(enumField);
            }
        }

        return fields.AsSpan();
    }

    private static string? FindField(ReadOnlySpan<EnumField> fields, string identifier, long? value)
    {
        foreach (var field in fields)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals(field.Identifier, identifier))
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

    public static long? UnboxToLong(object? number)
        => number switch
        {
            null => null,
            sbyte => (sbyte)number,
            byte => (byte)number,
            short => (short)number,
            ushort => (ushort)number,
            int => (int)number,
            uint => (uint)number,
            long => (long)number,
            //TODO: maybe need handle this exception and add diagnostic
            _ => throw new ArgumentOutOfRangeException(nameof(number), $"{nameof(number)} must be sbyte, byte, short, ushort, int, uint or long.")
        };
}
