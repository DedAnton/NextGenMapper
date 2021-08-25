using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Validators;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class MemberMap
    {
        public Type FromType { get; }
        public string FromName { get; }
        public Type ToType { get; }
        public string ToName { get; }
        public MemberMapType MapType { get; }
        public bool IsProvidedByUser { get; }
        public string? FlattenPropertyName { get; }
        public ArgumentSyntax? ArgumentSyntax { get; private set; }
        public InitializerExpressionSyntax? InitializerExpressionSyntax { get; private set; }

        public bool IsSameTypes => FromType.Name == ToType.Name;
        public bool HasImplicitConversion => ImplicitNumericConversionValidator.HasImplicitConversion(FromType, ToType);

        public static MemberMap Counstructor(Property from, Parameter to, string? flattenPropertyName = null) 
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Constructor, false, flattenPropertyName);

        public static MemberMap Initializer(Property from, Property to, string? flattenPropertyName = null)
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Initializer, false, flattenPropertyName);

        public static MemberMap CounstructorUnflatten(Type from, Parameter to)
            => new(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenConstructor, false, null);

        public static MemberMap InitializerUnflatten(Type from, Property to)
            => new(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenInitializer, false, null);

        public static MemberMap Field(Enum fromType, EnumField fromField, Enum toType, EnumField toField)
            => new(fromType, fromField.Name, toType, toField.Name, MemberMapType.Field, false, null);

        public static MemberMap User(Property from, Parameter to)
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Constructor, true, null);

        public static MemberMap User(Property to)
            => new(to.Type, to.Name, to.Type, to.Name, MemberMapType.Initializer, true, null);

        public static MemberMap Argument(Parameter parameter, ArgumentSyntax argument)
            => new(parameter.Type, parameter.Name, parameter.Type, parameter.Name, MemberMapType.Constructor, true, null) { ArgumentSyntax = argument };

        public static MemberMap InitializerExpression(Property property, InitializerExpressionSyntax initializerExpression)
            => new(property.Type, property.Name, property.Type, property.Name, MemberMapType.Initializer, true, null) { InitializerExpressionSyntax = initializerExpression };

        private MemberMap(
            Type fromType,
            string fromName,
            Type toType,
            string toName,
            MemberMapType mapType,
            bool isProvidedByUser,
            string? flattenPropertyName)
        {
            FromType = fromType;
            ToType = toType;
            FromName = flattenPropertyName != null ? $"{flattenPropertyName}.{fromName}" : fromName;
            ToName = toName;
            MapType = mapType;
            IsProvidedByUser = isProvidedByUser;
            FlattenPropertyName = flattenPropertyName;
        }
    }

    public enum MemberMapType
    {
        Constructor,
        Initializer,
        Field,
        UnflattenConstructor,
        UnflattenInitializer
    }
}
