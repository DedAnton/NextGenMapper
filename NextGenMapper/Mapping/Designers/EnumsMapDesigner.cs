using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    private static void DesignEnumsMap(
        ITypeSymbol source, 
        ITypeSymbol destination, 
        Location location, 
        ref ValueListBuilder<Map> maps, 
        CancellationToken cancellationToken)
    {
        ReadOnlySpan<EnumField> sourceFields;
        ReadOnlySpan<EnumField> destinationFields;
        try
        {
            sourceFields = GetFields(source);
            destinationFields = GetFields(destination);
        }
        catch (ArgumentOutOfRangeException)
        {
            var diagnostic = Diagnostics.UnsupportedEnumType(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));
            return;
        }

        var fieldsMaps = new EnumFieldMap[sourceFields.Length];
        for (int i = 0; i < sourceFields.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destinationFieldIdentifier = FindField(destinationFields, sourceFields[i].Identifier, sourceFields[i].Value);
            if (destinationFieldIdentifier is null)
            {
                var diagnostic = Diagnostics.UnmappedEnumValue(location, source, destination, sourceFields[i].Identifier);
                maps.Append(Map.Error(source, destination, diagnostic));

                return;
            }

            fieldsMaps[i] = new EnumFieldMap(sourceFields[i].Identifier, destinationFieldIdentifier);
        }
        var immutableFieldsMap = Unsafe.CastArrayToImmutableArray(ref fieldsMaps);

        var map = Map.Enum(source.ToNotNullableString(), destination.ToNotNullableString(), immutableFieldsMap);
        maps.Append(map);
    }

    private static ReadOnlySpan<EnumField> GetFields(ITypeSymbol enumType)
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
            _ => throw new ArgumentOutOfRangeException(nameof(number), $"{nameof(number)} must be sbyte, byte, short, ushort, int, uint or long.")
        };
}
