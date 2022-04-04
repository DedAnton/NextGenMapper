using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Validators
{
    public static class ImplicitNumericConversionValidator
    {
        private static readonly Dictionary<SpecialType, HashSet<SpecialType>> _implicitNumericConversions =
            new()
            {
                { SpecialType.System_SByte, new() { SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Byte, new() { SpecialType.System_Int16, SpecialType.System_UInt16, SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Int16, new() { SpecialType.System_Int32, SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_UInt16, new() { SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Int32, new() { SpecialType.System_Int64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_UInt32, new() { SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Int64, new() { SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_UInt64, new() { SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Char, new() { SpecialType.System_UInt16, SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64, SpecialType.System_Single, SpecialType.System_Double, SpecialType.System_Decimal } },
                { SpecialType.System_Single, new() { SpecialType.System_Double } }
            };

        public static bool HasImplicitConversion(ITypeSymbol from, ITypeSymbol to)
            => _implicitNumericConversions.TryGetValue(from.SpecialType, out var implicitTypes) && implicitTypes.Contains(to.SpecialType);
    }
}
