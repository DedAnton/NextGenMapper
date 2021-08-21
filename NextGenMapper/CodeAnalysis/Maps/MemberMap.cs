using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.CodeAnalysis.Validators;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class FlattenedMap : MemberMap
    {
        public string FlattenPropertyName { get; }

        public FlattenedMap(IMember from, IMember to, MemberMapType mapType, string flattenedPropertyName)
            : base(from.Type, $"{flattenedPropertyName}.{from.Name}", to.Type, to.Name, mapType)
        {
            FlattenPropertyName = flattenedPropertyName;
        }
    }

    public class UnflattenedMap : MemberMap
    {
        public UnflattenedMap(Type from, IMember to, MemberMapType mapType)
            : base(from, from.Name, to.Type, to.Name, mapType)
        {

        }
    }

    public class CustomMap : MemberMap
    {
        private CustomMap(IMember from, IMember to, MemberMapType mapType)
            : base(from.Type, from.Name, to.Type, to.Name, mapType)
        {

        }

        public static CustomMap Constructor(Property fromProperty, Parameter constructorParameter) => new CustomMap(fromProperty, constructorParameter, MemberMapType.Constructor);
        public static CustomMap Initializer(Property initializerProperty) => new CustomMap(initializerProperty, initializerProperty, MemberMapType.Initializer);
    }

    public class CustomArgumentMap : MemberMap
    {
        public ArgumentSyntax ArgumentSyntax { get; }

        public CustomArgumentMap(Parameter parameter, ArgumentSyntax argument)
            : base(parameter.Type, parameter.Name, parameter.Type, parameter.Name, MemberMapType.Constructor)
        {
            ArgumentSyntax = argument;
        }
    }

    public class CustomInitializerExpressionMap : MemberMap
    {
        public AssignmentExpressionSyntax InitializerExpression { get; }

        public CustomInitializerExpressionMap(Property property, AssignmentExpressionSyntax initializerExpression)
            : base(property.Type, property.Name, property.Type, property.Name, MemberMapType.Initializer)
        {
            InitializerExpression = initializerExpression;
        }
    }


    public class EnumFieldMap : MemberMap
    {
        public EnumFieldMap(EnumField from, EnumField to)
            :base(from.Type, from.Name, to.Type, from.Name, MemberMapType.Special)
        { }
    }

    public class MemberMap
    {
        public MemberMap(IMember from, IMember to, MemberMapType mapType)
        {
            FromType = from.Type;
            FromName = from.Name;
            ToType = to.Type;
            ToName = to.Name;
            Type = mapType;
        }

        public MemberMap(Type fromType, string fromName, Type toType, string toName, MemberMapType mapType)
        {
            FromType = fromType;
            FromName = fromName;
            ToType = toType;
            ToName = toName;
            Type = mapType;
        }

        public Type FromType { get; }
        public string FromName { get; }
        public Type ToType { get; }
        public string ToName { get; }
        public MemberMapType Type { get; }

        public bool IsSameTypes => FromType == ToType;
        public bool HasImplicitConversion => ImplicitNumericConversionValidator.HasImplicitConversion(FromType, ToType);
    }

    public enum MemberMapType
    {
        Constructor,
        Initializer,
        Special
    }
}
