// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ICU4N: Corresponds primarily with the NumberFormatInfo class in .NET

using ICU4N.Text;
using System;
using System.Diagnostics;
using System.Globalization;
#nullable enable

namespace ICU4N.Globalization
{
    /// <icuenhanced cref="NumberFormatInfo"/>.<icu>_usage_</icu>
    /// <summary>
    /// Provides culture-specific information for formatting and parsing numeric values.
    /// </summary>
    /// <remarks>
    /// The NumberFormatInfo class contains culture-specific information that is used when you
    /// format and parse numeric values. This information includes the currency symbol, the
    /// decimal symbol, the group separator symbol, and the symbols for positive and negative signs.
    /// </remarks>
    /// <draft>ICU 60.1</draft>
    public sealed partial class UNumberFormatInfo : IFormatProvider, IDecimalFormatSymbols // ICU4N NOTE: DecimalFormatSymbols was not sealed in ICU4J, but .NET seals NumberFormatInfo.
    {
        private static volatile UNumberFormatInfo? s_invariantInfo;


        internal bool isReadOnly;

        /// <summary>
        /// Initializes a new writable instance of the <see cref="UNumberFormatInfo"/> class that is
        /// culture-independent (invariant).
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public UNumberFormatInfo()
        {
            this.cultureData = UCultureData.Invariant;
        }

        internal UNumberFormatInfo(UCultureData cultureData)
        {
            Debug.Assert(cultureData != null);
            cultureData.GetNFIValues(this);
            this.cultureData = cultureData;
        }

        /// <summary>
        /// Gets a value that indicates whether this <see cref="UNumberFormatInfo"/> object is read-only.
        /// </summary>
        /// <value><c>true</c> if the <see cref="UNumberFormatInfo"/> is read-only; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Attempting to perform an assignment to a property of a read-only <see cref="UNumberFormatInfo"/> causes
        /// an <see cref="InvalidOperationException"/>.
        /// <para/>
        /// You can call the <see cref="Clone()"/> method to create a read/write <see cref="UNumberFormatInfo"/> object
        /// from a read-only object.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public bool IsReadOnly => isReadOnly;

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

        /// <summary>
        /// Gets a read-only <see cref="UNumberFormatInfo"/> that formats values based on
        /// the current culture.
        /// </summary>
        /// <value>A read-only <see cref="UNumberFormatInfo"/> based on the culture of the
        /// current thread.</value>
        /// <remarks>
        /// Retrieving a <see cref="UNumberFormatInfo"/> object from the <see cref="CurrentInfo"/>
        /// property is equivalent to retrieving a <see cref="UNumberFormatInfo"/> object from the
        /// <c>UCultureInfo.CurrentCulture.NumberFormat</c> property.
        /// <para/>
        /// This culture corresponds with the "default locale" in ICU jargon.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
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

        /// <summary>
        /// Gets a read-only <see cref="UNumberFormatInfo"/> object that is culture-independent (invariant).
        /// </summary>
        /// <value>A read-only object that is culture-independent (invariant).</value>
        /// <remarks>
        /// This <see cref="UNumberFormatInfo"/> object returned by this property does not change, regardless
        /// of the current culture. It represents the formatting conventions of the invariant culture, which
        /// is a culture associated with the English language but not with any country/region. The invariant
        /// culture is used in formatting operations that are culture-independent or that produce result strings
        /// suitable for display across multiple cultures.
        /// <para/>
        /// This culture corresponds with the "root locale" in ICU jargon.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public static UNumberFormatInfo InvariantInfo => s_invariantInfo ??=
            // Lazy create the invariant info. This cannot be done in a .cctor because exceptions can
            // be thrown out of a .cctor stack that will need this.
            new UNumberFormatInfo { isReadOnly = true };


