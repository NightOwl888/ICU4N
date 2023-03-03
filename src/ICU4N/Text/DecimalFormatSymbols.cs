using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
#nullable enable

namespace ICU4N.Text
{
    /// <icuenhanced cref="NumberFormatInfo"/>.<icu>_usage_</icu>
    /// <summary>
    /// This class represents the set of symbols (such as the decimal separator, the grouping
    /// separator, and so on) needed by <see cref="DecimalFormat"/> to format
    /// numbers. <see cref="DecimalFormat"/> creates for itself an instance of
    /// <see cref="DecimalFormatSymbols"/> from its locale data.  If you need to change any of
    /// these symbols, you can get the <see cref="DecimalFormatSymbols"/> object from your
    /// <see cref="DecimalFormat"/> and modify it.
    /// </summary>
    /// <seealso cref="CultureInfo"/>
    /// <seealso cref="DecimalFormat"/>
    /// <author>Mark Davis</author>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        class DecimalFormatSymbols // ICU4N TODO: serialization, Refactor into UNumberFormatInfo..?
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /// <summary>
        /// Creates a <see cref="DecimalFormatSymbols"/> object for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public DecimalFormatSymbols()
            : this(CultureInfo.CurrentCulture) // ICU4N TODO: in .NET, the default is invariant culture
        {
        }

        /// <summary>
        /// Creates a <see cref="DecimalFormatSymbols"/> object for the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 2.0</stable>
        public DecimalFormatSymbols(CultureInfo locale)
            : this(locale.ToUCultureInfo())
        {
        }

        /// <summary>
        /// Creates a <see cref="DecimalFormatSymbols"/> object for the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.2</stable>
        public DecimalFormatSymbols(UCultureInfo locale)
            : this(locale, null) // ICU4N: Reuse private constuctor
        {
        }

        private DecimalFormatSymbols(CultureInfo locale, NumberingSystem? ns)
            : this(locale.ToUCultureInfo(), ns)
        {
        }

