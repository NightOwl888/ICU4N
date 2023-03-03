// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ICU4N.Text;
using System;
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

        private void VerifyWritable()
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(SR.InvalidOperation_ReadOnly);
            }
        }

        // ICU4N TODO: CurrentInfo

        public static UNumberFormatInfo InvariantInfo => s_invariantInfo ??=
            // Lazy create the invariant info. This cannot be done in a .cctor because exceptions can
            // be thrown out of a .cctor stack that will need this.
            new UNumberFormatInfo { isReadOnly = true };


        // ICU4N TODO: GetInstance(IFormatProvider? formatProvider)
        public static UNumberFormatInfo GetInstance(UCultureInfo culture)
        {
            throw new NotImplementedException(); // ICU4N TODO: Complete implementation
        }

        public static UNumberFormatInfo GetInstance(CultureInfo culture)
        {
            throw new NotImplementedException(); // ICU4N TODO: Complete implementation
        }

        public object Clone()
        {
            UNumberFormatInfo n = (UNumberFormatInfo)MemberwiseClone();
            n.isReadOnly = false;
            return n;
        }

        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(NumberFormatInfo) ? this : null;
        }

        private static class SR
        {
            
            public const string Argument_EmptyDecString = "Decimal separator cannot be the empty string.";
            public const string Argument_InvalidNativeDigitCount = "The NativeDigits array must contain exactly ten members.";
            public const string Argument_InvalidNativeDigitValue = "Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit.";
            public const string Argument_UnknownCurrencySpacing = "Unknown currency spacing: {0}.";
            public const string ArgumentNull_Array = "Array cannot be null.";
            public const string ArgumentNull_ArrayValue = "Found a null value within an array.";

            public const string InvalidOperation_ReadOnly = "Instance is read-only.";

            public const string NotSupported_UseNativeDigitsInstead = "Use UCultureInfo.NativeDigits instead.";
        }
    }
}