        /// <summary>
        /// Gets the <see cref="UNumberFormatInfo"/> associated with the specified <see cref="IFormatProvider"/>.
        /// </summary>
        /// <param name="formatProvider">
        /// The <see cref="IFormatProvider"/> used to get the <see cref="UNumberFormatInfo"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// <c>null</c> to get <see cref="CurrentInfo"/>.
        /// </param>
        /// <returns>The <see cref="UNumberFormatInfo"/> associated with the specified <see cref="IFormatProvider"/>.</returns>
        /// <exception cref="NotSupportedException"><paramref name="formatProvider"/> is of type
        /// <see cref="NumberFormatInfo"/>.</exception>
        /// <remarks>
        /// This method uses the <see cref="IFormatProvider.GetFormat(Type?)"/> method of <paramref name="formatProvider"/>
        /// using <see cref="UNumberFormatInfo"/> as the Type parameter. If <paramref name="formatProvider"/> is <c>null</c>
        /// or if <see cref="IFormatProvider.GetFormat(Type?)"/> returns <c>null</c>, this method returns <see cref="CurrentInfo"/>.
        /// <para/>
        /// Your application gets a <see cref="UNumberFormatInfo"/> object for a specific culture using one of the following
        /// methods:
        /// <list type="bullet">
        ///     <item><description>Through the <see cref="UCultureInfo.NumberFormat"/> property.</description></item>
        ///     <item><description>Through the <see cref="UCultureInfo.NumberFormat"/> property of the <see cref="UCultureInfo"/> instance
        ///         returned from the <see cref="CultureInfoExtensions.ToUCultureInfo(CultureInfo)"/> extension method.</description></item>
        ///     <item><description>Through the <see cref="GetInstance(IFormatProvider?)"/> method where provider is a <see cref="CultureInfo"/>.</description></item>
        /// </list>
        /// A <see cref="UNumberFormatInfo"/> object is created only for the invariant culture or for specific cultures,
        /// not for neutral cultures. For more information about the invariant culture, specific cultures, and neutral
        /// cultures, see the <see cref="CultureInfo"/> class.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
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

        // ICU4N TODO: Add overload of GetInstance() that allows passing in a "base culture" as either CultureInfo or UCultureInfo
        // in addition to a System.Globalization.NumberFormatInfo instance so we can set up the culture with user-chosen values
        // for BreakIterator and other internally localized stuff passing in only .NET platform types.

        /// <summary>
        /// Creates a shallow copy of the <see cref="UNumberFormatInfo"/> object.
        /// </summary>
        /// <returns>A new writable object copied from the original
        /// <see cref="UNumberFormatInfo"/> object.</returns>
        /// <remarks>
        /// The clone is writable even if the original <see cref="UNumberFormatInfo"/> object is read-only.
        /// Therefore, the properties of the clone can be modified with user-defined patterns.
        /// <para/>
        /// A shallow copy of an object is a copy of the object only. If the object contains references to
        /// other objects, the shallow copy will not create copies of the referred objects. It will refer
        /// to the original objects instead. On the other hand, a deep copy of an object creates a copy of
        /// the object and a copy of everything directly or indirectly referenced by that object. In the case
        /// of a <see cref="UNumberFormatInfo"/> object, a shallow copy is sufficient for copying all instance
        /// properties, because all properties that return object references are static (Shared in Visual Basic).
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public object Clone() // ICU4N TODO: API Need to make a final decision on IFreezable from ICU4J, this .NET approach, or both.
        {
            UNumberFormatInfo n = (UNumberFormatInfo)MemberwiseClone();
            n.isReadOnly = false;
            return n;
        }

        /// <summary>
        /// Gets an object of the specified type that provides a number formatting service.
        /// </summary>
        /// <param name="formatType">The <see cref="Type"/> of the required formatting service.</param>
        /// <returns>The current <see cref="UNumberFormatInfo"/>, if <paramref name="formatType"/>
        /// is the same as the type of the current <see cref="UNumberFormatInfo"/>; otherwise, <c>null</c>.</returns>
        /// <remarks>
        /// The <c>ToString</c> and <c>TryFormat</c> methods supported by the base data types invoke this method
        /// when the current <c>UNumberFormatInfo</c> is passed as the <c>IFormatProvider</c> parameter. This method
        /// implements <see cref="IFormatProvider.GetFormat(Type?)"/>.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(UNumberFormatInfo) ? this : null;
        }

        /// <summary>
        /// Returns a read-only <see cref="UNumberFormatInfo"/> wrapper.
        /// </summary>
        /// <param name="nfi">The <see cref="UNumberFormatInfo"/> to wrap.</param>
        /// <returns>A read-only <see cref="UNumberFormatInfo"/> wrapper around <paramref name="nfi"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nfi"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This wrapper prevents any modifications to <paramref name="nfi"/>.
        /// <para/>
        /// Attempting to perform an assignment to a property of a read-only
        /// <see cref="UNumberFormatInfo"/> causes an <see cref="InvalidOperationException"/>.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public static UNumberFormatInfo ReadOnly(UNumberFormatInfo nfi) // ICU4N TODO: API Need to make a final decision on IFreezable from ICU4J, this .NET approach, or both.
        {
            if (nfi == null)
            {
                throw new ArgumentNullException(nameof(nfi));
            }

            if (nfi.IsReadOnly)
            {
                return nfi;
            }

            UNumberFormatInfo info = (UNumberFormatInfo)nfi.MemberwiseClone();
            info.isReadOnly = true;
            return info;
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