        private DecimalFormatSymbols(UCultureInfo locale, NumberingSystem? ns)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale)); // ICU4N: Added guard clause
            Initialize(locale, ns);
        }

        /// <summary>
        /// Returns a <see cref="DecimalFormatSymbols"/> instance for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>A <see cref="DecimalFormatSymbols"/> instance.</returns>
        /// <stable>ICU 3.8</stable>
        public static DecimalFormatSymbols GetInstance()
        {
            return new DecimalFormatSymbols();
        }

        /// <summary>
        /// Returns a <see cref="DecimalFormatSymbols"/> instance for the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns>A <see cref="DecimalFormatSymbols"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.8</stable>
        public static DecimalFormatSymbols GetInstance(CultureInfo locale)
        {
            return new DecimalFormatSymbols(locale);
        }

        /// <summary>
        /// Returns a <see cref="DecimalFormatSymbols"/> instance for the given <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns>A <see cref="DecimalFormatSymbols"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.8</stable>
        public static DecimalFormatSymbols GetInstance(UCultureInfo locale)
        {
            return new DecimalFormatSymbols(locale);
        }

        /// <summary>
        /// <icu/> Returns a <see cref="DecimalFormatSymbols"/> instance for the given locale with digits and symbols
        /// corresponding to the given <see cref="NumberingSystem"/>.
        /// <para/>
        /// This method behaves equivalently to <see cref="GetInstance(UCultureInfo)"/> called with a locale having a
        /// "numbers=xxxx" keyword.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="ns">The numbering system.</param>
        /// <returns>A <see cref="DecimalFormatSymbols"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <draft>ICU 60</draft>
        public static DecimalFormatSymbols ForNumberingSystem(CultureInfo locale, NumberingSystem ns) // ICU4N TODO: API - rename according to .NET naming conventions
        {
            return new DecimalFormatSymbols(locale, ns);
        }

        /// <summary>
        /// <icu/> Returns a <see cref="DecimalFormatSymbols"/> instance for the given locale with digits and symbols
        /// corresponding to the given <see cref="NumberingSystem"/>.
        /// <para/>
        /// This method behaves equivalently to <see cref="GetInstance(UCultureInfo)"/> called with a locale having a
        /// "numbers=xxxx" keyword.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="ns">The numbering system.</param>
        /// <returns>A <see cref="DecimalFormatSymbols"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        /// <draft>ICU 60</draft>
        public static DecimalFormatSymbols ForNumberingSystem(UCultureInfo locale, NumberingSystem ns) // ICU4N TODO: API - rename according to .NET naming conventions
        {
            return new DecimalFormatSymbols(locale, ns);
        }

        /// <summary>
        /// Returns an array of all locales for which the <see cref="GetInstance(CultureInfo)"/> methods of
        /// this class can return localized instances.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>An array of <see cref="CultureInfo"/>s for which localized <see cref="DecimalFormatSymbols"/> instances are available.</returns>
        /// <stable>ICU 3.8</stable>
        public static CultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from getAvailableLocales()
        {
            return ICUResourceBundle.GetCultures(types);
        }

        /// <summary>
        /// <icu/> Returns an array of all locales for which the <see cref="GetInstance(UCultureInfo)"/> methods of
        /// this class can return localized instances.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>An array of <see cref="UCultureInfo"/>s for which localized <see cref="DecimalFormatSymbols"/> instances are available.</returns>
        /// <stable>ICU 3.8 (retain)</stable>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from getAvailableULocales()
        {
            return ICUResourceBundle.GetUCultures(types);
        }


        /// <summary>
        /// Gets or sets the character used for zero. Different for Arabic, etc.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="DigitStrings"/> setter instead.</discouraged>
        public virtual char ZeroDigit
        {
            get => zeroDigit;
            set => SetZeroDigit(value);
        }

        /// <summary>
        /// Gets the array of characters used as digits, in order from 0 through 9.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        /// <seealso cref="DigitStrings"/>
        /// <discouraged>ICU 58 use <see cref="DigitStrings"/> instead.</discouraged>
        public virtual char[] Digits => (char[])digits.Clone();

        /// <summary>
        /// Sets the character used for zero.
        /// <para/>
        /// <b>Note:</b> This method propagates digit 1 to
        /// digit 9 by incrementing code point one by one.
        /// </summary>
        /// <param name="zeroDigit">The zero character.</param>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="DigitStrings"/> setter instead.</discouraged>
        private void SetZeroDigit(char zeroDigit)
        {
            this.zeroDigit = zeroDigit;

            // digitStrings or digits might be referencing a cached copy for
            // optimization purpose, so creating a copy before making a modification
            digitStrings = (string[])digitStrings.Clone();
            digits = (char[])digits.Clone();

            // Make digitStrings field and digits field in sync
            digitStrings[0] = char.ToString(zeroDigit);
            digits[0] = zeroDigit;

            // Always propagate to digits 1-9 for JDK and ICU4C consistency.
            for (int i = 1; i < 10; i++)
            {
                char d = (char)(zeroDigit + i);
                digitStrings[i] = char.ToString(d);
                digits[i] = d;
            }

            // Update codePointZero: it is simply zeroDigit.
            codePointZero = zeroDigit;
        }

        /// <summary>
        /// <icu/> Gets or sets the array of strings used as digits, in order from 0 through 9.
        /// <para/>
        /// <strong>Note:</strong> When the input array of digit strings contains any strings
        /// represented by multiple <see cref="char"/>s, then <see cref="Digits"/> will return
        /// the default digits ('0' - '9') and <see cref="ZeroDigit"/> will return the
        /// default zero digit ('0').
        /// </summary>
        /// <exception cref="ArgumentNullException">The setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The setter <paramref name="value"/> array length is not 10.</exception>
        /// <stable>ICU 58</stable>
        public virtual string[] DigitStrings // Equivalent to NumberFormatInfo.NativeDigits
        {
            get => (string[])digitStrings.Clone();
            set => SetDigitStrings(value);
        }

        /// <summary>
        /// Gets the array of strings used as digits, in order from 0 through 9.
        /// Package private method - doesn't create a defensively copy.
        /// <para/>
        /// <strong>WARNING:</strong> Mutating the returned array will cause undefined behavior.
        /// If you need to change the value of the array, use <see cref="DigitStrings"/> instead.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual string[] DigitStringsLocal => digitStrings; // ICU4N specific - marked internal instead of public

        /// <summary>
        /// If the digit strings array corresponds to a sequence of increasing code points, this method
        /// returns the code point corresponding to the first entry in the digit strings array. If the
        /// digit strings array is <em>not</em> a sequence of increasing code points, returns -1.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual int CodePointZero => codePointZero; // ICU4N specific - marked internal instead of public

        /// <summary>
        /// <icu/> Sets the array of strings used as digits, in order from 0 through 9.
        /// <para/>
        /// <strong>Note:</strong> When the input array of digit strings contains any strings
        /// represented by multiple <see cref="char"/>s, then <see cref="Digits"/> will return
        /// the default digits ('0' - '9') and <see cref="ZeroDigit"/> will return the
        /// default zero digit ('0').
        /// </summary>
        /// <param name="digitStrings">The array of digit strings. The length of the array must be exactly 10.</param>
        /// <exception cref="ArgumentNullException"><paramref name="digitStrings"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The array length is not 10.</exception>
        /// <seealso cref="DigitStrings"/>
        /// <stable>ICU 58</stable>
        private void SetDigitStrings(string[] digitStrings)
        {
            if (digitStrings == null)
            {
                throw new ArgumentNullException(nameof(digitStrings), "The input digit string array is null");
            }
            if (digitStrings.Length != 10)
            {
                throw new ArgumentException("Number of digit strings is not 10");
            }

            // Scan input array and create char[] representation if possible
            // Also update codePointZero if possible
            string[] tmpDigitStrings = new string[10];
            char[]? tmpDigits = new char[10];
            int tmpCodePointZero = -1;
            for (int i = 0; i < 10; i++)
            {
                string digitStr = digitStrings[i];
                if (digitStr == null)
                {
                    throw new ArgumentException("The input digit string array contains a null element");
                }
                tmpDigitStrings[i] = digitStr;
                int cp, cc;
                if (digitStr.Length == 0)
                {
                    cp = -1;
                    cc = 0;
                }
                else
                {
                    cp = Character.CodePointAt(digitStrings[i], 0);
                    cc = Character.CharCount(cp);
                }
                if (cc == digitStr.Length)
                {
                    // One code point in this digit.
                    // If it is 1 UTF-16 code unit long, set it in tmpDigits.
                    if (cc == 1 && tmpDigits != null)
                    {
                        tmpDigits[i] = (char)cp;
                    }
                    else
                    {
                        tmpDigits = null;
                    }
                    // Check for validity of tmpCodePointZero.
                    if (i == 0)
                    {
                        tmpCodePointZero = cp;
                    }
                    else if (cp != tmpCodePointZero + i)
                    {
                        tmpCodePointZero = -1;
                    }
                }
                else
                {
                    // More than one code point in this digit.
                    // codePointZero and tmpDigits are going to be invalid.
                    tmpCodePointZero = -1;
                    tmpDigits = null;
                }
            }

            this.digitStrings = tmpDigitStrings;
            this.codePointZero = tmpCodePointZero;

            if (tmpDigits is null)
            {
                // fallback to the default digit chars
                this.zeroDigit = DEF_DIGIT_CHARS_ARRAY[0];
                this.digits = DEF_DIGIT_CHARS_ARRAY;
            }
            else
            {
                this.zeroDigit = tmpDigits[0];
                this.digits = tmpDigits;
            }
        }

        /// <summary>
        /// Gets or sets the character used to represent a significant digit in a pattern.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public virtual char SignificantDigit
        {
            get => sigDigit;
            set => sigDigit = value;
        }

        /// <summary>
        /// Gets or sets the character used for grouping separator. Different for French, etc.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="GroupingSeparatorString"/> instead.</discouraged>
        public virtual char GroupingSeparator
        {
            get => groupingSeparator;
            set => SetGroupingSeparator(value);
        }

        /// <summary>
        /// Sets the character used for grouping separator. Different for French, etc.
        /// </summary>
        /// <param name="groupingSeparator">The thousands character.</param>
        /// <seealso cref="GroupingSeparatorString"/>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="GroupingSeparatorString"/> instead.</discouraged>
        private void SetGroupingSeparator(char groupingSeparator)
        {
            this.groupingSeparator = groupingSeparator;
            this.groupingSeparatorString = char.ToString(groupingSeparator);
        }

        /// <summary>
        /// <icu/> Gets or sets the string used for grouping separator. Different for French, etc.
        /// </summary>
        /// <stable>ICU 58</stable>
        public string GroupingSeparatorString // Equivalent to NumberFormatInfo.NumberGroupSeparator
        {
            get => groupingSeparatorString;
            set => SetGroupingSeparatorString(value);
        }

        /// <summary>
        /// <icu/> Sets the string used for grouping separator.
        /// <para/>
        /// <b>Note:</b> When the input grouping separator String is represented
        /// by multiple <see cref="char"/>s, then <see cref="GroupingSeparator"/> will
        /// return the default grouping separator character (',').
        /// </summary>
        /// <param name="groupingSeparatorString">The grouping separator string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="groupingSeparatorString"/> is <c>null</c>.</exception>
        /// <seealso cref="GroupingSeparatorString"/>
        /// <stable>ICU 58</stable>
        private void SetGroupingSeparatorString(string groupingSeparatorString)
        {
            if (groupingSeparatorString == null)
            {
                throw new ArgumentNullException(nameof(groupingSeparatorString), "The input grouping separator is null");
            }
            this.groupingSeparatorString = groupingSeparatorString;
            if (groupingSeparatorString.Length == 1)
            {
                this.groupingSeparator = groupingSeparatorString[0];
            }
            else
            {
                // Use the default grouping separator character as fallback
                this.groupingSeparator = DEF_GROUPING_SEPARATOR;
            }
        }

        /// <summary>
        /// Gets or sets the character used for decimal sign. Different for French, etc.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="DecimalSeparatorString"/> instead.</discouraged>
        public virtual char DecimalSeparator
        {
            get => decimalSeparator;
            set => SetDecimalSeparator(value);
        }

        /// <summary>
        /// Sets the character used for decimal sign. Different for French, etc.
        /// </summary>
        /// <param name="decimalSeparator">The decimal character.</param>
        /// <stable>ICU 2.0</stable>
        private void SetDecimalSeparator(char decimalSeparator)
        {
            this.decimalSeparator = decimalSeparator;
            this.decimalSeparatorString = char.ToString(decimalSeparator);
        }

        /// <summary>
        /// <icu/> Gets or sets the string used for decimal sign.
        /// <para/>
        /// <b>Note:</b> When the input decimal separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="DecimalSeparator"/> will
        /// return the default decimal separator character ('.').
        /// </summary>
        /// <stable>ICU 58</stable>
        public virtual string DecimalSeparatorString // Equivalent to NumberFormatInfo.NumberDecimalSeparator
        {
            get => decimalSeparatorString;
            set => SetDecimalSeparatorString(value);
        }

        /// <summary>
        /// <icu/> Sets the string used for decimal sign.
        /// <para/>
        /// <b>Note:</b> When the input decimal separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="DecimalSeparator"/> will
        /// return the default decimal separator character ('.').
        /// </summary>
        /// <param name="decimalSeparatorString">The decimal sign string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="decimalSeparatorString"/> is <c>null</c>.</exception>
        /// <seealso cref="DecimalSeparatorString"/>
        /// <stable>ICU 58</stable>
        private void SetDecimalSeparatorString(string decimalSeparatorString)
        {
            if (decimalSeparatorString == null)
            {
                throw new ArgumentNullException(nameof(decimalSeparatorString), "The input decimal separator is null");
            }
            this.decimalSeparatorString = decimalSeparatorString;
            if (decimalSeparatorString.Length == 1)
            {
                this.decimalSeparator = decimalSeparatorString[0];
            }
            else
            {
                // Use the default decimal separator character as fallback
                this.decimalSeparator = DEF_DECIMAL_SEPARATOR;
            }
        }

        /// <summary>
        /// Gets or sets the character used for mille percent sign. Different for Arabic, etc.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="PerMillString"/> instead.</discouraged>
        public virtual char PerMill
        {
            get => perMill;
            set => SetPerMill(value);
        }

        /// <summary>
        /// Sets the character used for mille percent sign. Different for Arabic, etc.
        /// </summary>
        /// <param name="perMill">The mille percent character.</param>
        /// <stable>ICU 2.0</stable>
        private void SetPerMill(char perMill)
        {
            this.perMill = perMill;
            this.perMillString = char.ToString(perMill);
        }

        /// <summary>
        /// <icu/> Gets or sets the string used for permille sign.
        /// <para/>
        /// <b>Note:</b> When the input permille string is represented
        /// by multiple <see cref="char"/>s, then <see cref="PerMill"/> will
        /// return the default permille character ('&#x2030;').
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string PerMillString // Equivalent to NumberFormatInfo.PerMilleSymbol
        {
            get => perMillString;
            set => SetPerMillString(value);
        }

        /// <summary>
        /// <icu/> Sets the string used for permille sign.
        /// <para/>
        /// <b>Note:</b> When the input permille string is represented
        /// by multiple <see cref="char"/>s, then <see cref="PerMill"/> will
        /// return the default permille character ('&#x2030;').
        /// </summary>
        /// <param name="perMillString">The permille string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="perMillString"/> is <c>null</c>.</exception>
        /// <seealso cref="PerMillString"/>
        /// <stable>ICU 58</stable>
        private void SetPerMillString(string perMillString)
        {
            if (perMillString == null)
            {
                throw new ArgumentNullException(nameof(perMillString), "The input permille string is null");
            }
            this.perMillString = perMillString;
            if (perMillString.Length == 1)
            {
                this.perMill = perMillString[0];
            }
            else
            {
                // Use the default permille character as fallback
                this.perMill = DEF_PERMILL;
            }
        }

        /// <summary>
        /// Gets or sets the character used for percent sign. Different for Arabic, etc.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="PercentString"/> instead.</discouraged>
        public virtual char Percent
        {
            get => percent;
            set => SetPercent(value);
        }

        /// <summary>
        /// Sets the character used for percent sign. Different for Arabic, etc.
        /// </summary>
        /// <param name="percent">The percent character.</param>
        /// <stable>ICU 2.0</stable>
        private void SetPercent(char percent)
        {
            this.percent = percent;
            this.percentString = char.ToString(percent);
        }

        /// <summary>
        /// <icu/>Gets or sets the string used for percent sign.
        /// <para/>
        /// <b>Note:</b> When the input grouping separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="Percent"/> will
        /// return the default percent sign character ('%').
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string PercentString // Equivalent to NumberFormatInfo.PercentSymbol
        {
            get => percentString;
            set => SetPercentString(value);
        }

        /// <summary>
        /// <icu/> Sets the string used for percent sign.
        /// <para/>
        /// <b>Note:</b> When the input grouping separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="Percent"/> will
        /// return the default percent sign character ('%').
        /// </summary>
        /// <param name="percentString">The percent string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="percentString"/> is <c>null</c>.</exception>
        /// <seealso cref="PercentString"/>
        /// <stable>ICU 58</stable>
        private void SetPercentString(string percentString)
        {
            if (percentString == null)
            {
                throw new ArgumentNullException(nameof(percentString), "The input percent sign is null");
            }
            this.percentString = percentString;
            if (percentString.Length == 1)
            {
                this.percent = percentString[0];
            }
            else
            {
                // Use default percent character as fallback
                this.percent = DEF_PERCENT;
            }
        }

        /// <summary>
        /// Gets or sets the character used for a digit in a pattern.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual char Digit
        {
            get => digit;
            set => digit = value;
        }

        /// <summary>
        /// Gets or sets the character used to separate positive and negative subpatterns
        /// in a pattern.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual char PatternSeparator
        {
            get => patternSeparator;
            set => patternSeparator = value;
        }

        /// <summary>
        /// Gets or sets the string used to represent infinity. Almost always left
        /// unchanged.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        //Bug 4194173 [Richard/GCL]

        public virtual string? Infinity // Equivalent to NumberFormatInfo.PositiveInfinitySymbol. Need to investigate how to do NegativeInfinitySymbol.
        {
            get => infinity;
            set => infinity = value;
        }

        /// <summary>
        /// Gets or sets the string used to represent NaN. Almost always left
        /// unchanged.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        //Bug 4194173 [Richard/GCL]
        public virtual string? NaN // Equivalent to NumberFormatInfo.NaNSymbol
        {
            get => naN;
            set => naN = value;
        }

        /// <summary>
        /// Gets or sets the character used to represent minus sign. If no explicit
        /// negative format is specified, one is formed by prefixing
        /// <see cref="MinusSign"/> to the positive format.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="MinusSignString"/> instead.</discouraged>
        public virtual char MinusSign
        {
            get => minusSign;
            set => SetMinusSign(value);
        }

        /// <summary>
        /// Sets the character used to represent minus sign. If no explicit
        /// negative format is specified, one is formed by prefixing
        /// <see cref="MinusSign"/> to the positive format.
        /// </summary>
        /// <param name="minusSign">The minus sign character.</param>
        /// <stable>ICU 2.0</stable>
        private void SetMinusSign(char minusSign)
        {
            this.minusSign = minusSign;
            this.minusString = char.ToString(minusSign);
        }

        /// <summary>
        /// <icu/>Gets or sets the string used to represent minus sign.
        /// <para/>
        /// <b>Note:</b> When the input minus sign string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MinusSign"/> will
        /// return the default minus sign character ('-').
        /// </summary>
        /// <exception cref="ArgumentNullException">The setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string MinusSignString // Equivalent to NumberFormatInfo.NegativeSign
        {
            get => minusString;
            set => SetMinusSignString(value);
        }

        /// <summary>
        /// <icu/> Sets the string used to represent minus sign.
        /// <para/>
        /// <b>Note:</b> When the input minus sign string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MinusSign"/> will
        /// return the default minus sign character ('-').
        /// </summary>
        /// <param name="minusSignString">The minus sign string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="minusSignString"/> is <c>null</c>.</exception>
        /// <seealso cref="MinusSignString"/>
        private void SetMinusSignString(string minusSignString)
        {
            if (minusSignString == null)
            {
                throw new ArgumentNullException(nameof(minusSignString), "The input minus sign is null");
            }
            this.minusString = minusSignString;
            if (minusSignString.Length == 1)
            {
                this.minusSign = minusSignString[0];
            }
            else
            {
                // Use the default minus sign as fallback
                this.minusSign = DEF_MINUS_SIGN;
            }
        }

        /// <summary>
        /// <icu/> Returns the localized plus sign used in localized patterns and formatted
        /// strings.
        /// </summary>
        /// <seealso cref="MinusSign"/>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="PlusSignString"/> instead.</discouraged>
        public virtual char PlusSign
        {
            get => plusSign;
            set => SetPlusSign(value);
        }

        /// <summary>
        /// <icu/> Sets the localized plus sign.
        /// </summary>
        /// <param name="plus">The plus sign used in localized patterns and formatted strings.</param>
        /// <seealso cref="PlusSign"/>
        /// <seealso cref="MinusSign"/>
        /// <stable>ICU 2.0</stable>
        private void SetPlusSign(char plus)
        {
            this.plusSign = plus;
            this.plusString = char.ToString(plus);
        }

        /// <summary>
        /// <icu/> Gets or sets the string used to represent plus sign.
        /// <para/>
        /// <strong>Note:</strong> When the input plus sign string is represented
        /// by multiple <see cref="char"/>s, then <see cref="PlusSign"/> will
        /// return the default plus sign character ('+').
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string PlusSignString // Equivalent to NumberFormatInfo.PositiveSign
        {
            get => plusString;
            set => SetPlusSignString(value);
        }

        /// <summary>
        /// <icu/> Sets the localized plus sign string.
        /// <para/>
        /// <strong>Note:</strong> When the input plus sign string is represented
        /// by multiple <see cref="char"/>s, then <see cref="PlusSign"/> will
        /// return the default plus sign character ('+').
        /// </summary>
        /// <param name="plusSignString">The plus sign string used in localized patterns and formatted strings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="plusSignString"/> is <c>null</c>.</exception>
        /// <seealso cref="PlusSignString"/>
        /// <stable>ICU 58</stable>
        private void SetPlusSignString(string plusSignString)
        {
            if (plusSignString == null)
            {
                throw new ArgumentNullException(nameof(plusSignString), "The input plus sign is null");
            }
            this.plusString = plusSignString;
            if (plusSignString.Length == 1)
            {
                this.plusSign = plusSignString[0];
            }
            else
            {
                // Use the default plus sign as fallback
                this.plusSign = DEF_PLUS_SIGN;
            }
        }

        /// <summary>
        /// Gets or sets the string denoting the local currency.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual string CurrencySymbol // Equivalent to NumberFormatInfo.CurrencySymbol
        {
            get => currencySymbol;
            set => currencySymbol = value;
        }

        /// <summary>
        /// Gets or sets the international string denoting the local currency.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual string InternationalCurrencySymbol
        {
            get => intlCurrencySymbol;
            set => intlCurrencySymbol = value;
        }

        /// <summary>
        /// Gets or sets the currency symbol. ICU clients should use the <see cref="ICU4N.Util.Currency"/> API directly.
        /// <para/>
        /// <strong>Note:</strong> ICU does not use the <see cref="DecimalFormatSymbols"/> for the currency
        /// any more. This API is present for API compatibility only.
        /// <para/>
        /// This also sets the currency symbol attribute to the currency's symbol
        /// in the <see cref="DecimalFormatSymbols"/>' locale, and the international currency
        /// symbol attribute to the currency's ISO 4217 currency code.
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            virtual Currency? Currency
        {
            get => currency;
            set => SetCurrency(value!);
        }

        /// <summary>
        /// Sets the currency.
        /// <para/>
        /// <strong>Note:</strong> ICU does not use the <see cref="DecimalFormatSymbols"/> for the currency
        /// any more. This API is present for API compatibility only.
        /// <para/>
        /// This also sets the currency symbol attribute to the currency's symbol
        /// in the <see cref="DecimalFormatSymbols"/>' locale, and the international currency
        /// symbol attribute to the currency's ISO 4217 currency code.
        /// </summary>
        /// <param name="currency">The new currency to be used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="currency"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.4</stable>
        private void SetCurrency(Currency currency)
        {
            if (currency is null)
                throw new ArgumentNullException(nameof(currency));

            this.currency = currency;
            intlCurrencySymbol = currency.CurrencyCode;
            currencySymbol = currency.GetSymbol(requestedLocale);
        }

        /// <summary>
        /// Gets or sets the monetary decimal separator.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <discouraged>ICU 58 use <see cref="MonetaryDecimalSeparatorString"/> instead.</discouraged>
        public virtual char MonetaryDecimalSeparator
        {
            get => monetarySeparator;
            set => SetMonetaryDecimalSeparator(value);
        }

        /// <summary>
        /// Sets the monetary decimal separator.
        /// </summary>
        /// <param name="sep">The monetary decimal separator character.</param>
        /// <stable>ICU 2.0</stable>
        private void SetMonetaryDecimalSeparator(char sep)
        {
            this.monetarySeparator = sep;
            this.monetarySeparatorString = char.ToString(sep);
        }

        /// <summary>
        /// <icu/> Gets or sets the monetary decimal separator string.
        /// <para/>
        /// <strong>Note:</strong> When the input monetary decimal separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MonetaryDecimalSeparatorString"/> will
        /// return the default monetary decimal separator character ('.').
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string MonetaryDecimalSeparatorString // Equivalent to NumberFormatInfo.CurrencyDecimalSeparator
        {
            get => monetarySeparatorString;
            set => SetMonetaryDecimalSeparatorString(value);
        }

        /// <summary>
        /// <icu/> Sets the monetary decimal separator string.
        /// <para/>
        /// <strong>Note:</strong> When the input monetary decimal separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MonetaryDecimalSeparatorString"/> will
        /// return the default monetary decimal separator character ('.').
        /// </summary>
        /// <param name="sep">The monetary decimal separator string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sep"/> is <c>null</c>.</exception>
        /// <seealso cref="MonetaryDecimalSeparatorString"/>
        /// <stable>ICU 58</stable>
        private void SetMonetaryDecimalSeparatorString(string sep)
        {
            if (sep == null)
            {
                throw new ArgumentNullException(nameof(sep), "The input monetary decimal separator is null");
            }
            this.monetarySeparatorString = sep;
            if (sep.Length == 1)
            {
                this.monetarySeparator = sep[0];
            }
            else
            {
                // Use default decimap separator character as fallbacl
                this.monetarySeparator = DEF_DECIMAL_SEPARATOR;
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the monetary grouping separator.
        /// </summary>
        /// <stable>ICU 3.6</stable>
        /// <discouraged>ICU 58 use <see cref="MonetaryDecimalSeparatorString"/> instead.</discouraged>
        public virtual char MonetaryGroupingSeparator
        {
            get => monetaryGroupingSeparator;
            set => SetMonetaryGroupingSeparator(value);
        }

        /// <summary>
        /// <icu/> Sets the monetary grouping separator.
        /// </summary>
        /// <param name="sep">The monetary grouping separator character.</param>
        /// <stable>ICU 3.6</stable>
        private void SetMonetaryGroupingSeparator(char sep)
        {
            this.monetaryGroupingSeparator = sep;
            this.monetaryGroupingSeparatorString = char.ToString(sep);
        }

        /// <summary>
        /// <icu/> Gets or sets the monetary grouping separator.
        /// <para/>
        /// <strong>Note:</strong> When the input grouping separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MonetaryDecimalSeparator"/> will
        /// return the default monetary grouping separator character (',').
        /// </summary>
        /// <exception cref="ArgumentNullException">Setter <paramref name="value"/> is <c>null</c>.</exception>
        /// <stable>ICU 58</stable>
        public virtual string MonetaryGroupingSeparatorString // Equivalent to NumberFormatInfo.CurrencyGroupSeparator
        {
            get => monetaryGroupingSeparatorString;
            set => SetMonetaryGroupingSeparatorString(value);
        }

        /// <summary>
        /// <icu/> Sets the monetary grouping separator string.
        /// <para/>
        /// <strong>Note:</strong> When the input grouping separator string is represented
        /// by multiple <see cref="char"/>s, then <see cref="MonetaryDecimalSeparator"/> will
        /// return the default monetary grouping separator character (',').
        /// </summary>
        /// <param name="sep">The monetary grouping separator string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sep"/> is <c>null</c>.</exception>
        /// <seealso cref="MonetaryDecimalSeparatorString"/>
        /// <stable>ICU 58</stable>
        private void SetMonetaryGroupingSeparatorString(string sep)
        {
            if (sep == null)
            {
                throw new ArgumentNullException(nameof(sep), "The input monetary grouping separator is null");
            }
            this.monetaryGroupingSeparatorString = sep;
            if (sep.Length == 1)
            {
                this.monetaryGroupingSeparator = sep[0];
            }
            else
            {
                // Use default grouping separator character as fallback
                this.monetaryGroupingSeparator = DEF_GROUPING_SEPARATOR;
            }
        }

        /// <summary>
        /// Gets the currency pattern string. Internal API for <see cref="NumberFormat"/>
        /// </summary>
        internal virtual string? CurrencyPattern => currencyPattern;

        /// <summary>
        /// Gets or sets the multiplication sign.
        /// </summary>
        /// <stable>ICU 54</stable>
        public virtual string? ExponentMultiplicationSign
        {
            get => exponentMultiplicationSign;
            set => exponentMultiplicationSign = value;
        }

        /// <summary>
        /// <icu/> Gets or sets the string used to separate the mantissa from the exponent.
        /// Examples: "x10^" for 1.23x10^4, "E" for 1.23E4. The localized exponent symbol is
        /// used in localized patterns and formatted strings.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual string? ExponentSeparator
        {
            get => exponentSeparator;
            set => exponentSeparator = value;
        }

        /// <summary>
        /// <icu/> Gets or sets the character used to pad numbers out to a specified width. This is
        /// not the pad character itself; rather, it is the special pattern character
        /// <em>preceding</em> the pad character.  In the pattern "*_#,##0", '*' is the pad
        /// escape, and '_' is the pad character.
        /// </summary>
        /// <seealso cref="DecimalFormat.FormatWidth"/>
        /// <seealso cref="DecimalFormat.PadPosition"/>
        /// <seealso cref="DecimalFormat.PadCharacter"/>
        /// <stable>ICU 2.0</stable>
        public virtual char PadEscape
        {
            get => padEscape;
            set => padEscape = value;
        }

        // ICU4N: De-nested CURRENCY_SPC_CURRENCY_MATCH, CURRENCY_SPC_SURROUNDING_MATCH, and CURRENCY_SPC_INSERT
        // and converted them into a new enum CurrencyPatternSpacing

        private string[] currencySpcBeforeSym;
        private string[] currencySpcAfterSym;

        /// <summary>
        /// <icu/> Returns the desired currency spacing value. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be null. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// For more information, see <a href="http://www.unicode.org/reports/tr35/#Currencies">
        /// UTS#35 section 5.10.2</a>.
        /// </summary>
        /// <param name="itemType">The spacing property to retrieve.</param>
        /// <param name="beforeCurrency"><c>true</c> to get the <c>beforeCurrency</c> values,
        /// <c>false</c> to get the <c>afterCurrency</c> values.</param>
        /// <returns>The value or <c>null</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="itemType"/> is out of range for <see cref="CurrencySpacingPattern"/>.</exception>
        /// <seealso cref="SetPatternForCurrencySpacing(CurrencySpacingPattern, bool, string)"/>
        /// <stable>ICU 4.2</stable>
        public virtual string GetPatternForCurrencySpacing(CurrencySpacingPattern itemType, bool beforeCurrency) // ICU4N TODO: Refactor this into 6 separate properties, this is ugly.
        {
            if (itemType < CurrencySpacingPattern.CurrencyMatch ||
                itemType > CurrencySpacingPattern.InsertBetween)
            {
                throw new ArgumentException("unknown currency spacing: " + itemType);
            }
            if (beforeCurrency)
            {
                return currencySpcBeforeSym[(int)itemType];
            }
            return currencySpcAfterSym[(int)itemType];
        }

        /// <summary>
        /// <icu/> Sets the indicated currency spacing pattern or value. See
        /// <see cref="GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/> for more information.
        /// 
        /// <para/>Values for currency match and surrounding match must be <see cref="UnicodeSet"/>
        /// patterns. Values for insert can be any string.
        /// 
        /// <para/><strong>Note:</strong> This is not currently in use by ICU4N.
        /// </summary>
        /// <param name="itemType">The spacing property to set.</param>
        /// <param name="beforeCurrency"><c>true</c> if the pattern is for before the currency symbol.
        /// <c>false</c> if the pattern is for after it.</param>
        /// <param name="pattern">Pattern string to override current setting; can be <c>null</c>.</param>
        /// <exception cref="ArgumentException"><paramref name="itemType"/> is out of range for <see cref="CurrencySpacingPattern"/>.</exception>
        /// <seealso cref="GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/>
        /// <stable>ICU 4.2</stable>
        public virtual void SetPatternForCurrencySpacing(CurrencySpacingPattern itemType, bool beforeCurrency, string pattern) // ICU4N TODO: Refactor this into 6 separate properties, this is ugly.
        {
            if (itemType < CurrencySpacingPattern.CurrencyMatch ||
                itemType > CurrencySpacingPattern.InsertBetween)
            {
                throw new ArgumentException("unknown currency spacing: " + itemType);
            }
            if (beforeCurrency)
            {
                currencySpcBeforeSym[(int)itemType] = pattern;
            }
            else
            {
                currencySpcAfterSym[(int)itemType] = pattern;
            }
        }

        /// <summary>
        /// Gets the locale for which this object was constructed.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual CultureInfo Culture => requestedLocale;

        /// <summary>
        /// Gets the locale for which this object was constructed.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public virtual UCultureInfo UCulture => ulocale;

        /// <summary>
        /// Creates a shallow copy of the current <see cref="object"/>.
        /// </summary>
        /// <returns>A shallow copy of the current <see cref="object"/>.</returns>
        /// <stable>ICU 2.0</stable>
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
        public override bool Equals(object? obj)
        {
            if (!(obj is DecimalFormatSymbols other))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            for (int i = 0; i <= (int)CurrencySpacingPattern.InsertBetween; i++)
            {
                if (!currencySpcBeforeSym[i].Equals(other.currencySpcBeforeSym[i], StringComparison.Ordinal))
                {
                    return false;
                }
                if (!currencySpcAfterSym[i].Equals(other.currencySpcAfterSym[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (other.digits == null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (digits[i] != other.zeroDigit + i)
                    {
                        return false;
                    }
                }
            }
            else if (!ArrayEqualityComparer<char>.OneDimensional.Equals(digits, other.digits))
            {
                return false;
            }

            return
                groupingSeparator == other.groupingSeparator &&
                decimalSeparator == other.decimalSeparator &&
                percent == other.percent &&
                perMill == other.perMill &&
                digit == other.digit &&
                minusSign == other.minusSign &&
                minusString.Equals(other.minusString, StringComparison.Ordinal) &&
                patternSeparator == other.patternSeparator &&
                StringComparer.Ordinal.Equals(infinity, other.infinity) &&
                StringComparer.Ordinal.Equals(NaN, other.naN) &&
                currencySymbol.Equals(other.currencySymbol, StringComparison.Ordinal) &&
                intlCurrencySymbol.Equals(other.intlCurrencySymbol, StringComparison.Ordinal) &&
                padEscape == other.padEscape &&
                plusSign == other.plusSign &&
                plusString.Equals(other.plusString, StringComparison.Ordinal) &&
                StringComparer.Ordinal.Equals(exponentSeparator, other.exponentSeparator) &&
                monetarySeparator == other.monetarySeparator &&
                monetaryGroupingSeparator == other.monetaryGroupingSeparator &&
                StringComparer.Ordinal.Equals(exponentMultiplicationSign, other.exponentMultiplicationSign);
        }

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
        public override int GetHashCode()
        {
            int result = digits[0];
            result = result * 37 + groupingSeparator;
            result = result * 37 + decimalSeparator;
            return result;
        }

        /// <summary>
        /// List of field names to be loaded from the data files.
        /// The indices of each name into the array correspond to the position of that item in the
        /// numberElements array.
        /// </summary>
        private static readonly string[] SYMBOL_KEYS = new string[] {
            "decimal",
            "group",
            "list",
            "percentSign",
            "minusSign",
            "plusSign",
            "exponential",
            "perMille",
            "infinity",
            "nan",
            "currencyDecimal",
            "currencyGroup",
            "superscriptingExponent"
        };

        /// <summary>
        /// Default digits
        /// </summary>
        private static readonly string[] DEF_DIGIT_STRINGS_ARRAY =
            new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private static readonly char[] DEF_DIGIT_CHARS_ARRAY =
            new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Default symbol characters, used for fallbacks.
        /// </summary>
        private const char DEF_DECIMAL_SEPARATOR = '.';
        private const char DEF_GROUPING_SEPARATOR = ',';
        private const char DEF_PERCENT = '%';
        private const char DEF_MINUS_SIGN = '-';
        private const char DEF_PLUS_SIGN = '+';
        private const char DEF_PERMILL = '\u2030';

        /// <summary>
        /// List of default values for the symbols.
        /// </summary>
        private static readonly string?[] SYMBOL_DEFAULTS = new string?[]
        {
            char.ToString(DEF_DECIMAL_SEPARATOR),  // decimal
            char.ToString(DEF_GROUPING_SEPARATOR), // group
            ";", // list
            char.ToString(DEF_PERCENT),    // percentSign
            char.ToString(DEF_MINUS_SIGN), // minusSign
            char.ToString(DEF_PLUS_SIGN),  // plusSign
            "E", // exponential
            char.ToString(DEF_PERMILL),    // perMille
            "\u221e", // infinity
            "NaN", // NaN
            null, // currency decimal
            null, // currency group
            "\u00D7" // superscripting exponent
        };

        /// <summary>
        /// Constants for path names in the data bundles.
        /// </summary>
        private const string LATIN_NUMBERING_SYSTEM = "latn";
        private const string NUMBER_ELEMENTS = "NumberElements";
        private const string SYMBOLS = "symbols";

        /// <summary>
        /// Sink for enumerating all of the decimal format symbols (more specifically, anything
        /// under the "NumberElements.symbols" tree).
        /// <para/>
        /// More specific bundles (en_GB) are enumerated before their parents (en_001, en, root):
        /// Only store a value if it is still missing, that is, it has not been overridden.
        /// </summary>
        private sealed class DecFmtDataSink : ResourceSink
        {

            private readonly string?[] numberElements; // Array where to store the characters (set in constructor)

            public DecFmtDataSink(string?[] numberElements)
            {
                this.numberElements = numberElements ?? throw new ArgumentNullException(nameof(numberElements));
            }

            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable symbolsTable = value.GetTable();
                for (int j = 0; symbolsTable.GetKeyAndValue(j, key, value); ++j)
                {
                    for (int i = 0; i < SYMBOL_KEYS.Length; i++)
                    {
                        if (key.ContentEquals(SYMBOL_KEYS[i]))
                        {
                            if (numberElements[i] == null)
                            {
                                numberElements[i] = value.ToString();
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the symbols from the locale data.
        /// </summary>
        [MemberNotNull(nameof(requestedLocale))]
        [MemberNotNull(nameof(ulocale))]
        [MemberNotNull(nameof(validLocale))]
        [MemberNotNull(nameof(actualLocale))]
        [MemberNotNull(nameof(digits))]
        [MemberNotNull(nameof(digitStrings))]
        [MemberNotNull(nameof(codePointZero))]
        [MemberNotNull(nameof(zeroDigit))]
        [MemberNotNull(nameof(decimalSeparator))]
        [MemberNotNull(nameof(decimalSeparatorString))]
        [MemberNotNull(nameof(groupingSeparator))]
        [MemberNotNull(nameof(groupingSeparatorString))]
        [MemberNotNull(nameof(patternSeparator))]
        [MemberNotNull(nameof(percent))]
        [MemberNotNull(nameof(percentString))]
        [MemberNotNull(nameof(minusSign))]
        [MemberNotNull(nameof(minusString))]
        [MemberNotNull(nameof(plusSign))]
        [MemberNotNull(nameof(plusString))]
        [MemberNotNull(nameof(exponentSeparator))]
        [MemberNotNull(nameof(perMill))]
        [MemberNotNull(nameof(perMillString))]
        [MemberNotNull(nameof(monetarySeparator))]
        [MemberNotNull(nameof(monetarySeparatorString))]
        [MemberNotNull(nameof(monetaryGroupingSeparator))]
        [MemberNotNull(nameof(monetaryGroupingSeparatorString))]
        [MemberNotNull(nameof(padEscape))]
        [MemberNotNull(nameof(sigDigit))]
        [MemberNotNull(nameof(intlCurrencySymbol))]
        [MemberNotNull(nameof(currencySymbol))]
        [MemberNotNull(nameof(currencySpcBeforeSym))]
        [MemberNotNull(nameof(currencySpcAfterSym))]
        private void Initialize(UCultureInfo locale, NumberingSystem? ns)
        {
            this.requestedLocale = locale.ToCultureInfo();
            this.ulocale = locale;

            // TODO: The cache requires a single key, so we just save the NumberingSystem into the
            // locale string. NumberingSystem is then decoded again in the loadData() method. It would
            // be more efficient if we didn't have to serialize and deserialize the NumberingSystem.
            UCultureInfo keyLocale = (ns == null) ? locale : locale.SetKeywordValue("numbers", ns.Name);
            CacheData data = cachedLocaleData.GetOrCreate(keyLocale, null /* unused */);

            SetCulture(data.validLocale, data.validLocale);
            SetDigitStrings(data.digits);
            string?[] numberElements = data.numberElements;

            // Copy data from the numberElements map into instance fields
            SetDecimalSeparatorString(numberElements[0]!);
            SetGroupingSeparatorString(numberElements[1]!);

            // See CLDR #9781
            // assert numberElements[2].length() == 1;
            patternSeparator = numberElements[2]![0];

            SetPercentString(numberElements[3]!);
            SetMinusSignString(numberElements[4]!);
            SetPlusSignString(numberElements[5]!);
            ExponentSeparator = numberElements[6];
            SetPerMillString(numberElements[7]!);
            Infinity = numberElements[8];
            NaN = numberElements[9];
            SetMonetaryDecimalSeparatorString(numberElements[10]!);
            SetMonetaryGroupingSeparatorString(numberElements[11]!);
            ExponentMultiplicationSign = numberElements[12];

            digit = '#';  // Localized pattern character no longer in CLDR
            padEscape = '*';
            sigDigit = '@';


            CurrencyDisplayInfo info = CurrencyData.Provider.GetInstance(locale, true);

            // Obtain currency data from the currency API.  This is strictly
            // for backward compatibility; we don't use DecimalFormatSymbols
            // for currency data anymore.
            currency = Currency.GetInstance(locale);
            if (currency != null)
            {
                intlCurrencySymbol = currency.CurrencyCode;
                currencySymbol = currency.GetName(locale, CurrencyNameStyle.SymbolName, out bool _);
                CurrencyFormatInfo fmtInfo = info.GetFormatInfo(intlCurrencySymbol);
                if (fmtInfo != null)
                {
                    currencyPattern = fmtInfo.CurrencyPattern;
                    MonetaryDecimalSeparatorString = fmtInfo.MonetaryDecimalSeparator;
                    MonetaryGroupingSeparatorString = fmtInfo.MonetaryGroupingSeparator;
                }
            }
            else
            {
                intlCurrencySymbol = "XXX";
                currencySymbol = "\u00A4"; // 'OX' currency symbol
            }


            // Get currency spacing data.
            InitSpacingInfo(info.GetSpacingInfo());
        }

        private static CacheData LoadData(UCultureInfo locale)
        {
            string nsName;
            // Attempt to set the decimal digits based on the numbering system for the requested locale.
            NumberingSystem ns = NumberingSystem.GetInstance(locale);
            string[] digits = new string[10];
            if (ns != null && ns.Radix == 10 && !ns.IsAlgorithmic &&
                    NumberingSystem.IsValidDigitString(ns.Description))
            {
                string digitString = ns.Description;

                for (int i = 0, offset = 0; i < 10; i++)
                {
                    int cp = digitString.CodePointAt(offset);
                    int nextOffset = offset + Character.CharCount(cp);
                    digits[i] = digitString.Substring(offset, nextOffset - offset); // ICU4N: Corrected 2nd parameter
                    offset = nextOffset;
                }
                nsName = ns.Name;
            }
            else
            {
                // Default numbering system
                digits = DEF_DIGIT_STRINGS_ARRAY;
                nsName = "latn";
            }

            // Open the resource bundle and get the locale IDs.
            // TODO: Is there a better way to get the locale than making an ICUResourceBundle instance?
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.
                    GetBundleInstance(ICUData.IcuBaseName, locale);
            // TODO: Determine actual and valid locale correctly.
            UCultureInfo validLocale = rb.UCulture;

            string?[] numberElements = new string[SYMBOL_KEYS.Length];

            // Load using a data sink
            DecFmtDataSink sink = new DecFmtDataSink(numberElements);
            try
            {
                rb.GetAllItemsWithFallback(string.Concat(NUMBER_ELEMENTS, "/", nsName, "/", SYMBOLS), sink);
            }
            catch (MissingManifestResourceException)
            {
                // The symbols don't exist for the given nsName and resource bundle.
                // Silently ignore and fall back to Latin.
            }

            // Load the Latin fallback if necessary
            bool hasNull = false;
            foreach (string? entry in numberElements)
            {
                if (entry == null)
                {
                    hasNull = true;
                    break;
                }
            }
            if (hasNull && !nsName.Equals(LATIN_NUMBERING_SYSTEM, StringComparison.Ordinal))
            {
                rb.GetAllItemsWithFallback(string.Concat(NUMBER_ELEMENTS, "/", LATIN_NUMBERING_SYSTEM, "/", SYMBOLS), sink);
            }

            // Fill in any remaining missing values
            for (int i = 0; i < SYMBOL_KEYS.Length; i++)
            {
                if (numberElements[i] == null)
                {
                    numberElements[i] = SYMBOL_DEFAULTS[i];
                }
            }

            // If monetary decimal or grouping were not explicitly set, then set them to be the same as
            // their non-monetary counterparts.
            if (numberElements[10] == null)
            {
                numberElements[10] = numberElements[0];
            }
            if (numberElements[11] == null)
            {
                numberElements[11] = numberElements[1];
            }

            return new CacheData(validLocale, digits, numberElements);
        }

        private void InitSpacingInfo(CurrencySpacingInfo spcInfo)
        {
            currencySpcBeforeSym = spcInfo.GetBeforeSymbols();
            currencySpcAfterSym = spcInfo.GetAfterSymbols();
        }

        // ICU4N TODO: Serialization
        /////**
        //// * Reads the default serializable fields, then if <code>serialVersionOnStream</code>
        //// * is less than 1, initialize <code>monetarySeparator</code> to be
        //// * the same as <code>decimalSeparator</code> and <code>exponential</code>
        //// * to be 'E'.
        //// * Finally, sets serialVersionOnStream back to the maximum allowed value so that
        //// * default serialization will work properly if this object is streamed out again.
        //// */
        ////private void readObject(ObjectInputStream stream)
        ////        throws IOException, ClassNotFoundException {

        ////        // TODO: it looks to me {dlf} that the serialization code was never updated
        ////        // to handle the actual/valid ulocale fields.

        ////        stream.defaultReadObject();
        ///////CLOVER:OFF
        ////// we don't have data for these old serialized forms any more
        ////if (serialVersionOnStream < 1)
        ////{
        ////    // Didn't have monetarySeparator or exponential field;
        ////    // use defaults.
        ////    monetarySeparator = decimalSeparator;
        ////    exponential = 'E';
        ////}
        ////if (serialVersionOnStream < 2)
        ////{
        ////    padEscape = '*';
        ////    plusSign = '+';
        ////    exponentSeparator = String.valueOf(exponential);
        ////    // Although we read the exponential field on stream to create the
        ////    // exponentSeparator, we don't do the reverse, since scientific
        ////    // notation isn't supported by the old classes, even though the
        ////    // symbol is there.
        ////}
        ///////CLOVER:ON
        ////if (serialVersionOnStream < 3)
        ////{
        ////    // Resurrected objects from old streams will have no
        ////    // locale.  There is no 100% fix for this.  A
        ////    // 90% fix is to construct a mapping of data back to
        ////    // locale, perhaps a hash of all our members.  This is
        ////    // expensive and doesn't seem worth it.
        ////    requestedLocale = Locale.getDefault();
        ////}
        ////if (serialVersionOnStream < 4)
        ////{
        ////    // use same default behavior as for versions with no Locale
        ////    ulocale = ULocale.forLocale(requestedLocale);
        ////}
        ////if (serialVersionOnStream < 5)
        ////{
        ////    // use the same one for groupingSeparator
        ////    monetaryGroupingSeparator = groupingSeparator;
        ////}
        ////if (serialVersionOnStream < 6)
        ////{
        ////    // Set null to CurrencySpacing related fields.
        ////    if (currencySpcBeforeSym == null)
        ////    {
        ////        currencySpcBeforeSym = new String[CURRENCY_SPC_INSERT + 1];
        ////    }
        ////    if (currencySpcAfterSym == null)
        ////    {
        ////        currencySpcAfterSym = new String[CURRENCY_SPC_INSERT + 1];
        ////    }
        ////    initSpacingInfo(CurrencyData.CurrencySpacingInfo.DEFAULT);
        ////}
        ////if (serialVersionOnStream < 7)
        ////{
        ////    // Set minusString,plusString from minusSign,plusSign
        ////    if (minusString == null)
        ////    {
        ////        minusString = String.valueOf(minusSign);
        ////    }
        ////    if (plusString == null)
        ////    {
        ////        plusString = String.valueOf(plusSign);
        ////    }
        ////}
        ////if (serialVersionOnStream < 8)
        ////{
        ////    if (exponentMultiplicationSign == null)
        ////    {
        ////        exponentMultiplicationSign = "\u00D7";
        ////    }
        ////}
        ////if (serialVersionOnStream < 9)
        ////{
        ////    // String version of digits
        ////    if (digitStrings == null)
        ////    {
        ////        digitStrings = new String[10];
        ////        if (digits != null && digits.length == 10)
        ////        {
        ////            zeroDigit = digits[0];
        ////            for (int i = 0; i < 10; i++)
        ////            {
        ////                digitStrings[i] = String.valueOf(digits[i]);
        ////            }
        ////        }
        ////        else
        ////        {
        ////            char digit = zeroDigit;
        ////            if (digits == null)
        ////            {
        ////                digits = new char[10];
        ////            }
        ////            for (int i = 0; i < 10; i++)
        ////            {
        ////                digits[i] = digit;
        ////                digitStrings[i] = String.valueOf(digit);
        ////                digit++;
        ////            }
        ////        }
        ////    }

        ////    // String version of symbols
        ////    if (decimalSeparatorString == null)
        ////    {
        ////        decimalSeparatorString = String.valueOf(decimalSeparator);
        ////    }
        ////    if (groupingSeparatorString == null)
        ////    {
        ////        groupingSeparatorString = String.valueOf(groupingSeparator);
        ////    }
        ////    if (percentString == null)
        ////    {
        ////        percentString = String.valueOf(percent);
        ////    }
        ////    if (perMillString == null)
        ////    {
        ////        perMillString = String.valueOf(perMill);
        ////    }
        ////    if (monetarySeparatorString == null)
        ////    {
        ////        monetarySeparatorString = String.valueOf(monetarySeparator);
        ////    }
        ////    if (monetaryGroupingSeparatorString == null)
        ////    {
        ////        monetaryGroupingSeparatorString = String.valueOf(monetaryGroupingSeparator);
        ////    }
        ////}

        ////serialVersionOnStream = currentSerialVersion;

        ////// recreate
        ////currency = Currency.getInstance(intlCurrencySymbol);

        ////// Refresh digitStrings in order to populate codePointZero
        ////setDigitStrings(digitStrings);
        ////    }

        /// <summary>
        /// Character used for zero.  This remains only for backward compatibility
        /// purposes.  The digits array below is now used to actively store the digits.
        /// </summary>
        /// <seealso cref="ZeroDigit"/>
        private char zeroDigit;

        /// <summary>
        /// Array of characters used for the digits 0-9 in order.
        /// </summary>
        private char[] digits;

        /// <summary>
        /// Array of Strings used for the digits 0-9 in order.
        /// </summary>
        /// <serial/>
        private string[] digitStrings;

        /// <summary>
        /// Dealing with code points is faster than dealing with strings when formatting. Because of
        /// this, we maintain a value containing the zero code point that is used whenever
        /// <see cref="digitStrings"/> represents a sequence of ten code points in order.
        /// <para/>
        /// If the value stored here is positive, it means that the code point stored in this value
        /// corresponds to the <see cref="digitStrings"/> array, and <see cref="codePointZero"/> can be
        /// used instead of the <see cref="digitStrings"/> array for the purposes of efficient formatting;
        /// if -1, then <see cref="digitStrings"/> does *not* contain a sequence of code points, and it
        /// must be used directly.
        /// <para/>
        /// It is assumed that <see cref="codePointZero"/> always shadows the value in <see cref="digitStrings"/>.
        /// <see cref="codePointZero"/> should never be set directly; rather, it should be updated only when
        /// <see cref="digitStrings"/> mutates. That is, the flow of information is
        /// <see cref="digitStrings"/> -> <see cref="codePointZero"/>, not the other way.
        /// </summary>
        [NonSerialized]
        private int codePointZero;

        /// <summary>
        /// Character used for thousands separator.
        /// </summary>
        /// <serial/>
        /// <seealso cref="GroupingSeparator"/>
        private char groupingSeparator;

        /// <summary>
        /// String used for thousands separator.
        /// </summary>
        /// <serial/>
        private string groupingSeparatorString;

        /// <summary>
        /// Character used for decimal sign.
        /// </summary>
        /// <serial/>
        /// <seealso cref="DecimalSeparator"/>
        private char decimalSeparator;

        /// <summary>
        /// String used for decimal sign.
        /// </summary>
        /// <serial/>
        private string decimalSeparatorString;

        /// <summary>
        /// Character used for mille percent sign.
        /// </summary>
        /// <serial/>
        /// <seealso cref="PerMill"/>
        private char perMill;

        /// <summary>
        /// String used for mille percent sign.
        /// </summary>
        /// <serial/>
        private string perMillString;

        /// <summary>
        /// Character used for percent sign.
        /// </summary>
        /// <serial/>
        /// <seealso cref="Percent"/>
        private char percent;

        /// <summary>
        /// String used for percent sign.
        /// </summary>
        /// <serial/>
        private string percentString;

        /// <summary>
        /// Character used for a digit in a pattern.
        /// </summary>
        /// <serial/>
        /// <seealso cref="Digit"/>
        private char digit;

        /// <summary>
        /// Character used for a significant digit in a pattern.
        /// </summary>
        /// <serial/>
        /// <seealso cref="SignificantDigit"/>
        private char sigDigit;

        /// <summary>
        /// Character used to separate positive and negative subpatterns
        /// in a pattern.
        /// </summary>
        /// <serial/>
        /// <seealso cref="PatternSeparator"/>
        private char patternSeparator;

        /// <summary>
        /// Character used to represent infinity.
        /// </summary>
        /// <serial/>
        /// <seealso cref="Infinity"/>
        private string? infinity;

        /// <summary>
        /// Character used to represent NaN.
        /// </summary>
        /// <serial/>
        /// <seealso cref="NaN"/>
        private string? naN;

        /// <summary>
        /// Character used to represent minus sign.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MinusSign"/>
        private char minusSign;

        /// <summary>
        /// String versions of minus sign.
        /// </summary>
        /// <serial/>
        /// <since>ICU 52</since>
        private string minusString;

        /// <summary>
        /// The character used to indicate a plus sign.
        /// </summary>
        /// <serial/>
        /// <since>AlphaWorks</since>
        private char plusSign;

        /// <summary>
        /// String versions of plus sign.
        /// </summary>
        /// <serial/>
        /// <since>ICU 52</since>
        private string plusString;

        /// <summary>
        /// String denoting the local currency, e.g. "$".
        /// </summary>
        /// <serial/>
        /// <seealso cref="CurrencySymbol"/>
        private string currencySymbol;

        /// <summary>
        /// International string denoting the local currency, e.g. "USD".
        /// </summary>
        /// <serial/>
        /// <seealso cref="InternationalCurrencySymbol"/>
        private string intlCurrencySymbol;

        /// <summary>
        /// The decimal separator character used when formatting currency values.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MonetaryDecimalSeparator"/>
        private char monetarySeparator; // Field new in JDK 1.1.6

        /// <summary>
        /// The decimal separator string used when formatting currency values.
        /// </summary>
        /// <serial/>
        private string monetarySeparatorString;

        /// <summary>
        /// The grouping separator character used when formatting currency values.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MonetaryGroupingSeparator"/>
        private char monetaryGroupingSeparator; // Field new in JDK 1.1.6

        /// <summary>
        /// The grouping separator string used when formatting currency values.
        /// </summary>
        /// <serial/>
        private string monetaryGroupingSeparatorString;

        /// <summary>
        /// The character used to distinguish the exponent in a number formatted
        /// in exponential notation, e.g. 'E' for a number such as "1.23E45".
        /// <para/>
        /// Note that this field has been superseded by <see cref="exponentSeparator"/>.
        /// It is retained for backward compatibility.
        /// </summary>
        /// <serial/>
#pragma warning disable CS0169, S1144 // Unused private types or members should be removed
        private char exponential;       // Field new in JDK 1.1.6
#pragma warning restore CS0169, S1144 // Unused private types or members should be removed

        /// <summary>
        /// The string used to separate the mantissa from the exponent.
        /// Examples: "x10^" for 1.23x10^4, "E" for 1.23E4.
        /// <para/>
        /// Note that this supersedes the <see cref="exponential"/> field.
        /// </summary>
        /// <serial/>
        /// <since>AlphaWorks</since>
        private string? exponentSeparator;

        /// <summary>
        /// The character used to indicate a padding character in a format,
        /// e.g., '*' in a pattern such as "$*_#,##0.00".
        /// </summary>
        /// <serial/>
        /// <since>AlphaWorks</since>
        private char padEscape;

        /// <summary>
        /// The locale for which this object was constructed.  Set to the
        /// default locale for objects resurrected from old streams.
        /// </summary>
        /// <since>ICU 2.2</since>
        private CultureInfo requestedLocale;

        /// <summary>
        /// The requested <see cref="UCultureInfo"/>. We keep the old locale for serialization compatibility.
        /// </summary>
        /// <since>ICU 3.2</since>
        private UCultureInfo ulocale;

        /// <summary>
        /// Exponent multiplication sign. e.g "x"
        /// </summary>
        /// <serial/>
        /// <since>ICU 54</since>
        private string? exponentMultiplicationSign = null;


        // ICU4N TODO: Serialization
        ////// Proclaim JDK 1.1 FCS compatibility
        ////private const long serialVersionUID = 5772796243397350300L;

        ////// The internal serial version which says which version was written
        ////// - 0 (default) for version up to JDK 1.1.5
        ////// - 1 for version from JDK 1.1.6, which includes two new fields:
        //////     monetarySeparator and exponential.
        ////// - 2 for version from AlphaWorks, which includes 3 new fields:
        //////     padEscape, exponentSeparator, and plusSign.
        ////// - 3 for ICU 2.2, which includes the locale field
        ////// - 4 for ICU 3.2, which includes the ULocale field
        ////// - 5 for ICU 3.6, which includes the monetaryGroupingSeparator field
        ////// - 6 for ICU 4.2, which includes the currencySpc* fields
        ////// - 7 for ICU 52, which includes the minusString and plusString fields
        ////// - 8 for ICU 54, which includes exponentMultiplicationSign field.
        ////// - 9 for ICU 58, which includes a series of String symbol fields.
        ////private static readonly int currentSerialVersion = 8; // ICU4N NOTE: This should not be a const so we can update it via this assembly

        /////**
        //// * Describes the version of <code>DecimalFormatSymbols</code> present on the stream.
        //// * Possible values are:
        //// * <ul>
        //// * <li><b>0</b> (or uninitialized): versions prior to JDK 1.1.6.
        //// *
        //// * <li><b>1</b>: Versions written by JDK 1.1.6 or later, which includes
        //// *      two new fields: <code>monetarySeparator</code> and <code>exponential</code>.
        //// * <li><b>2</b>: Version for AlphaWorks.  Adds padEscape, exponentSeparator,
        //// *      and plusSign.
        //// * <li><b>3</b>: Version for ICU 2.2, which adds locale.
        //// * <li><b>4</b>: Version for ICU 3.2, which adds ulocale.
        //// * <li><b>5</b>: Version for ICU 3.6, which adds monetaryGroupingSeparator.
        //// * <li><b>6</b>: Version for ICU 4.2, which adds currencySpcBeforeSym and
        //// *      currencySpcAfterSym.
        //// * <li><b>7</b>: Version for ICU 52, which adds minusString and plusString.
        //// * </ul>
        //// * When streaming out a <code>DecimalFormatSymbols</code>, the most recent format
        //// * (corresponding to the highest allowable <code>serialVersionOnStream</code>)
        //// * is always written.
        //// *
        //// * @serial
        //// */
        ////private int serialVersionOnStream = currentSerialVersion;

        private sealed class LocaleCache : SoftCache<UCultureInfo, CacheData>
        {
            public override CacheData GetOrCreate(UCultureInfo key, Func<UCultureInfo, CacheData> valueFactory)
            {
                return base.GetOrCreate(key, (locale) => DecimalFormatSymbols.LoadData(locale));
            }
        }

        /// <summary>
        /// cache to hold the NumberElements of a Locale.
        /// </summary>
        private static readonly CacheBase<UCultureInfo, CacheData> cachedLocaleData = new LocaleCache();

        /// <summary>
        /// 
        /// </summary>
        private string? currencyPattern = null;

        // -------- BEGIN ULocale boilerplate --------

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the locale of the resource containing the data. This is always
        /// at or above the valid locale. If the valid locale does not contain the
        /// specific data being requested, then the actual locale will be
        /// above the valid locale. If the object was not constructed from
        /// locale data, then the valid locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UCultureInfo? ActualCulture
            => actualLocale;

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the most specific locale for which any data exists.
        /// This is always at or above the requested locale, and at or below
        /// the actual locale. If the requested locale does not correspond
        /// to any resource data, then the valid locale will be above the
        /// requested locale. If the object was not constructed from locale
        /// data, then the actual locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UCultureInfo? ValidCulture
            => validLocale;

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale. This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// </summary>
        /// <param name="valid">The most specific locale containing any resource data, or <c>null</c>.</param>
        /// <param name="actual">The locale containing data used to construct this object, or <c>null</c>.</param>
        /// <seealso cref="UCultureInfo"/>
        internal void SetCulture(UCultureInfo? valid, UCultureInfo? actual) // ICU4N: Renamed from setLocale()
        {
            // Change the following to an assertion later
            if ((valid == null) != (actual == null))
            {
                ////CLOVER:OFF
                throw new ArgumentException();
                ////CLOVER:ON
            }
            // Another check we could do is that the actual locale is at
            // the same level or less specific than the valid locale.
            this.validLocale = valid;
            this.actualLocale = actual;
        }

        /// <summary>
        /// The most specific locale containing any resource data, or <c>null</c>.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        private UCultureInfo? validLocale;

        /// <summary>
        /// The locale containing data used to construct this object, or
        /// <c>null</c>.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        private UCultureInfo? actualLocale;

        // not serialized, reconstructed from intlCurrencyCode
        [NonSerialized]
        private Currency? currency;

        // -------- END ULocale boilerplate --------

        internal sealed class CacheData
        {
            internal readonly UCultureInfo validLocale;
            internal readonly string[] digits;
            internal readonly string?[] numberElements;

            public CacheData(UCultureInfo loc, string[] digits, string?[] numberElements)
            {
                validLocale = loc ?? throw new ArgumentNullException(nameof(loc));
                this.digits = digits ?? throw new ArgumentNullException(nameof(digits));
                this.numberElements = numberElements ?? throw new ArgumentNullException(nameof(numberElements));
            }
        }

    }

    /// <summary>
    /// Indicates currency matching info used in <see cref="DecimalFormatSymbols.GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/>.
    /// </summary>
    public enum CurrencySpacingPattern // ICU4N TODO: Merge this with CurrencySpacingInfo.SpacingPattern ?
    {
        /// <summary>
        /// <icu/> Indicates the currency match pattern used in
        /// <see cref="DecimalFormatSymbols.GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/>.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        CurrencyMatch = 0,

        /// <summary>
        /// <icu/> Indicates the surrounding match pattern used in
        /// <see cref="DecimalFormatSymbols.GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/>.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        SurroundingMatch = 1,

        /// <summary>
        /// <icu/> Indicates the insertion value used in
        /// <see cref="DecimalFormatSymbols.GetPatternForCurrencySpacing(CurrencySpacingPattern, bool)"/>.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        InsertBetween = 2,
    }
}
