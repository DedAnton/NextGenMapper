using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Validators;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public sealed class MemberMap
    {
        public ITypeSymbol FromType { get; }
        public string FromName { get; }
        public ITypeSymbol ToType { get; }
        public string ToName { get; }
        public MemberMapType MapType { get; }
        public bool IsProvidedByUser { get; }
        public string? FlattenPropertyName { get; }

        public bool IsSameTypes => FromType.Equals(ToType, SymbolEqualityComparer.IncludeNullability);
        public bool HasImplicitConversion => ImplicitNumericConversionValidator.HasImplicitConversion(FromType, ToType);

        public static MemberMap Counstructor(IPropertySymbol from, IParameterSymbol to, string? flattenPropertyName = null) 
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Constructor, false, flattenPropertyName);

        public static MemberMap Initializer(IPropertySymbol from, IPropertySymbol to, string? flattenPropertyName = null)
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Initializer, false, flattenPropertyName);

        public static MemberMap CounstructorUnflatten(ITypeSymbol from, IParameterSymbol to)
            => new(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenConstructor, false, null);

        public static MemberMap InitializerUnflatten(ITypeSymbol from, IPropertySymbol to)
            => new(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenInitializer, false, null);

        public static MemberMap Field(IPropertySymbol from, IParameterSymbol to)
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Field, false, null);

        public static MemberMap User(IPropertySymbol from, IParameterSymbol to)
            => new(from.Type, from.Name, to.Type, to.Name, MemberMapType.Constructor, true, null);

        public static MemberMap User(IPropertySymbol to)
            => new(to.Type, to.Name, to.Type, to.Name, MemberMapType.Initializer, true, null);

        public MemberMap(IFieldSymbol from, IFieldSymbol to)
            : this(from.Type, from.Name, to.Type, to.Name, MemberMapType.Field, false, null)
        { }

        private MemberMap(
            ITypeSymbol fromType,
            string fromName,
            ITypeSymbol toType,
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
