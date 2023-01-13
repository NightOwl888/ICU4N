using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Resources;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Constants to be used to specify <see cref="NumberFormat"/> style.
    /// </summary>
    internal enum NumberFormatStyle // ICU4N: Marked internal until implementation is completed
    {
        /// <summary>
        /// <icu/> Constant to specify normal number style of format.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        NumberStyle = 0, // ICU4N TODO: API - remove the suffix Style in this enum, as it is redundant

        /// <summary>
        /// <icu/> Constant to specify general currency style of format. Defaults to
        /// <see cref="StandardCurrencyStyle"/>, using currency symbol, for example "$3.00", with
        /// non-accounting style for negative values (e.g. minus sign).
        /// The specific style may be specified using the -cf- locale key.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        CurrencyStyle = 1,

        /// <summary>
        /// <icu/> Constant to specify a style of format to display percent.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        PercentStyle = 2,

        /// <summary>
        /// <icu/> Constant to specify a style of format to display scientific number.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        ScientificStyle = 3,

        /// <summary>
        /// <icu/> Constant to specify a integer number style format.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        IntegerStyle = 4,

        /// <summary>
        /// <icu/> Constant to specify currency style of format which uses currency
        /// ISO code to represent currency, for example: "USD3.00".
        /// </summary>
        /// <stable>ICU 4.2</stable>
        ISOCurrencyStyle = 5,

        /// <summary>
        /// <icu/> Constant to specify currency style of format which uses currency
        /// long name with plural format to represent currency, for example,
        /// "3.00 US Dollars".
        /// </summary>
        /// <stable>ICU 4.2</stable>
        PluralCurrencyStyle = 6,

        /// <summary>
        /// <icu/> Constant to specify currency style of format which uses currency symbol
        /// to represent currency for accounting, for example: "($3.00), instead of
        /// "-$3.00" (<see cref="CurrencyStyle"/>).
        /// Overrides any style specified using -cf- key in locale.
        /// </summary>
        /// <stable>ICU 53</stable>
        AccountingCurrencyStyle = 7,

        /// <summary>
        /// <icu/> Constant to specify currency cash style of format which uses currency
        /// ISO code to represent currency, for example: "NT$3" instead of "NT$3.23".
        /// </summary>
        /// <stable>ICU 54</stable>
        CashCurrencyStyle = 8,

        /// <summary>
        /// <icu/>Constant to specify currency style of format which uses currency symbol
        /// to represent currency, for example "$3.00", using non-accounting style for
        /// negative values (e.g. minus sign).
        /// Overrides any style specified using -cf- key in locale.
        /// </summary>
        /// <stable>ICU 56</stable>
        StandardCurrencyStyle = 9,
    }


    internal abstract class NumberFormat : UFormat // ICU4N: Marked internal until implementation is completed
    {
        // ICU4N specific - moved constants to an enum named NumberFormatStyle

        /**
         * Field constant used to construct a FieldPosition object. Signifies that
         * the position of the integer part of a formatted number should be returned.
         * @see java.text.FieldPosition
         * @stable ICU 2.0
         */
        public const int IntegerField = 0;

        /**
         * Field constant used to construct a FieldPosition object. Signifies that
         * the position of the fraction part of a formatted number should be returned.
         * @see java.text.FieldPosition
         * @stable ICU 2.0
         */
        public const int FractionField = 1;

        /**
         * Formats a number and appends the resulting text to the given string buffer.
         * {@icunote} recognizes <code>BigInteger</code>
         * and <code>BigDecimal</code> objects.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public override StringBuffer Format(object number,
                                   StringBuffer toAppendTo,
                                   FieldPosition pos)
        {
            if (number is Long @long)
            {
                return Format(@long.ToInt64(), toAppendTo, pos);
            }
            else if (number is long l)
            {
                return Format(l, toAppendTo, pos);
            }
            else if (number is Numerics.BigMath.BigInteger javaBigInteger)
            {
                return Format(javaBigInteger, toAppendTo, pos);
            }
            else if (number is BigInteger bigInteger)
            {
                return Format(bigInteger, toAppendTo, pos);
            }
            else if (number is Numerics.BigMath.BigDecimal javaBigDecimal)
            {
                return Format(javaBigDecimal, toAppendTo, pos);
            }
            else if (number is BigDecimal bigDecimal)
            {
                return Format(bigDecimal, toAppendTo, pos);
            }
            else if (number is CurrencyAmount currencyAmount)
            {
                return Format(currencyAmount, toAppendTo, pos);
            }
            else if (number is Double @double)
            {
                return Format(@double.ToDouble(), toAppendTo, pos);
            }
            else if (number is J2N.Numerics.Number num)
            {
                return Format(num.ToDouble(), toAppendTo, pos);
            }
            else if (number.IsNumber())
            {
                return Format(Convert.ToDouble(number), toAppendTo, pos);
            }
            else
            {
                throw new ArgumentException("Cannot format given Object as a Number");
            }
        }

        /**
         * Parses text from a string to produce a number.
         * @param source the String to parse
         * @param parsePosition the position at which to start the parse
         * @return the parsed number, or null
         * @see java.text.NumberFormat#parseObject(String, ParsePosition)
         * @stable ICU 2.0
         */
        public override sealed object ParseObject(string source,
                                        ParsePosition parsePosition)
        {
            return Parse(source, parsePosition);
        }

        /**
         * Specialization of format.
         * @see java.text.Format#format(Object)
         * @stable ICU 2.0
         */
        public string Format(double number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /**
         * Specialization of format.
         * @see java.text.Format#format(Object)
         * @stable ICU 2.0
         */
        public string Format(long number)
        {
            StringBuffer buf = new StringBuffer(19);
            FieldPosition pos = new FieldPosition(0);
            Format(number, buf, pos);
            return buf.ToString();
        }

        /**
         * {@icu} Convenience method to format a BigInteger.
         * @stable ICU 2.0
         */
        public string Format(Numerics.BigMath.BigInteger number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /**
         * Convenience method to format a BigDecimal.
         * @stable ICU 2.0
         */
        public string Format(Numerics.BigMath.BigDecimal number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /**
         * {@icu} Convenience method to format an ICU BigDecimal.
         * @stable ICU 2.0
         */
        public string Format(Numerics.BigDecimal number) // ICU BigDecimal
    {
        return Format(number, new StringBuffer(),
                      new FieldPosition(0)).ToString();
    }

    /**
     * {@icu} Convenience method to format a CurrencyAmount.
     * @stable ICU 3.0
     */
    public string Format(CurrencyAmount currAmt)
        {
            return Format(currAmt, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /**
         * Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public abstract StringBuffer Format(double number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);

        /**
         * Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public abstract StringBuffer Format(long number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);

        // ICU4N TODO: System.Numerics.BigInteger overload
        /**
         * {@icu} Formats a BigInteger. Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public abstract StringBuffer Format(Numerics.BigMath.BigInteger number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);


        /**
         * {@icu} Formats a BigDecimal. Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public abstract StringBuffer Format(Numerics.BigMath.BigDecimal number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);
        /**
         * {@icu} Formats an ICU BigDecimal. Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 2.0
         */
        public abstract StringBuffer Format(Numerics.BigDecimal number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos); // ICU BigDecimal

        /**
         * {@icu} Formats a CurrencyAmount. Specialization of format.
         * @see java.text.Format#format(Object, StringBuffer, FieldPosition)
         * @stable ICU 3.0
         */
        public virtual StringBuffer Format(CurrencyAmount currAmt,
                                   StringBuffer toAppendTo,
                                   FieldPosition pos)
        {
            // Default implementation -- subclasses may override
            lock (this) // ICU4N TODO: Create specialized lock object - Note this is shared with subclasses
            {
                Currency save = Currency, curr = currAmt.Currency;
                bool same = curr.Equals(save);
                if (!same) Currency = curr;
                Format(currAmt.Number, toAppendTo, pos);
                if (!same) Currency = save;
            }
            return toAppendTo;
        }

        /// <summary>
        /// Returns a <see cref="long"/> if possible (e.g., within the range [<see cref="long.MinValue"/>,
        /// <see cref="long.MaxValue"/>] and with no decimals); otherwise, returns another type,
        /// such as a <see cref="BigDecimal"/>, <see cref="BigInteger"/>, or <see cref="double"/>. The return type is not
        /// guaranteed other than for the <see cref="long"/> case.
        /// <para/>
        /// If <see cref="ParseIntegerOnly"/> is set, will stop at a decimal
        /// point (or equivalent; e.g., for rational numbers "1 2/3", will stop
        /// after the 1).
        /// <para/>
        /// Does not throw an exception; if no object can be parsed, index is
        /// unchanged!
        /// <para/>
        /// For more detail on parsing, see the "Parsing" header in the class
        /// documentation of <see cref="DecimalFormat"/>.
        /// </summary>
        /// <seealso cref="ParseIntegerOnly"/>
        /// <seealso cref="DecimalFormat.ParseBigDecimal"/>
        /// <seealso cref="Formatter.ParseObject(string, ParsePosition)"/>
        /// <stable>ICU 2.0</stable>
        public abstract J2N.Numerics.Number Parse(string text, ParsePosition parsePosition);

        /**
         * Parses text from the beginning of the given string to produce a number.
         * The method might not use the entire text of the given string.
         *
         * @param text A String whose beginning should be parsed.
         * @return A Number parsed from the string.
         * @throws ParseException if the beginning of the specified string
         * cannot be parsed.
         * @see #format
         * @stable ICU 2.0
         */
        //Bug 4375399 [Richard/GCL]
        public virtual J2N.Numerics.Number Parse(string text)
        {
            ParsePosition parsePosition = new ParsePosition(0);
            J2N.Numerics.Number result = Parse(text, parsePosition);
            if (parsePosition.Index == 0)
            {
                throw new FormatException("Unparseable number: \"" + text + " ErrorIndex: " +
                                         parsePosition.ErrorIndex);
            }
            return result;
        }

        /**
         * Parses text from the given string as a CurrencyAmount.  Unlike
         * the parse() method, this method will attempt to parse a generic
         * currency name, searching for a match of this object's locale's
         * currency display names, or for a 3-letter ISO currency code.
         * This method will fail if this format is not a currency format,
         * that is, if it does not contain the currency pattern symbol
         * (U+00A4) in its prefix or suffix.
         *
         * @param text the text to parse
         * @param pos input-output position; on input, the position within
         * text to match; must have 0 &lt;= pos.getIndex() &lt; text.length();
         * on output, the position after the last matched character. If
         * the parse fails, the position in unchanged upon output.
         * @return a CurrencyAmount, or null upon failure
         * @stable ICU 49
         */
        public virtual CurrencyAmount ParseCurrency(string text, ParsePosition pos) // ICU4N - converted ICharSequence to string
        {
            ////CLOVER:OFF
            // Default implementation only -- subclasses should override
            J2N.Numerics.Number n = Parse(text.ToString(), pos);
            return n == null ? null : new CurrencyAmount(n, EffectiveCurrency);
            ////CLOVER:ON
        }

        /**
         * Returns true if this format will parse numbers as integers only.
         * For example in the English locale, with ParseIntegerOnly true, the
         * string "1234." would be parsed as the integer value 1234 and parsing
         * would stop at the "." character.  The decimal separator accepted
         * by the parse operation is locale-dependent and determined by the
         * subclass.
         *
         * @return true if this will parse integers only
         * @stable ICU 2.0
         */
        public virtual bool ParseIntegerOnly
        {
            get => parseIntegerOnly;
            set => parseIntegerOnly = value;
        }

        /////**
        //// * Sets whether to ignore the fraction part of a number when parsing
        //// * (defaults to false). If a string contains a decimal point, parsing will stop before the decimal
        //// * point. Note that determining whether a character is a decimal point depends on the locale.
        //// *
        //// * <p>For example, in <em>en-US</em>, parsing the string "123.45" will return the number 123 and
        //// * parse position 3.
        //// *
        //// * @param value true if this should parse integers only
        //// * @see #isParseIntegerOnly
        //// * @stable ICU 2.0
        //// */
        ////public void setParseIntegerOnly(boolean value)
        ////{
        ////    parseIntegerOnly = value;
        ////}

        /////**
        //// * {@icu} Sets whether strict parsing is in effect.  When this is true, the string
        //// * is required to be a stronger match to the pattern than when lenient parsing is in
        //// * effect.  More specifically, the following conditions cause a parse failure relative
        //// * to lenient mode (examples use the pattern "#,##0.#"):<ul>
        //// * <li>The presence and position of special symbols, including currency, must match the
        //// * pattern.<br>
        //// * '+123' fails (there is no plus sign in the pattern)</li>
        //// * <li>Leading or doubled grouping separators<br>
        //// * ',123' and '1,,234" fail</li>
        //// * <li>Groups of incorrect length when grouping is used<br>
        //// * '1,23' and '1234,567' fail, but '1234' passes</li>
        //// * <li>Grouping separators used in numbers followed by exponents<br>
        //// * '1,234E5' fails, but '1234E5' and '1,234E' pass ('E' is not an exponent when
        //// * not followed by a number)</li>
        //// * </ul>
        //// * When strict parsing is off,  all grouping separators are ignored.
        //// * This is the default behavior.
        //// * @param value True to enable strict parsing.  Default is false.
        //// * @see #isParseStrict
        //// * @stable ICU 3.6
        //// */
        ////public void setParseStrict(boolean value)
        ////{
        ////    parseStrict = value;
        ////}

        /**
         * {@icu} Returns whether strict parsing is in effect.
         * @return true if strict parsing is in effect
         * @see #setParseStrict
         * @stable ICU 3.6
         */
        public virtual bool ParseStrict
        {
            get => parseStrict;
            set => parseStrict = value;
        }

        /**
         * {@icu} Set a particular DisplayContext value in the formatter,
         * such as CAPITALIZATION_FOR_STANDALONE.
         *
         * @param context The DisplayContext value to set.
         * @stable ICU 53
         */
        public virtual void SetContext(DisplayContext context)
        {
            if (context.Type() == DisplayContextType.Capitalization)
            {
                capitalizationSetting = context;
            }
        }

        /**
         * {@icu} Get the formatter's DisplayContext value for the specified DisplayContext.Type,
         * such as CAPITALIZATION.
         *
         * @param type the DisplayContext.Type whose value to return
         * @return the current DisplayContext setting for the specified type
         * @stable ICU 53
         */
        public virtual DisplayContext GetContext(DisplayContextType type)
        {
            // ICU4N note: capitalizationSetting not nullable.
            return (type == DisplayContextType.Capitalization /*&& capitalizationSetting != null*/) ?
                    capitalizationSetting : DisplayContext.CapitalizationNone;
        }


        //============== Locale Stuff =====================

        /**
         * Returns the default number format for the current default <code>FORMAT</code> locale.
         * The default format is one of the styles provided by the other
         * factory methods: getNumberInstance, getIntegerInstance,
         * getCurrencyInstance or getPercentInstance.
         * Exactly which one is locale-dependent.
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.NumberStyle);
        }

        /**
         * Returns the default number format for the specified locale.
         * The default format is one of the styles provided by the other
         * factory methods: getNumberInstance, getCurrencyInstance or getPercentInstance.
         * Exactly which one is locale-dependent.
         * @stable ICU 2.0
         */
        public static NumberFormat GetInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.NumberStyle);
        }

        /**
         * {@icu} Returns the default number format for the specified locale.
         * The default format is one of the styles provided by the other
         * factory methods: getNumberInstance, getCurrencyInstance or getPercentInstance.
         * Exactly which one is locale-dependent.
         * @stable ICU 3.2
         */
        public static NumberFormat GetInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.NumberStyle);
        }

        /**
         * {@icu} Returns a specific style number format for default <code>FORMAT</code> locale.
         * @param style  number format style
         * @see Category#FORMAT
         * @stable ICU 4.2
         */
        public static NumberFormat GetInstance(NumberFormatStyle style)
        {
            return GetInstance(UCultureInfo.CurrentCulture, style);
        }

        /**
         * {@icu} Returns a specific style number format for a specific locale.
         * @param inLocale  the specific locale.
         * @param style     number format style
         * @stable ICU 4.2
         */
        public static NumberFormat GetInstance(CultureInfo inLocale, NumberFormatStyle style)
        {
            return GetInstance(inLocale.ToUCultureInfo(), style);
        }


        /**
         * Returns a general-purpose number format for the current default <code>FORMAT</code> locale.
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        public static NumberFormat GetNumberInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.NumberStyle);
        }

        /**
         * Returns a general-purpose number format for the specified locale.
         * @stable ICU 2.0
         */
        public static NumberFormat GetNumberInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.NumberStyle);
        }

        /**
         * {@icu} Returns a general-purpose number format for the specified locale.
         * @stable ICU 3.2
         */
        public static NumberFormat GetNumberInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.NumberStyle);
        }

        /**
         * Returns an integer number format for the current default <code>FORMAT</code> locale. The
         * returned number format is configured to round floating point numbers
         * to the nearest integer using IEEE half-even rounding (see {@link
         * com.ibm.icu.math.BigDecimal#ROUND_HALF_EVEN ROUND_HALF_EVEN}) for formatting,
         * and to parse only the integer part of an input string (see {@link
         * #isParseIntegerOnly isParseIntegerOnly}).
         *
         * @return a number format for integer values
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetIntegerInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.IntegerStyle);
        }

        /**
         * Returns an integer number format for the specified locale. The
         * returned number format is configured to round floating point numbers
         * to the nearest integer using IEEE half-even rounding (see {@link
         * com.ibm.icu.math.BigDecimal#ROUND_HALF_EVEN ROUND_HALF_EVEN}) for formatting,
         * and to parse only the integer part of an input string (see {@link
         * #isParseIntegerOnly isParseIntegerOnly}).
         *
         * @param inLocale the locale for which a number format is needed
         * @return a number format for integer values
         * @stable ICU 2.0
         */
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetIntegerInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.IntegerStyle);
        }

        /**
         * {@icu} Returns an integer number format for the specified locale. The
         * returned number format is configured to round floating point numbers
         * to the nearest integer using IEEE half-even rounding (see {@link
         * com.ibm.icu.math.BigDecimal#ROUND_HALF_EVEN ROUND_HALF_EVEN}) for formatting,
         * and to parse only the integer part of an input string (see {@link
         * #isParseIntegerOnly isParseIntegerOnly}).
         *
         * @param inLocale the locale for which a number format is needed
         * @return a number format for integer values
         * @stable ICU 3.2
         */
        public static NumberFormat GetIntegerInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.IntegerStyle);
        }

        /**
         * Returns a currency format for the current default <code>FORMAT</code> locale.
         * @return a number format for currency
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        public static NumberFormat GetCurrencyInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.CurrencyStyle);
        }

        /**
         * Returns a currency format for the specified locale.
         * @return a number format for currency
         * @stable ICU 2.0
         */
        public static NumberFormat GetCurrencyInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.CurrencyStyle);
        }

        /**
         * {@icu} Returns a currency format for the specified locale.
         * @return a number format for currency
         * @stable ICU 3.2
         */
        public static NumberFormat GetCurrencyInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.CurrencyStyle);
        }

        /**
         * Returns a percentage format for the current default <code>FORMAT</code> locale.
         * @return a number format for percents
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        public static NumberFormat GetPercentInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.PercentStyle);
        }

        /**
         * Returns a percentage format for the specified locale.
         * @return a number format for percents
         * @stable ICU 2.0
         */
        public static NumberFormat GetPercentInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.PercentStyle);
        }

        /**
         * {@icu} Returns a percentage format for the specified locale.
         * @return a number format for percents
         * @stable ICU 3.2
         */
        public static NumberFormat GetPercentInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.PercentStyle);
        }

        /**
         * {@icu} Returns a scientific format for the current default <code>FORMAT</code> locale.
         * @return a scientific number format
         * @see Category#FORMAT
         * @stable ICU 2.0
         */
        public static NumberFormat GetScientificInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.ScientificStyle);
        }

        /**
         * {@icu} Returns a scientific format for the specified locale.
         * @return a scientific number format
         * @stable ICU 2.0
         */
        public static NumberFormat GetScientificInstance(CultureInfo inLocale)
        {
            return GetInstance(inLocale.ToUCultureInfo(), NumberFormatStyle.ScientificStyle);
        }

        /**
         * {@icu} Returns a scientific format for the specified locale.
         * @return a scientific number format
         * @stable ICU 3.2
         */
        public static NumberFormat GetScientificInstance(UCultureInfo inLocale)
        {
            return GetInstance(inLocale, NumberFormatStyle.ScientificStyle);
        }

        // ICU4N specific - de-nested NumberFormatFactory and SimpleNumberFormatFactory classes

        // ===== Factory stuff =====


        // shim so we can build without service code
        internal abstract class NumberFormatShim
        {
            internal abstract CultureInfo[] GetCultures(UCultureTypes types); // ICU4N: Renamed from GetAvailableLocales
            internal abstract UCultureInfo[] GetUCultures(UCultureTypes types); // ICU4N: Renamed from GetAvailableULocales
            internal abstract object RegisterFactory(NumberFormatFactory f);
            internal abstract bool Unregister(object k);
            internal abstract NumberFormat CreateInstance(UCultureInfo l, NumberFormatStyle k);
        }

        private static NumberFormatShim shim;
        private static NumberFormatShim GetShim()
        {
            // Note: this instantiation is safe on loose-memory-model configurations
            // despite lack of synchronization, since the shim instance has no state--
            // it's all in the class init.  The worst problem is we might instantiate
            // two shim instances, but they'll share the same state so that's ok.
            if (shim == null)
            {
                try
                {
                    //Class <?> cls = Class.forName("com.ibm.icu.text.NumberFormatServiceShim");
                    //shim = (NumberFormatShim)cls.newInstance();
                    Type type = Type.GetType("ICU4N.Text.NumberFormatServiceShim");
                    shim = (NumberFormatShim)Activator.CreateInstance(type);
                    //shim = new NumberFormatServiceShim();

                }
                ////CLOVER:OFF
                catch (MissingManifestResourceException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // e.printStackTrace();
                    throw new Exception(e.ToString(), e);
                }
                ////CLOVER:ON
            }
            return shim;
        }

        /**
         * Returns the list of Locales for which NumberFormats are available.
         * @return the available locales
         * @stable ICU 2.0
         */
        public static CultureInfo[] GetCultures(UCultureTypes types)
        {
            if (shim == null)
            {
                return ICUResourceBundle.GetCultures(types);
            }
            return GetShim().GetCultures(types);
        }

        /**
         * {@icu} Returns the list of Locales for which NumberFormats are available.
         * @return the available locales
         * @draft ICU 3.2 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public static UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableULocales
        {
            if (shim is null)
            {
                return ICUResourceBundle.GetUCultures(types);
            }
            return GetShim().GetUCultures(types);
        }

        /// <summary>
        /// <icu/> Registers a new <see cref="NumberFormatFactory"/>. The factory is adopted by
        /// the service and must not be modified.  The returned object is a
        /// key that can be used to unregister this factory.
        /// <para/>
        /// Because ICU may choose to cache NumberFormat objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="NumberFormat.GetInstance()"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="factory">The factory to register.</param>
        /// <returns>A key with which to unregister the factory.</returns>
        /// <stable>ICU 2.6</stable>
        public static object RegisterFactory(NumberFormatFactory factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory), "factory must not be null");
            }
            return GetShim().RegisterFactory(factory);
        }

        /**
         * {@icu} Unregisters the factory or instance associated with this key (obtained from
         * registerInstance or registerFactory).
         * @param registryKey a key obtained from registerFactory
         * @return true if the object was successfully unregistered
         * @stable ICU 2.6
         */
        public static bool Unregister(object registryKey)
        {
            if (registryKey is null)
            {
                throw new ArgumentNullException(nameof(registryKey), "registryKey must not be null");
            }

            if (shim == null)
            {
                return false;
            }

            return shim.Unregister(registryKey);
        }

        // ===== End of factory stuff =====

        /**
         * {@inheritDoc}
         *
         * @stable ICU 2.0
         */
        public override int GetHashCode()
        {
            return maximumIntegerDigits * 37 + maxFractionDigits;
            // just enough fields for a reasonable distribution
        }

        /**
         * Overrides equals.
         * Two NumberFormats are equal if they are of the same class
         * and the settings (groupingUsed, parseIntegerOnly, maximumIntegerDigits, etc.
         * are equal.
         * @param obj the object to compare against
         * @return true if the object is equal to this.
         * @stable ICU 2.0
         */
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj)
                return true;
            if (GetType() != obj.GetType())
                return false;
            NumberFormat other = (NumberFormat)obj;
            return maximumIntegerDigits == other.maximumIntegerDigits
                && minimumIntegerDigits == other.minimumIntegerDigits
                && maximumFractionDigits == other.maximumFractionDigits
                && minimumFractionDigits == other.minimumFractionDigits
                && groupingUsed == other.groupingUsed
                && parseIntegerOnly == other.parseIntegerOnly
                && parseStrict == other.parseStrict
                && capitalizationSetting == other.capitalizationSetting;
        }

        /**
         * Overrides clone.
         * @stable ICU 2.0
         */
        public override object Clone()
        {
            NumberFormat other = (NumberFormat)base.Clone();
            return other;
        }

        /**
         * Returns true if grouping is used in this format. For example, in the
         * en_US locale, with grouping on, the number 1234567 will be formatted
         * as "1,234,567". The grouping separator as well as the size of each group
         * is locale-dependent and is determined by subclasses of NumberFormat.
         * Grouping affects both parsing and formatting.
         * @return true if grouping is used
         * @see #setGroupingUsed
         * @stable ICU 2.0
         */
        public virtual bool IsGroupingUsed
        {
            get => groupingUsed;
            set => groupingUsed = value;
        }

        /////**
        //// * Sets whether or not grouping will be used in this format.  Grouping
        //// * affects both parsing and formatting.
        //// * @see #isGroupingUsed
        //// * @param newValue true to use grouping.
        //// * @stable ICU 2.0
        //// */
        ////public void setGroupingUsed(boolean newValue)
        ////{
        ////    groupingUsed = newValue;
        ////}

        /**
         * Returns the maximum number of digits allowed in the integer portion of a
         * number.  The default value is 40, which subclasses can override.
         *
         * When formatting, if the number of digits exceeds this value, the highest-
         * significance digits are truncated until the limit is reached, in accordance
         * with UTS#35.
         *
         * This setting has no effect on parsing.
         *
         * @return the maximum number of integer digits
         * @see #setMaximumIntegerDigits
         * @stable ICU 2.0
         */
        public virtual int MaximumIntegerDigits
        {
            get => maximumIntegerDigits;
            set => maximumIntegerDigits = value;
        }

        /////**
        //// * Sets the maximum number of digits allowed in the integer portion of a
        //// * number. This must be &gt;= minimumIntegerDigits.  If the
        //// * new value for maximumIntegerDigits is less than the current value
        //// * of minimumIntegerDigits, then minimumIntegerDigits will also be set to
        //// * the new value.
        //// * @param newValue the maximum number of integer digits to be shown; if
        //// * less than zero, then zero is used.  Subclasses might enforce an
        //// * upper limit to this value appropriate to the numeric type being formatted.
        //// * @see #getMaximumIntegerDigits
        //// * @stable ICU 2.0
        //// */
        ////public void setMaximumIntegerDigits(int newValue)
        ////{
        ////    maximumIntegerDigits = Math.max(0, newValue);
        ////    if (minimumIntegerDigits > maximumIntegerDigits)
        ////        minimumIntegerDigits = maximumIntegerDigits;
        ////}

        /**
         * Returns the minimum number of digits allowed in the integer portion of a
         * number.  The default value is 1, which subclasses can override.
         * When formatting, if this value is not reached, numbers are padded on the
         * left with the locale-specific '0' character to ensure at least this
         * number of integer digits.  When parsing, this has no effect.
         * @return the minimum number of integer digits
         * @see #setMinimumIntegerDigits
         * @stable ICU 2.0
         */
        public virtual int MinimumIntegerDigits
        {
            get => minimumIntegerDigits;
            set => minimumIntegerDigits = value;
        }

        /////**
        //// * Sets the minimum number of digits allowed in the integer portion of a
        //// * number.  This must be &lt;= maximumIntegerDigits.  If the
        //// * new value for minimumIntegerDigits is more than the current value
        //// * of maximumIntegerDigits, then maximumIntegerDigits will also be set to
        //// * the new value.
        //// * @param newValue the minimum number of integer digits to be shown; if
        //// * less than zero, then zero is used. Subclasses might enforce an
        //// * upper limit to this value appropriate to the numeric type being formatted.
        //// * @see #getMinimumIntegerDigits
        //// * @stable ICU 2.0
        //// */
        ////public void setMinimumIntegerDigits(int newValue)
        ////{
        ////    minimumIntegerDigits = Math.max(0, newValue);
        ////    if (minimumIntegerDigits > maximumIntegerDigits)
        ////        maximumIntegerDigits = minimumIntegerDigits;
        ////}

        /**
         * Returns the maximum number of digits allowed in the fraction
         * portion of a number.  The default value is 3, which subclasses
         * can override.  When formatting, the exact behavior when this
         * value is exceeded is subclass-specific.  When parsing, this has
         * no effect.
         * @return the maximum number of fraction digits
         * @see #setMaximumFractionDigits
         * @stable ICU 2.0
         */
        public virtual int MaximumFractionDigits
        {
            get => maximumFractionDigits;
            set
            {
                maximumFractionDigits = Math.Max(0, value);
                if (maximumFractionDigits < minimumFractionDigits)
                    minimumFractionDigits = maximumFractionDigits;
            }
        }

        /////**
        //// * Sets the maximum number of digits allowed in the fraction portion of a
        //// * number. This must be &gt;= minimumFractionDigits.  If the
        //// * new value for maximumFractionDigits is less than the current value
        //// * of minimumFractionDigits, then minimumFractionDigits will also be set to
        //// * the new value.
        //// * @param newValue the maximum number of fraction digits to be shown; if
        //// * less than zero, then zero is used. The concrete subclass may enforce an
        //// * upper limit to this value appropriate to the numeric type being formatted.
        //// * @see #getMaximumFractionDigits
        //// * @stable ICU 2.0
        //// */
        ////public void setMaximumFractionDigits(int newValue)
        ////{
        ////    maximumFractionDigits = Math.max(0, newValue);
        ////    if (maximumFractionDigits < minimumFractionDigits)
        ////        minimumFractionDigits = maximumFractionDigits;
        ////}

        /**
         * Returns the minimum number of digits allowed in the fraction portion of a
         * number.  The default value is 0, which subclasses can override.
         * When formatting, if this value is not reached, numbers are padded on
         * the right with the locale-specific '0' character to ensure at least
         * this number of fraction digits.  When parsing, this has no effect.
         * @return the minimum number of fraction digits
         * @see #setMinimumFractionDigits
         * @stable ICU 2.0
         */
        public virtual int MinimumFractionDigits
        {
            get => minimumFractionDigits;
            set
            {
                minimumFractionDigits = Math.Max(0, value);
                if (maximumFractionDigits < minimumFractionDigits)
                    maximumFractionDigits = minimumFractionDigits;
            }
        }

        /////**
        //// * Sets the minimum number of digits allowed in the fraction portion of a
        //// * number.  This must be &lt;= maximumFractionDigits.  If the
        //// * new value for minimumFractionDigits exceeds the current value
        //// * of maximumFractionDigits, then maximumFractionDigits will also be set to
        //// * the new value.
        //// * @param newValue the minimum number of fraction digits to be shown; if
        //// * less than zero, then zero is used.  Subclasses might enforce an
        //// * upper limit to this value appropriate to the numeric type being formatted.
        //// * @see #getMinimumFractionDigits
        //// * @stable ICU 2.0
        //// */
        ////public void setMinimumFractionDigits(int newValue)
        ////{
        ////    minimumFractionDigits = Math.max(0, newValue);
        ////    if (maximumFractionDigits < minimumFractionDigits)
        ////        maximumFractionDigits = minimumFractionDigits;
        ////}

        // ICU4N TODO: Currency
        /////**
        //// * Sets the <tt>Currency</tt> object used to display currency
        //// * amounts.  This takes effect immediately, if this format is a
        //// * currency format.  If this format is not a currency format, then
        //// * the currency object is used if and when this object becomes a
        //// * currency format.
        //// * @param theCurrency new currency object to use.  May be null for
        //// * some subclasses.
        //// * @stable ICU 2.6
        //// */
        ////public virtual void setCurrency(Currency theCurrency)
        ////{
        ////    currency = theCurrency;
        ////}

        /**
         * Returns the <tt>Currency</tt> object used to display currency
         * amounts.  This may be null.
         * @stable ICU 2.6
         */
        public virtual Currency Currency
        {
            get => currency;
            set => currency = value;
        }

        /**
         * Returns the currency in effect for this formatter.  Subclasses
         * should override this method as needed.  Unlike getCurrency(),
         * this method should never return null.
         * @return a non-null Currency
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("his API is ICU internal only.")]
        protected virtual Currency EffectiveCurrency
        {
            get
            {
                Currency c = Currency;
                if (c == null)
                {
                    UCultureInfo uloc = ValidCulture;
                    if (uloc == null)
                    {
                        uloc = UCultureInfo.CurrentCulture;
                    }
                    c = Currency.GetInstance(uloc);
                }
                return c;
            }
        }

        // ICU4N specific - no need to add properties that cannot be supported
        /**
         * Returns the rounding mode used in this NumberFormat.  The default implementation of
         * tis method in NumberFormat always throws <code>UnsupportedOperationException</code>.
         * @return A rounding mode, between <code>BigDecimal.ROUND_UP</code>
         * and <code>BigDecimal.ROUND_UNNECESSARY</code>.
         * @see #setRoundingMode(int)
         * @stable ICU 4.0
         */
        public virtual Numerics.BigMath.RoundingMode RoundingMode
        {
            get => throw new NotSupportedException("RoundingMode getter must be implemented by the subclass implementation.");
            set => throw new NotSupportedException("RoundingMode setter must be implemented by the subclass implementation.");
        }

    /////**
    //// * Set the rounding mode used in this NumberFormat.  The default implementation of
    //// * tis method in NumberFormat always throws <code>UnsupportedOperationException</code>.
    //// * @param roundingMode A rounding mode, between
    //// * <code>BigDecimal.ROUND_UP</code> and
    //// * <code>BigDecimal.ROUND_UNNECESSARY</code>.
    //// * @see #getRoundingMode()
    //// * @stable ICU 4.0
    //// */
    ////public virtual void setRoundingMode(int roundingMode)
    ////{
    ////    throw new NotSupportedException(
    ////        "setRoundingMode must be implemented by the subclass implementation.");
    ////}


    /**
     * Returns a specific style number format for a specific locale.
     * @param desiredLocale  the specific locale.
     * @param choice         number format style
     * @throws IllegalArgumentException  if choice is not one of
     *                                   NUMBERSTYLE, CURRENCYSTYLE,
     *                                   PERCENTSTYLE, SCIENTIFICSTYLE,
     *                                   INTEGERSTYLE, ISOCURRENCYSTYLE,
     *                                   PLURALCURRENCYSTYLE, ACCOUNTINGCURRENCYSTYLE.
     *                                   CASHCURRENCYSTYLE, STANDARDCURRENCYSTYLE.
     * @stable ICU 4.2
     */
    public static NumberFormat GetInstance(UCultureInfo desiredLocale, NumberFormatStyle choice)
        {
            if (choice < NumberFormatStyle.NumberStyle || choice > NumberFormatStyle.StandardCurrencyStyle)
            {
                throw new ArgumentException(
                    "choice should be from NumberStyle to StandardCurrencyStyle");
            }
            //          if (shim == null) {
            //              return createInstance(desiredLocale, choice);
            //          } else {
            //              // TODO: shims must call setLocale() on object they create
            //              return getShim().createInstance(desiredLocale, choice);
            //          }
            return GetShim().CreateInstance(desiredLocale, choice);
        }

        // =======================privates===============================
        // Hook for service
        internal static NumberFormat CreateInstance(UCultureInfo desiredLocale, NumberFormatStyle choice)
        {
            // If the choice is PLURALCURRENCYSTYLE, the pattern is not a single
            // pattern, it is a pattern set, so we do not need to get them here.
            // If the choice is ISOCURRENCYSTYLE, the pattern is the currrency
            // pattern in the locale but by replacing the single currency sign
            // with double currency sign.
            string pattern = GetPattern(desiredLocale, choice);
            DecimalFormatSymbols symbols = new DecimalFormatSymbols(desiredLocale);

            // Here we assume that the locale passed in is in the canonical
            // form, e.g: pt_PT_@currency=PTE not pt_PT_PREEURO
            // This style wont work for currency plural format.
            // For currency plural format, the pattern is get from
            // the locale (from CurrencyUnitPatterns) without override.
            if (choice == NumberFormatStyle.CurrencyStyle || choice == NumberFormatStyle.ISOCurrencyStyle || choice == NumberFormatStyle.AccountingCurrencyStyle
                || choice == NumberFormatStyle.CashCurrencyStyle || choice == NumberFormatStyle.StandardCurrencyStyle)
            {
                string temp = symbols.CurrencyPattern;
                if (temp != null)
                {
                    pattern = temp;
                }
            }

            // replace single currency sign in the pattern with double currency sign
            // if the choice is ISOCURRENCYSTYLE.
            if (choice == NumberFormatStyle.ISOCurrencyStyle)
            {
                pattern = pattern.Replace("\u00A4", doubleCurrencyStr);
            }

            // Get the numbering system
            NumberingSystem ns = NumberingSystem.GetInstance(desiredLocale);
            if (ns == null)
            {
                return null;
            }

            NumberFormat format;

            if (ns != null && ns.IsAlgorithmic)
            {
                string nsDesc;
                string nsRuleSetGroup;
                string nsRuleSetName;
                UCultureInfo nsLoc;
                NumberPresentation desiredRulesType = NumberPresentation.NumberingSystem;

                nsDesc = ns.Description;
                int firstSlash = nsDesc.IndexOf('/');
                int lastSlash = nsDesc.LastIndexOf('/');

                if (lastSlash > firstSlash)
                {
                    string nsLocID = nsDesc.Substring(0, firstSlash); // ICU4N: Checked 2nd arg
                    nsRuleSetGroup = nsDesc.Substring(firstSlash + 1, lastSlash - (firstSlash + 1)); // ICU4N: Corrected 2nd arg
                    nsRuleSetName = nsDesc.Substring(lastSlash + 1);

                    nsLoc = new UCultureInfo(nsLocID);
                    if (nsRuleSetGroup.Equals("SpelloutRules"))
                    {
                        desiredRulesType = NumberPresentation.SpellOut;
                    }
                }
                else
                {
                    nsLoc = desiredLocale;
                    nsRuleSetName = nsDesc;
                }

                RuleBasedNumberFormat r = new RuleBasedNumberFormat(nsLoc, desiredRulesType);
                r.SetDefaultRuleSet(nsRuleSetName);
                format = r;
            }
            else
            {
                DecimalFormat f = new DecimalFormat(pattern, symbols, choice);
                // System.out.println("loc: " + desiredLocale + " choice: " + choice + " pat: " + pattern + " sym: " + symbols + " result: " + format);

                /*Bug 4408066
                 Add codes for the new method getIntegerInstance() [Richard/GCL]
                */
                // TODO: revisit this -- this is almost certainly not the way we want
                // to do this.  aliu 1/6/2004
                if (choice == NumberFormatStyle.IntegerStyle)
                {
                    f.MaximumFractionDigits = 0;
                    f.DecimalSeparatorAlwaysShown = false;
                    f.ParseIntegerOnly = true;
                }
                if (choice == NumberFormatStyle.CashCurrencyStyle)
                {
                    f.CurrencyUsage = CurrencyUsage.Cash;
                }
                if (choice == NumberFormatStyle.PluralCurrencyStyle)
                {
                    f.CurrencyPluralInfo = CurrencyPluralInfo.GetInstance(desiredLocale);
                }
                format = f;
            }
            // TODO: the actual locale of the *pattern* may differ from that
            // for the *symbols*.  For now, we use the data for the symbols.
            // Revisit this.
            UCultureInfo valid = symbols.ValidCulture;
            UCultureInfo actual = symbols.ActualCulture;
            format.SetCulture(valid, actual);

            return format;
        }

        /**
         * Returns the pattern for the provided locale and choice.
         * @param forLocale the locale of the data.
         * @param choice the pattern format.
         * @return the pattern
         * @deprecated ICU 3.4 subclassers should override getPattern(ULocale, int) instead of this method.
         */
        [Obsolete("ICU 3.4 subclassers should override GetPattern(ULocale, int) instead of this method.")]
        protected internal static string GetPattern(CultureInfo forLocale, NumberFormatStyle choice)
        {
            return GetPattern(forLocale.ToUCultureInfo(), choice);
        }

        /**
         * Returns the pattern for the provided locale and choice.
         * @param forLocale the locale of the data.
         * @param choice the pattern format.
         * @return the pattern
         * @stable ICU 3.2
         */
        protected internal static string GetPattern(UCultureInfo forLocale, NumberFormatStyle choice)
        {
#pragma warning disable 612, 618
            return GetPatternForStyle(forLocale, choice);
#pragma warning restore 612, 618
        }

        /**
         * Returns the pattern for the provided locale and choice.
         * @param forLocale the locale of the data.
         * @param choice the pattern format.
         * @return the pattern
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static string GetPatternForStyle(UCultureInfo forLocale, NumberFormatStyle choice)
        {
            NumberingSystem ns = NumberingSystem.GetInstance(forLocale);
            string nsName = ns.Name;
            return GetPatternForStyleAndNumberingSystem(forLocale, nsName, choice);
        }

        /**
         * Returns the pattern for the provided locale, numbering system, and choice.
         * @param forLocale the locale of the data.
         * @param nsName The name of the numbering system, like "latn".
         * @param choice the pattern format.
         * @return the pattern
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static string GetPatternForStyleAndNumberingSystem(UCultureInfo forLocale, string nsName, NumberFormatStyle choice)
        {
            /* for ISOCURRENCYSTYLE and PLURALCURRENCYSTYLE,
             * the pattern is the same as the pattern of CURRENCYSTYLE
             * but by replacing the single currency sign with
             * double currency sign or triple currency sign.
             */
            string patternKey = null;
            switch (choice)
            {
                case NumberFormatStyle.NumberStyle:
                case NumberFormatStyle.IntegerStyle:
                case NumberFormatStyle.PluralCurrencyStyle:
                    patternKey = "decimalFormat";
                    break;
                case NumberFormatStyle.CurrencyStyle:
                    patternKey = (forLocale.Keywords.TryGetValue("cf", out string cfKeyValue)
                        && cfKeyValue != null && cfKeyValue.Equals("account")) ?
                            "accountingFormat" : "currencyFormat";
                    break;
                case NumberFormatStyle.CashCurrencyStyle:
                case NumberFormatStyle.ISOCurrencyStyle:
                case NumberFormatStyle.StandardCurrencyStyle:
                    patternKey = "currencyFormat";
                    break;
                case NumberFormatStyle.PercentStyle:
                    patternKey = "percentFormat";
                    break;
                case NumberFormatStyle.ScientificStyle:
                    patternKey = "scientificFormat";
                    break;
                case NumberFormatStyle.AccountingCurrencyStyle:
                    patternKey = "accountingFormat";
                    break;
                default:
                    Debug.Assert(false);
                    patternKey = "decimalFormat";
                    break;
            }

            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle
                .GetBundleInstance(ICUData.IcuBaseName, forLocale);

            string result = rb.FindStringWithFallback(
                        "NumberElements/" + nsName + "/patterns/" + patternKey);
            if (result == null)
            {
                result = rb.GetStringWithFallback("NumberElements/latn/patterns/" + patternKey);
            }

            return result;
        }

        /**
         * First, read in the default serializable data.
         *
         * Then, if <code>serialVersionOnStream</code> is less than 1, indicating that
         * the stream was written by JDK 1.1,
         * set the <code>int</code> fields such as <code>maximumIntegerDigits</code>
         * to be equal to the <code>byte</code> fields such as <code>maxIntegerDigits</code>,
         * since the <code>int</code> fields were not present in JDK 1.1.
         * Finally, set serialVersionOnStream back to the maximum allowed value so that
         * default serialization will work properly if this object is streamed out again.
         */
        private void ReadObject(Stream stream)
        {
            // ICU4N TODO: Serialization
            //stream.defaultReadObject();
            /////CLOVER:OFF
            //// we don't have serialization data for this format
            //if (serialVersionOnStream< 1) {
            //    // Didn't have additional int fields, reassign to use them.
            //    maximumIntegerDigits = maxIntegerDigits;
            //    minimumIntegerDigits = minIntegerDigits;
            //    maximumFractionDigits = maxFractionDigits;
            //    minimumFractionDigits = minFractionDigits;
            //}
            //if (serialVersionOnStream< 2) {
            //    // Didn't have capitalizationSetting, set it to default
            //    capitalizationSetting = DisplayContext.CAPITALIZATION_NONE;
            //}
            /////CLOVER:ON
            ////*Bug 4185761
            //  Validate the min and max fields [Richard/GCL]
            //*/
            //if (minimumIntegerDigits > maximumIntegerDigits ||
            //    minimumFractionDigits > maximumFractionDigits ||
            //    minimumIntegerDigits< 0 || minimumFractionDigits< 0) {
            //    throw new InvalidObjectException("Digit count range invalid");
            //}
            //serialVersionOnStream = currentSerialVersion;
        }

        /**
         * Write out the default serializable data, after first setting
         * the <code>byte</code> fields such as <code>maxIntegerDigits</code> to be
         * equal to the <code>int</code> fields such as <code>maximumIntegerDigits</code>
         * (or to <code>Byte.MAX_VALUE</code>, whichever is smaller), for compatibility
         * with the JDK 1.1 version of the stream format.
         */
        private void WriteObject(Stream stream)
        {
            // ICU4N TOOD: Serialization
            //maxIntegerDigits = (maximumIntegerDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
            //        (byte)maximumIntegerDigits;
            //minIntegerDigits = (minimumIntegerDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
            //        (byte)minimumIntegerDigits;
            //maxFractionDigits = (maximumFractionDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
            //        (byte)maximumFractionDigits;
            //minFractionDigits = (minimumFractionDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
            //        (byte)minimumFractionDigits;
            //stream.defaultWriteObject();
        }

        // Unused -- Alan 2003-05
        //    /**
        //     * Cache to hold the NumberPatterns of a Locale.
        //     */
        //    private static final Hashtable cachedLocaleData = new Hashtable(3);

        private static readonly char[] doubleCurrencySign = { (char)0xA4, (char)0xA4 };
        private static readonly string doubleCurrencyStr = new string(doubleCurrencySign);

        /*Bug 4408066
          Add Field for the new method getIntegerInstance() [Richard/GCL]
        */


        /**
         * True if the the grouping (i.e. thousands) separator is used when
         * formatting and parsing numbers.
         *
         * @serial
         * @see #isGroupingUsed
         */
        private bool groupingUsed = true;

        /////**
        //// * The maximum number of digits allowed in the integer portion of a
        //// * number.  <code>maxIntegerDigits</code> must be greater than or equal to
        //// * <code>minIntegerDigits</code>.
        //// * <p>
        //// * <strong>Note:</strong> This field exists only for serialization
        //// * compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        //// * <code>int</code> field <code>maximumIntegerDigits</code> is used instead.
        //// * When writing to a stream, <code>maxIntegerDigits</code> is set to
        //// * <code>maximumIntegerDigits</code> or <code>Byte.MAX_VALUE</code>,
        //// * whichever is smaller.  When reading from a stream, this field is used
        //// * only if <code>serialVersionOnStream</code> is less than 1.
        //// *
        //// * @serial
        //// * @see #getMaximumIntegerDigits
        //// */
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte maxIntegerDigits = 40; // ICU4N: Not used (for now)
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /////**
        //// * The minimum number of digits allowed in the integer portion of a
        //// * number.  <code>minimumIntegerDigits</code> must be less than or equal to
        //// * <code>maximumIntegerDigits</code>.
        //// * <p>
        //// * <strong>Note:</strong> This field exists only for serialization
        //// * compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        //// * <code>int</code> field <code>minimumIntegerDigits</code> is used instead.
        //// * When writing to a stream, <code>minIntegerDigits</code> is set to
        //// * <code>minimumIntegerDigits</code> or <code>Byte.MAX_VALUE</code>,
        //// * whichever is smaller.  When reading from a stream, this field is used
        //// * only if <code>serialVersionOnStream</code> is less than 1.
        //// *
        //// * @serial
        //// * @see #getMinimumIntegerDigits
        //// */
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte minIntegerDigits = 1; // ICU4N: Not used (for now)
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /////**
        //// * The maximum number of digits allowed in the fractional portion of a
        //// * number.  <code>maximumFractionDigits</code> must be greater than or equal to
        //// * <code>minimumFractionDigits</code>.
        //// * <p>
        //// * <strong>Note:</strong> This field exists only for serialization
        //// * compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        //// * <code>int</code> field <code>maximumFractionDigits</code> is used instead.
        //// * When writing to a stream, <code>maxFractionDigits</code> is set to
        //// * <code>maximumFractionDigits</code> or <code>Byte.MAX_VALUE</code>,
        //// * whichever is smaller.  When reading from a stream, this field is used
        //// * only if <code>serialVersionOnStream</code> is less than 1.
        //// *
        //// * @serial
        //// * @see #getMaximumFractionDigits
        //// */
        private byte maxFractionDigits = 3;    // invariant, >= minFractionDigits

        /////**
        //// * The minimum number of digits allowed in the fractional portion of a
        //// * number.  <code>minimumFractionDigits</code> must be less than or equal to
        //// * <code>maximumFractionDigits</code>.
        //// * <p>
        //// * <strong>Note:</strong> This field exists only for serialization
        //// * compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        //// * <code>int</code> field <code>minimumFractionDigits</code> is used instead.
        //// * When writing to a stream, <code>minFractionDigits</code> is set to
        //// * <code>minimumFractionDigits</code> or <code>Byte.MAX_VALUE</code>,
        //// * whichever is smaller.  When reading from a stream, this field is used
        //// * only if <code>serialVersionOnStream</code> is less than 1.
        //// *
        //// * @serial
        //// * @see #getMinimumFractionDigits
        //// */
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte minFractionDigits = 0;
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /**
         * True if this format will parse numbers as integers only.
         *
         * @serial
         * @see #isParseIntegerOnly
         */
        private bool parseIntegerOnly = false;

        // new fields for 1.2.  byte is too small for integer digits.

        /**
         * The maximum number of digits allowed in the integer portion of a
         * number.  <code>maximumIntegerDigits</code> must be greater than or equal to
         * <code>minimumIntegerDigits</code>.
         *
         * @serial
         * @see #getMaximumIntegerDigits
         */
        private int maximumIntegerDigits = 40;

        /**
         * The minimum number of digits allowed in the integer portion of a
         * number.  <code>minimumIntegerDigits</code> must be less than or equal to
         * <code>maximumIntegerDigits</code>.
         *
         * @serial
         * @see #getMinimumIntegerDigits
         */
        private int minimumIntegerDigits = 1;

        /**
         * The maximum number of digits allowed in the fractional portion of a
         * number.  <code>maximumFractionDigits</code> must be greater than or equal to
         * <code>minimumFractionDigits</code>.
         *
         * @serial
         * @see #getMaximumFractionDigits
         */
        private int maximumFractionDigits = 3;    // invariant, >= minFractionDigits

        /**
         * The minimum number of digits allowed in the fractional portion of a
         * number.  <code>minimumFractionDigits</code> must be less than or equal to
         * <code>maximumFractionDigits</code>.
         *
         * @serial
         * @see #getMinimumFractionDigits
         */
        private int minimumFractionDigits = 0;

        /**
         * Currency object used to format currencies.  Subclasses may
         * ignore this if they are not currency formats.  This will be
         * null unless a subclass sets it to a non-null value.
         * @since ICU 2.6
         */
        private Currency currency;

        internal static readonly int currentSerialVersion = 2;

        /////**
        //// * Describes the version of <code>NumberFormat</code> present on the stream.
        //// * Possible values are:
        //// * <ul>
        //// * <li><b>0</b> (or uninitialized): the JDK 1.1 version of the stream format.
        //// *     In this version, the <code>int</code> fields such as
        //// *     <code>maximumIntegerDigits</code> were not present, and the <code>byte</code>
        //// *     fields such as <code>maxIntegerDigits</code> are used instead.
        //// *
        //// * <li><b>1</b>: the JDK 1.2 version of the stream format.  The values of the
        //// *     <code>byte</code> fields such as <code>maxIntegerDigits</code> are ignored,
        //// *     and the <code>int</code> fields such as <code>maximumIntegerDigits</code>
        //// *     are used instead.
        //// *
        //// * <li><b>2</b>: adds capitalizationSetting.
        //// * </ul>
        //// * When streaming out a <code>NumberFormat</code>, the most recent format
        //// * (corresponding to the highest allowable <code>serialVersionOnStream</code>)
        //// * is always written.
        //// *
        //// * @serial
        //// */
        private int serialVersionOnStream = currentSerialVersion;

        // Removed "implements Cloneable" clause.  Needs to update serialization
        // ID for backward compatibility.
        //private static readonly long serialVersionUID = -2308460125733713944L;

        /// <summary>
        /// Empty constructor.  Public for API compatibility with historic versions of
        /// java.text.NumberFormat which had public constructor even though this is
        /// an abstract class.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public NumberFormat()
        {
        }

        // new in ICU4J 3.6
        private bool parseStrict;

        /// <summary>
        /// Capitalization context setting, new in ICU 53
        /// </summary>
        /// <serial/>
        private DisplayContext capitalizationSetting = DisplayContext.CapitalizationNone;

        // ICU4N specific - de-nested Field class
    }

    /// <summary>
    /// The instances of this inner class are used as attribute keys and values
    /// in <see cref="AttributedCharacterIterator"/> that
    /// <c>NumberFormat.FormatToCharacterIterator()</c> method returns.
    /// <para/>
    /// There is no public constructor to this class, the only instances are the
    /// constants defined here.
    /// </summary>
    /// <stable>ICU 3.6</stable>
    internal class NumberFormatField : FormatField // ICU4N: Marked internal until implementation is completed // ICU4N specific - renamed from NumberFormat.Field
    {
        // generated by serialver from JDK 1.4.1_01
        //static readonly long serialVersionUID = -4516273749929385842L;

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Sign = new NumberFormatField("sign");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Integer = new NumberFormatField("integer");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Fraction = new NumberFormatField("fraction");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Exponent = new NumberFormatField("exponent");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField ExponentSign = new NumberFormatField("exponent sign");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField ExponentSymbol = new NumberFormatField("exponent symbol");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField DecimalSeparator = new NumberFormatField("decimal separator");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField GroupingSeparator = new NumberFormatField("grouping separator");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Percent = new NumberFormatField("percent");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField PerMille = new NumberFormatField("per mille");

        /// <stable>ICU 3.6</stable>
        public static readonly NumberFormatField Currency = new NumberFormatField("currency");

        /// <summary>
        /// Constructs a new instance of <see cref="NumberFormatField"/> with the given
        /// <paramref name="fieldName"/>.
        /// </summary>
        /// <stable>ICU 3.6</stable>
        protected NumberFormatField(string fieldName)
                        : base(fieldName)
        {
        }

        /**
         * serizalization method resolve instances to the constant
         * NumberFormat.Field values
         * @stable ICU 3.6
         */
        protected override object ReadResolve()
        {
            return null; // ICU4N TODO: Implementation
                         //if (this.getName().equals(INTEGER.getName()))
                         //    return INTEGER;
                         //if (this.getName().equals(FRACTION.getName()))
                         //    return FRACTION;
                         //if (this.getName().equals(EXPONENT.getName()))
                         //    return EXPONENT;
                         //if (this.getName().equals(EXPONENT_SIGN.getName()))
                         //    return EXPONENT_SIGN;
                         //if (this.getName().equals(EXPONENT_SYMBOL.getName()))
                         //    return EXPONENT_SYMBOL;
                         //if (this.getName().equals(CURRENCY.getName()))
                         //    return CURRENCY;
                         //if (this.getName().equals(DECIMAL_SEPARATOR.getName()))
                         //    return DECIMAL_SEPARATOR;
                         //if (this.getName().equals(GROUPING_SEPARATOR.getName()))
                         //    return GROUPING_SEPARATOR;
                         //if (this.getName().equals(PERCENT.getName()))
                         //    return PERCENT;
                         //if (this.getName().equals(PERMILLE.getName()))
                         //    return PERMILLE;
                         //if (this.getName().equals(SIGN.getName()))
                         //    return SIGN;

            //throw new InvalidObjectException("An invalid object.");
        }
    }

    /// <summary>
    /// A <see cref="NumberFormatFactory"/> is used to register new number formats.  The factory
    /// should be able to create any of the predefined formats for each locale it
    /// supports.  When registered, the locales it supports extend or override the
    /// locales already supported by ICU.
    /// <para/>
    /// <b>Note:</b> as of ICU4J 3.2, the default API for <see cref="NumberFormatFactory"/> uses
    /// <see cref="UCultureInfo"/> instead of <see cref="CultureInfo"/>. Instead of overriding <see cref="CreateFormat(CultureInfo, int)"/>,
    /// new implementations should override <see cref="CreateFormat(UCultureInfo, int)"/>. Note that
    /// one of these two methods <b>MUST</b> be overridden or else an infinite
    /// loop will occur.
    /// </summary>
    /// <stable>ICU 2.6</stable>
    internal abstract class NumberFormatFactory // ICU4N: Marked internal until implementation is completed
    {
        /// <summary>
        /// Value passed to format requesting a default number format.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int FormatNumber = (int)NumberFormatStyle.NumberStyle;

        /// <summary>
        /// Value passed to format requesting a currency format.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int FormatCurrency = (int)NumberFormatStyle.CurrencyStyle;

        /// <summary>
        /// Value passed to format requesting a percent format.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int FormatPercent = (int)NumberFormatStyle.PercentStyle;

        /// <summary>
        /// Value passed to format requesting a scientific format.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int FormatScientific = (int)NumberFormatStyle.ScientificStyle;

        /// <summary>
        /// Value passed to format requesting an integer format.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public const int FormatInteger = (int)NumberFormatStyle.IntegerStyle;

        /// <summary>
        /// Returns true if this factory is visible.  Default is true.
        /// If not visible, the locales supported by this factory will not
        /// be listed by getAvailableLocales.  This value must not change.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public virtual bool Visible => true;

        /// <summary>
        /// Returns an immutable collection of the locale names directly
        /// supported by this factory.
        /// </summary>
        /// <returns>the supported locale names.</returns>
        /// <stable>ICU 2.6</stable>
        public abstract ICollection<string> GetSupportedLocaleNames();

        /// <summary>
        /// Returns a number format of the appropriate type.  If the locale
        /// is not supported, return null.  If the locale is supported, but
        /// the type is not provided by this service, return null.  Otherwise
        /// return an appropriate instance of <see cref="NumberFormat"/>.
        /// <b>Note:</b> as of ICU4J 3.2, implementations should override
        /// this method instead of <see cref="CreateFormat(CultureInfo, int)"/>.
        /// </summary>
        /// <param name="loc">the locale for which to create the format</param>
        /// <param name="formatType">the type of format</param>
        /// <returns>The <see cref="NumberFormat"/>, or null.</returns>
        /// <stable>ICU 3.2</stable>
        public virtual NumberFormat CreateFormat(UCultureInfo loc, int formatType) // ICU4N TODO: API Change formatType to NumberFormatStyle?
        {
            return CreateFormat(loc.ToCultureInfo(), formatType);
        }

        /// <summary>
        /// Returns a number format of the appropriate type.  If the locale
        /// is not supported, return null.  If the locale is supported, but
        /// the type is not provided by this service, return null.  Otherwise
        /// return an appropriate instance of <see cref="NumberFormat"/>.
        /// <b>Note:</b> as of ICU4J 3.2, <see cref="CreateFormat(UCultureInfo, int)"/> should be
        /// overridden instead of this method.  This method is no longer
        /// abstract and delegates to that method.
        /// </summary>
        /// <param name="loc">the locale for which to create the format</param>
        /// <param name="formatType">the type of format</param>
        /// <returns>the NumberFormat, or null.</returns>
        /// <stable>ICU 2.6</stable>
        public virtual NumberFormat CreateFormat(CultureInfo loc, int formatType) // ICU4N TODO: API Change formatType to NumberFormatStyle?
        {
            return CreateFormat(loc.ToUCultureInfo(), formatType);
        }

        /// <stable>ICU 2.6</stable>
        protected NumberFormatFactory()
        {
        }
    }

    /// <summary>
    /// A <see cref="NumberFormatFactory"/> that supports a single locale.  It can be visible or invisible.
    /// </summary>
    /// <stable>ICU 2.6</stable>
    internal abstract class SimpleNumberFormatFactory : NumberFormatFactory // ICU4N: Marked internal until implementation is completed
    {
        internal readonly ICollection<string> localeNames;
        internal readonly bool visible;

        /// <summary>
        /// Constructs a <see cref="SimpleNumberFormatFactory"/> with the given locale.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public SimpleNumberFormatFactory(CultureInfo locale)
            : this(locale, true)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SimpleNumberFormatFactory"/> with the given locale and the
        /// visibility.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public SimpleNumberFormatFactory(CultureInfo locale, bool visible)
        {
            localeNames = new string[] { locale.ToUCultureInfo().Name };
            this.visible = visible;
        }

        /// <summary>
        /// Constructs a <see cref="SimpleNumberFormatFactory"/> with the given locale.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public SimpleNumberFormatFactory(UCultureInfo locale)
            : this(locale, true)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SimpleNumberFormatFactory"/> with the given locale and the
        /// visibility.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public SimpleNumberFormatFactory(UCultureInfo locale, bool visible)
        {
            //localeNames = Collections.singleton(locale.getBaseName());
            localeNames = new string[] { locale.Name };
            this.visible = visible;
        }

        /// <summary>
        /// Returns true if this factory is visible.  Default is true.
        /// If not visible, the locales supported by this factory will not
        /// be listed by getAvailableLocales.  This value must not change.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public override sealed bool Visible => visible;

        /// <summary>
        /// Returns an immutable collection of the locale names directly
        /// supported by this factory.
        /// </summary>
        /// <returns>the supported locale names.</returns>
        /// <stable>ICU 2.6</stable>
        public override sealed ICollection<string> GetSupportedLocaleNames()
        {
            return localeNames;
        }
    }
}
