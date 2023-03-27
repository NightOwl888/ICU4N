// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ICU4N.Text;
using System;
using System.Diagnostics;
using System.Globalization;
#nullable enable

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo : IFormatProvider, IDecimalFormatSymbols // ICU4N NOTE: DecimalFormatSymbols was not sealed in ICU4J, but .NET seals NumberFormatInfo.
    {
        private static volatile UNumberFormatInfo? s_invariantInfo;


        internal bool isReadOnly;

        public UNumberFormatInfo()
        {
            this.cultureData = UCultureData.Invariant;
        }

        internal UNumberFormatInfo(UCultureData cultureData)
        {
            Debug.Assert(cultureData is not null);
            cultureData.GetNFIValues(this);
            this.cultureData = cultureData;
        }

        private static void VerifyDecimalSeparator(string decSep, string propertyName)
        {
            if (decSep == null)
            {
                throw new ArgumentNullException(propertyName);
            }

            if (decSep.Length == 0)
            {
                throw new ArgumentException(SR.Argument_EmptyDecString, propertyName);
            }
        }

        private static void VerifyGroupSeparator(string groupSep, string propertyName)
        {
            if (groupSep == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        private static void VerifyNativeDigits(string[] nativeDig, string propertyName)
        {
            if (nativeDig == null)
            {
                throw new ArgumentNullException(propertyName, SR.ArgumentNull_Array);
            }

            if (nativeDig.Length != 10)
            {
                throw new ArgumentException(SR.Argument_InvalidNativeDigitCount, propertyName);
            }

            for (int i = 0; i < nativeDig.Length; i++)
            {
                if (nativeDig[i] == null)
                {
                    throw new ArgumentNullException(propertyName, SR.ArgumentNull_ArrayValue);
                }

                if (nativeDig[i].Length != 1)
                {
                    if (nativeDig[i].Length != 2)
                    {
                        // Not 1 or 2 UTF-16 code points
                        throw new ArgumentException(SR.Argument_InvalidNativeDigitValue, propertyName);
                    }
                    else if (!char.IsSurrogatePair(nativeDig[i][0], nativeDig[i][1]))
                    {
                        // 2 UTF-6 code points, but not a surrogate pair
                        throw new ArgumentException(SR.Argument_InvalidNativeDigitValue, propertyName);
                    }
                }

                if (CharUnicodeInfo.GetDecimalDigitValue(nativeDig[i], 0) != i &&
                    CharUnicodeInfo.GetUnicodeCategory(nativeDig[i], 0) != UnicodeCategory.PrivateUse)
                {
                    // Not the appropriate digit according to the Unicode data properties
                    // (Digit 0 must be a 0, etc.).
                    throw new ArgumentException(SR.Argument_InvalidNativeDigitValue, propertyName);
                }
            }
        }

        private static void VerifyDigitSubstitution(UDigitShapes digitSub, string propertyName)
        {
            switch (digitSub)
            {
                //case UDigitShapes.Context:
                case UDigitShapes.None:
                case UDigitShapes.NativeNational:
                    // Success.
                    break;

                default:
                    throw new ArgumentException(SR.Argument_InvalidDigitSubstitution, propertyName);
            }
        }


        private void VerifyWritable()
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(SR.InvalidOperation_ReadOnly);
            }
        }

        public static UNumberFormatInfo CurrentInfo
        {
            get
            {
                UCultureInfo culture = UCultureInfo.CurrentCulture;

                UNumberFormatInfo? info = culture.numInfo;
                if (info != null)
                {
                    return info;
                }
                // returns non-nullable when passed typeof(NumberFormatInfo)
                return (UNumberFormatInfo)culture.GetFormat(typeof(UNumberFormatInfo))!;
            }
        }

        public static UNumberFormatInfo InvariantInfo => s_invariantInfo ??=
            // Lazy create the invariant info. This cannot be done in a .cctor because exceptions can
            // be thrown out of a .cctor stack that will need this.
            new UNumberFormatInfo { isReadOnly = true };


        public static UNumberFormatInfo GetInstance(IFormatProvider? formatProvider)
        {
            return formatProvider == null ?
                CurrentInfo : // Fast path for a null provider
                GetProviderNonNull(formatProvider);

            static UNumberFormatInfo GetProviderNonNull(IFormatProvider provider)
            {
                // Fast path for a regular CultureInfo
                if (provider is UCultureInfo cultureProvider)
                {
                    return cultureProvider.numInfo ?? cultureProvider.NumberFormat;
                }

                if (provider is CultureInfo dotnetCultureProvider)
                {
                    return dotnetCultureProvider.ToUCultureInfo().NumberFormat;
                }

                if (provider is NumberFormatInfo)
                    throw new NotSupportedException(SR.NotSupported_NumberFormatInfo); // ICU4N TODO: Add a best effort to convert this

                return
                    provider as UNumberFormatInfo ?? // Fast path for an NFI
                    provider.GetFormat(typeof(UNumberFormatInfo)) as UNumberFormatInfo ??
                    CurrentInfo;
            }
        }

        public object Clone()
        {
            UNumberFormatInfo n = (UNumberFormatInfo)MemberwiseClone();
            n.isReadOnly = false;
            return n;
        }

        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(UNumberFormatInfo) ? this : null;
        }

        private static class SR
        {
            public const string Argument_EmptyDecString = "Decimal separator cannot be the empty string.";
            public const string Argument_InvalidDigitSubstitution = "The DigitSubstitution property must be of a valid member of the UDigitShapes enumeration. Valid entries include NativeNational or None.";
            public const string Argument_InvalidNativeDigitCount = "The NativeDigits array must contain exactly ten members.";
            public const string Argument_InvalidNativeDigitValue = "Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit.";
            public const string Argument_InvalidGroupSize = "Every element in the value array should be between one and nine, except for the last element, which can be zero.";
            public const string Argument_UnknownCurrencySpacing = "Unknown currency spacing: {0}.";

            public const string ArgumentOutOfRange_Enum = "The enumeration value '{0}' was out of range of the '{1}' enum.";
            public const string ArgumentOutOfRange_MaxDigits = "{0} must be greater than or equal to {1}.";
            public const string ArgumentOutOfRange_MinDigits = "{0} must be less than or equal to {1}.";
            public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
            public const string ArgumentOutOfRange_Range = "Valid values are between {0} and {1}, inclusive.";

            public const string ArgumentNull_Array = "Array cannot be null.";
            public const string ArgumentNull_ArrayValue = "Found a null value within an array.";

            public const string InvalidOperation_ReadOnly = "Instance is read-only.";

            public const string NotSupported_UseNativeDigitsInstead = "Use UCultureInfo.NativeDigits instead.";
            public const string NotSupported_NumberFormatInfo = "NumberFormatInfo is not a currently supported provider. Populate a UNumberFormatInfo with the desired values instead.";
        }
    }
}
