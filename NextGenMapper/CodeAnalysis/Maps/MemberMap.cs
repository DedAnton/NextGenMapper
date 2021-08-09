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

        public MemberMap(IPropertySymbol from, IPropertySymbol to, bool isProvidedByUser = false, string? flattenPropertyName = null)
            :this(from.Type, from.Name, to.Type, to.Name, MemberMapType.Initializer, isProvidedByUser, flattenPropertyName) 
        { }

        public MemberMap(IPropertySymbol from, IParameterSymbol to, bool isProvidedByUser = false, string? flattenPropertyName = null)
            : this(from.Type, from.Name, to.Type, to.Name, MemberMapType.Constructor, isProvidedByUser, flattenPropertyName)
        { }

        public MemberMap(ITypeSymbol from, IPropertySymbol to, bool isProvidedByUser = false)
            : this(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenInitializer, isProvidedByUser, null)
        { }

        public MemberMap(ITypeSymbol from, IParameterSymbol to, bool isProvidedByUser = false)
            : this(from, from.Name, to.Type, to.Name, MemberMapType.UnflattenConstructor, isProvidedByUser, null)
        { }

        public MemberMap(IFieldSymbol from, IFieldSymbol to, bool isProvidedByUser = false)
            : this(from.Type, from.Name, to.Type, to.Name, MemberMapType.Field, isProvidedByUser, null)
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
