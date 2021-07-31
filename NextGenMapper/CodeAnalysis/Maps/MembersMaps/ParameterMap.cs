using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Validators;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public class ParameterMap : IMemberMap
    {
        public IPropertySymbol From { get; }
        public IParameterSymbol To { get; }

        public ParameterMap(IPropertySymbol from, IParameterSymbol to, bool isProvidedByUser = false)
        {
            From = from;
            To = to;
            IsProvidedByUser = isProvidedByUser;
        }

        public ITypeSymbol TypeFrom => From.Type;
        public ITypeSymbol TypeTo => To.Type;
        public string NameFrom => From.Name;
        public string NameTo => To.Name;
        public bool IsSameTypes => From.Type.Equals(To.Type, SymbolEqualityComparer.IncludeNullability);
        public bool HasImplicitConversion => ImplicitNumericConversionValidator.HasImplicitConversion(From.Type, To.Type);
        public bool IsProvidedByUser { get; }
    }
}
