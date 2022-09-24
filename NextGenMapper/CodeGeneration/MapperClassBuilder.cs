using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Linq;

namespace NextGenMapper.CodeGeneration;

public class MapperClassBuilder
{
    private const int TabWidth = 4;

    private const string Namespace = "namespace NextGenMapper";
    private const string Class = "internal static partial class Mapper";

    private const string NewLine = "\r\n";
    private const char OpenBracket = '(';
    private const char CloseBracket = ')';
    private const char OpenCurlyBracket = '{';
    private const char CloseCurlyBracket = '}';
    private const char OpenAngleBracket = '<';
    private const char CloseAngleBracket = '>';
    private const char Equal = '=';
    private const char Dot = '.';
    private const char Comma = ',';
    private const char Semicolon = ';';
    private const char Quotes = '"';
    private const char Underscore = '_';
    private const char WhiteSpace = ' ';
    private const string Lambda = "=>";
    private const char X = 'x';

    private const string Internal = "internal";
    private const string Static = "static";
    private const string Source = "source";
    private const string Map = "Map";
    private const string MapWith = "MapWith";
    private const string To = "To";
    private const string This = "this";
    private const string New = "new";
    private const string Switch = "switch";
    private const string Throw = "throw";
    private const string Select = "Select";
    private const string Return = "return";

    public string Generate(MapGroup mapGroup)
    {
        var builder = new ValueStringBuilder(stackalloc char[1024]);

        foreach (var @using in mapGroup.Usings)
        {
            builder.Append(@using);
            builder.Append(NewLine);
        };
        builder.Append(NewLine);
        builder.Append(Namespace);
        builder.Append(NewLine);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 1);
        builder.Append(Class);
        builder.Append(NewLine);
        AppendTabs(ref builder, 1);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        foreach (var map in mapGroup.Maps)
        {
            if (map is ClassMapWith classMapWith)
            {
                AppendPlaceholderClassMapWith(ref builder, classMapWith);
                if (classMapWith.Arguments.Count > 0)
                {
                    builder.Append(NewLine);
                    builder.Append(NewLine);
                    AppendClassMapWith(ref builder, classMapWith);
                }
            }
            else if (map is ClassMap classMap)
            {
                AppendCommonMapMethod(ref builder, classMap);
            }
            else if (map is EnumMap enumMap)
            {
                AppendEnumMapMethod(ref builder, enumMap);
            }
            else if (map is CollectionMap collectionMap)
            {
                AppendCollectionMapMethod(ref builder, collectionMap);
            }
            builder.Append(NewLine);
            builder.Append(NewLine);
        }
        AppendTabs(ref builder, 1);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(CloseCurlyBracket);

