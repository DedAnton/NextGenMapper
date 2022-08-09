using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;

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
    private const string To = "To";
    private const string This = "this";
    private const string New = "new";
    private const string Switch = "switch";
    private const string Throw = "throw";
    private const string Select = "Select";
    private const string UserFunction = "UserFunction";
    private const string Result = "result";
    private const string Var = "var";
    private const string Return = "return";
    private const string SpecialPrefix = "_a___";

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
            if (map is ClassPartialMap partialMap)
            {
                AppendClassPartialMapMethod(ref builder, partialMap);
            }
            else if (map is ClassPartialConstructorMap partialConstructorMap)
            {
                AppendClassPartialConstructorMapMethod(ref builder, partialConstructorMap);
            }
            else if (map is ClassMap classMap)
            {
                if (!classMap.IsUnflattening)
                {
                    AppendCommonMapMethod(ref builder, classMap);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (map is TypeCustomMap typeCustomMap)
            {
                AppendTypeCustomMapMethod(ref builder, typeCustomMap);
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

    //public string GenerateCustom(MapGroup mapGroup)
    //{
    //    var builder = new ValueStringBuilder(stackalloc char[1024]);

    //    foreach (var @using in mapGroup.Usings)
    //    {
    //        builder.Append(@using);
    //    };
    //    builder.Append(NewLine);
    //    builder.Append(NewLine);
    //    builder.Append(Namespace);
    //    builder.Append(NewLine);
    //    builder.Append(NewLine);
    //    builder.Append(Class);
    //    builder.Append(NewLine);
    //    builder.Append(OpenCurlyBracket);
    //    builder.Append(NewLine);
    //    foreach (var map in mapGroup.Maps)
    //    {
    //        if (map is TypeCustomMap typeCustomMap)
    //        {
    //            AppendTypeCustomMapMethod(ref builder, typeCustomMap);
    //        }
    //        else if (map is ClassPartialMap partialMap)
    //        {
    //            AppendClassPartialMapMethod(ref builder, partialMap);
    //        }
    //        else if (map is ClassPartialConstructorMap partialConstructorMap)
    //        {
    //            AppendClassPartialConstructorMapMethod(ref builder, partialConstructorMap);
    //        }
    //        builder.Append(NewLine);
    //        builder.Append(NewLine);
    //    }
    //    builder.Append(NewLine);
    //    builder.Append(CloseCurlyBracket);

    //    return builder.ToString();
    //}   

    private void AppendClassPartialMapMethod(ref ValueStringBuilder builder, ClassPartialMap map)
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
        builder.Append(SpecialPrefix);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(to);
        builder.Append(WhiteSpace);
        builder.Append(SpecialPrefix);
        builder.Append(UserFunction);
        builder.Append(OpenBracket);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append(map.ParameterName);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        foreach (var statement in map.CustomStatements)
        {
            AppendTabs(ref builder, 4);
            builder.Append(statement.ToString());
            builder.Append(NewLine);
        }
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Var);
        builder.Append(WhiteSpace);
        builder.Append(SpecialPrefix);
        builder.Append(Result);
        builder.Append(WhiteSpace);
        builder.Append(Equal);
        builder.Append(WhiteSpace);
        builder.Append(SpecialPrefix);
        builder.Append(UserFunction);
        builder.Append(OpenBracket);
        builder.Append(SpecialPrefix);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(Semicolon);
        builder.Append(NewLine);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Return);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToDisplayString());
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenBracket);
        var counter = 1;
        foreach (var property in map.ConstructorProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append(SpecialPrefix);
            if (property.IsProvidedByUser)
            {
                builder.Append(Result);
            }
            else
            {
                builder.Append(Source);
            }
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion))
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
        AppendTabs(ref builder, 3);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append(property.ToName);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            builder.Append(SpecialPrefix);
            if (property.IsProvidedByUser)
            {
                builder.Append(Result);
            }
            else
            {
                builder.Append(Source);
            }
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion))
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
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendClassPartialConstructorMapMethod(ref ValueStringBuilder builder, ClassPartialConstructorMap map)
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
        builder.Append(map.ParameterName);
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
            if (property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion)
            {
                if (property.ArgumentSyntax is not null)
                {
                    builder.Append(property.ArgumentSyntax?.ToString());
                }
                else
                {
                    builder.Append(map.ParameterName);
                    builder.Append(Dot);
                    builder.Append(property.FromName);
                }
            }
            else
            {
                builder.Append(map.ParameterName);
                builder.Append(Dot);
                builder.Append(property.FromName);
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
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            if (property.IsSameTypes || property.IsProvidedByUser || property.HasImplicitConversion)
            {
                if (property.InitializerExpressionSyntax is not null)
                {
                    builder.Append(property.InitializerExpressionSyntax.ToString());
                }
                else
                {
                    builder.Append(property.ToName);
                    builder.Append(WhiteSpace);
                    builder.Append(Equal);
                    builder.Append(WhiteSpace);
                    builder.Append(map.ParameterName);
                    builder.Append(Dot);
                    builder.Append(property.FromName);
                }
            }
            else
            {
                builder.Append(property.ToName);
                builder.Append(WhiteSpace);
                builder.Append(Equal);
                builder.Append(WhiteSpace);
                builder.Append(map.ParameterName);
                builder.Append(Dot);
                builder.Append(property.FromName);
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
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    //public static {map.To} Map<To>(this {map.From} {map.ParameterName}) => new {map.To}
    //(
    //    {one.ArgumentSyntax?.ToString() ?? {map.ParameterName}.{one.FromName},
    //    UnflatteningMap_{two.ToType.ToString().RemoveDots()}({map.ParameterName}),
    //    {map.ParameterName}.{@default.FromName}.Map<{@default.ToType}>(),
    //)
    //{
    //    {one.InitializerExpressionSyntax?.ToString() ?? {one.ToName} = {map.ParameterName}.{one.FromName},
    //    {two.ToName} = UnflatteningMap_{two.ToType.ToString().RemoveDots()}({map.ParameterName}),
    //    {@default.ToName} = {map.ParameterName}.{@default.FromName}.Map<{@default.ToType}>(),
    //};

    private void AppendTypeCustomMapMethod(ref ValueStringBuilder builder, TypeCustomMap map)
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
        builder.Append(map.ParameterName);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        if (map.MethodType == MethodBodyType.Expression)
        {
            builder.Append(map.ExpressionBody!.ToString());
            builder.Append(Semicolon);
        }
        else
        {
            builder.Append(map.Body!.ToString());
        }
    }

    //public static {map.To} Map<To>(this {map.From} {map.ParameterName})
    //  {map.Body?.ToString()}; or {map.ExpressionBody};

    //public string GenerateCommon(MapGroup commonMapGroup)
    //{
    //    var builder = new ValueStringBuilder(stackalloc char[1024]);

    //    foreach (var @using in commonMapGroup.Usings)
    //    {
    //        builder.Append(@using);
    //    };

    //    builder.Append(NewLine);
    //    builder.Append(NewLine);
    //    builder.Append(Namespace);
    //    builder.Append(NewLine);
    //    builder.Append(NewLine);
    //    builder.Append(Class);
    //    builder.Append(NewLine);
    //    builder.Append(OpenCurlyBracket);
    //    builder.Append(NewLine);
    //    foreach (var map in commonMapGroup.Maps)
    //    {
    //        if (map is ClassMap classMap)
    //        {
    //            if (!classMap.IsUnflattening)
    //            {
    //                AppendCommonMapMethod(ref builder, classMap);
    //            }
    //            else
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }
    //        else if (map is EnumMap enumMap)
    //        {
    //            AppendEnumMapMethod(ref builder, enumMap);
    //        }
    //        else if (map is CollectionMap collectionMap)
    //        {
    //            AppendCollectionMapMethod(ref builder, collectionMap);
    //        }
    //        builder.Append(NewLine);
    //        builder.Append(NewLine);
    //    }
    //    builder.Append(NewLine);
    //    builder.Append(CloseCurlyBracket);

    //    return builder.ToString();
    //}

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


    //public static {map.To} Map<To>(this {map.From} source)
    //{
    //  var destination = {map.To}[source.Count()]
    //  for(uint i = 0; i < destination.Length; i++)
    //  {
    //      destination[i] = source
    //  }
    //}
    //  => source.Select(x => x.Map<{map.ItemTo}>()){(map.CollectionType == CollectionType.List ? ".ToList()" : ".ToArray()")};

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

    //public static {map.To} Map<To>(this {map.From} source) => source switch
    //{
    //    {x.FromType}.{x.FromName} => {x.ToType}.{x.ToName},")
    //    _ => throw new System.ArgumentOutOfRangeException("Error when mapping { map.From} to { map.To}")
    //};

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

    //public static {map.To} Map<To>(this {map.From} source) => new object
    //(
    //    source.{property.FromName},
    //    UnflatteningMap_{property.ToType.ToString().RemoveDots()}(source),
    //    source.{property.FromName}.Map<{property.ToType}>(),
    //)
    //{
    //    {property.ToName} = source.{property.FromName},
    //    {property.ToName} = UnflatteningMap_{property.ToType.ToString().RemoveDots()}(source),
    //    {property.ToName} = {property.ToName} = source.{property.FromName}.Map<{property.ToType}>(),
    //};
}