        return builder.ToString();
    }

    private void AppendClassMapWith(ref ValueStringBuilder builder, ClassMapWith map)
    {
        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(WhiteSpace);
        builder.Append(MapWith);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(map.From.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(Comma);
        builder.Append(NewLine);
        var counter = 1;
        foreach (var argument in map.Arguments)
        {
            AppendTabs(ref builder, 3);
            builder.Append(argument.Type.ToString());
            builder.Append(WhiteSpace);
            builder.Append(argument.Name);

            if (counter < map.Arguments.Count)
            {
                builder.Append(Comma);
            }
            builder.Append(NewLine);
            counter++;
        }
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        counter = 1;
        foreach (var property in map.ConstructorProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            var argument = map.Arguments.FirstOrDefault(x => property.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            if (argument is not null)
            {
                builder.Append(argument.Name);
            }
            else
            {
                builder.Append(Source);
                builder.Append(Dot);
                builder.Append(property.FromName);
                if (!(property.IsSameTypes || property.HasImplicitConversion))
                {
                    builder.Append(Dot);
                    builder.Append(Map);
                    builder.Append(OpenAngleBracket);
                    builder.Append(property.ToType.ToString());
                    builder.Append(CloseAngleBracket);
                    builder.Append(OpenBracket);
                    builder.Append(CloseBracket);
                }
            }
            if (counter < map.ConstructorProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2); ;
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(property.ToName);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            var argument = map.Arguments.FirstOrDefault(x => property.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            if (argument is not null)
            {
                builder.Append(argument.Name);
            }
            else
            {
                builder.Append(Source);
                builder.Append(Dot);
                builder.Append(property.FromName);
                if (!(property.IsSameTypes || property.HasImplicitConversion))
                {
                    builder.Append(Dot);
                    builder.Append(Map);
                    builder.Append(OpenAngleBracket);
                    builder.Append(property.ToType.ToString());
                    builder.Append(CloseAngleBracket);
                    builder.Append(OpenBracket);
                    builder.Append(CloseBracket);
                }
            }
            if (counter < map.InitializerProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendPlaceholderClassMapWith(ref ValueStringBuilder builder, ClassMapWith map)
    {
        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(WhiteSpace);
        builder.Append(MapWith);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(map.From.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(Comma);
        builder.Append(NewLine);
        var counter = 1;
        foreach (var property in map.PublicPropertiesForParameters)
        {
            AppendTabs(ref builder, 3);
            builder.Append(property.Type.ToString());
            builder.Append(WhiteSpace);
            builder.Append(property.Name.ToCamelCase());
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            builder.Append("default");

            if (counter < map.PublicPropertiesForParameters.Length)
            {
                builder.Append(Comma);
            }
            counter++;
            builder.Append(NewLine);
        }
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Throw);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        //TODO: add diagnostic for prevent calling stub method
        builder.Append("System.NotImplementedException(\"This method is a stub and is not intended to be called\")");
        builder.Append(Semicolon);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendCollectionMapMethod(ref ValueStringBuilder builder, CollectionMap map)
    {
        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(map.From.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(WhiteSpace);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(Dot);
        builder.Append(Select);
        builder.Append(OpenBracket);
        builder.Append(X);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(X);
        builder.Append(Dot);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(map.ItemTo.ToString());
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(CloseBracket);
        builder.Append(CloseBracket);
        builder.Append(Dot);
        if (map.CollectionType == CollectionType.List)
        {
            builder.Append("ToList");
        }
        else
        {
            builder.Append("ToArray");
        }
        builder.Append(OpenBracket);
        builder.Append(CloseBracket);
        builder.Append(Semicolon);
    }

    private void AppendEnumMapMethod(ref ValueStringBuilder builder, EnumMap map)
    {
        var from = map.From.ToString().AsSpan();
        var to = map.To.ToString().AsSpan();

        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(WhiteSpace);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(WhiteSpace);
        builder.Append(Switch);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        foreach (var field in map.Fields)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(field.FromType.ToString());
            builder.Append(Dot);
            builder.Append(field.FromName);
            builder.Append(WhiteSpace);
            builder.Append(Lambda);
            builder.Append(WhiteSpace);
            builder.Append(field.ToType.ToString());
            builder.Append(Dot);
            builder.Append(field.ToName);
            builder.Append(Comma);
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Underscore);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(Throw);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append("System.ArgumentOutOfRangeException");
        builder.Append(OpenBracket);
        builder.Append(Quotes);
        builder.Append("Error when mapping");
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append("to");
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(Quotes);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendCommonMapMethod(ref ValueStringBuilder builder, ClassMap map)
    {
        var from = map.From.ToString().AsSpan();
        var to = map.To.ToString().AsSpan();

        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(WhiteSpace);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        var counter = 1;
        foreach (var property in map.ConstructorProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(Source);
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.HasImplicitConversion))
            {
                builder.Append(Dot);
                builder.Append(Map);
                builder.Append(OpenAngleBracket);
                builder.Append(property.ToType.ToString());
                builder.Append(CloseAngleBracket);
                builder.Append(OpenBracket);
                builder.Append(CloseBracket);
            }
            if (counter < map.ConstructorProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2); ;
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(property.ToName);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            builder.Append(Source);
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.HasImplicitConversion))
            {
                builder.Append(Dot);
                builder.Append(Map);
                builder.Append(OpenAngleBracket);
                builder.Append(property.ToType.ToString());
                builder.Append(CloseAngleBracket);
                builder.Append(OpenBracket);
                builder.Append(CloseBracket);
            }
            if (counter < map.InitializerProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendTabs(ref ValueStringBuilder builder, int count)
    {
        for (int i = 0; i < count * TabWidth; i++)
        {
            builder.Append(WhiteSpace);
        }
    }
}