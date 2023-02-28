using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Globalization;
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
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        enum NumberFormatStyle // ICU4N: Marked internal until implementation is completed
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

    /// <summary>
    /// Extensions for <see cref="NumberFormatStyle"/>.
    /// </summary>
    internal static class NumberFormatStyleExtensions
    {
        public static bool IsDefined(this NumberFormatStyle choice)
        {
            return choice >= NumberFormatStyle.NumberStyle && choice <= NumberFormatStyle.StandardCurrencyStyle;
        }
    }

    /// <summary>
    /// <see cref="NumberFormat"/> is the abstract base class for all number
    /// formats. This class provides the interface for formatting and parsing
    /// numbers. <see cref="NumberFormat"/> also provides methods for determining
    /// which locales have number formats, and what their names are.
    /// <para/>
    /// <see cref="NumberFormat"/> helps you to format and parse numbers for any locale.
    /// Your code can be completely independent of the locale conventions for
    /// decimal points, thousands-separators, or even the particular decimal
    /// digits used, or whether the number format is even decimal.
    /// </summary>
    /// <remarks>
    /// To format a number for <see cref="CultureInfo.CurrentCulture"/>, use one of the factory
    /// class methods:
    /// <code>
    /// myString = NumberFormat.GetInstance().Format(myNumber);
    /// </code>
    /// 
    /// If you are formatting multiple numbers, it is
    /// more efficient to get the format and use it multiple times so that
    /// the system doesn't have to fetch the information about the local
    /// language and country conventions multiple times.
    /// <code>
    /// NumberFormat nf = NumberFormat.GetInstance();
    /// for (int i = 0; i &lt; a.length; ++i)
    /// {
    ///     Console.WriteLine(nf.Format(myNumber[i]) + "; ");
    /// }
    /// </code>
    /// 
    /// To format a number for a different locale, specify it in the
    /// call to <see cref="GetInstance(CultureInfo)"/>.
    /// <code>
    /// NumberFormat nf = NumberFormat.GetInstance(new CultureInfo("fr"));
    /// </code>
    /// 
    /// You can also use a <see cref="NumberFormat"/> to parse numbers:
    /// <code>
    /// myNumber = nf.Parse(myString);
    /// </code>
    /// 
    /// Use overloads of <see cref="GetInstance()"/> or <see cref="GetNumberInstance()"/> to get the
    /// normal number format. Use overloads of <see cref="GetIntegerInstance()"/> to get an
    /// integer numbr format. Use overloads of <see cref="GetCurrencyInstance()"/> to get the
    /// currency number format. And use overloads of <see cref="GetPercentInstance()"/>
    /// format for displaying percentages. Some factory methods are found within
    /// subclasses of <see cref="NumberFormat"/>. With this format, a fraction like
    /// 0.53 is displayed as 53%.
    /// 
    /// <para/>
    /// Starting from ICU 4.2, you can use <see cref="GetInstance()"/> by passing in a <see cref="NumberFormatStyle"/>
    /// as parameter to get the correct instance.
    /// For example,
    /// use <see cref="NumberFormatStyle.NumberStyle"/> to get the normal number format,
    /// <see cref="NumberFormatStyle.PercentStyle"/> to get a format for displaying percentage,
    /// <see cref="NumberFormatStyle.ScientificStyle"/> to get a format for displaying scientific number,
    /// <see cref="NumberFormatStyle.IntegerStyle"/> to get an integer number format,
    /// <see cref="NumberFormatStyle.CurrencyStyle"/> to get the currency number format,
    /// in which the currency is represented by its symbol, for example, "$3.00".
    /// <see cref="NumberFormatStyle.ISOCurrencyStyle"/> to get the currency number format,
    /// in which the currency is represented by its ISO code, for example "USD3.00".
    /// <see cref="NumberFormatStyle.PluralCurrencyStyle"/> to get the currency number format,
    /// in which the currency is represented by its full name in plural format,
    /// for example, "3.00 US dollars" or "1.00 US dollar".
    /// 
    /// <para/>
    /// You can also control the display of numbers with such methods as
    /// <see cref="MinimumFractionDigits"/>.
    /// If you want even more control over the format or parsing,
    /// or want to give your users more control,
    /// you can try casting the <see cref="NumberFormat"/> you get from the factory methods
    /// to a <see cref="DecimalFormat"/>. This will work for the vast majority
    /// of locales; just remember to put it in a <c>try</c> block in case you
    /// encounter an unusual one.
    /// 
    /// <para/>
    /// <see cref="NumberFormat"/> is designed such that some controls
    /// work for formatting and others work for parsing. The following is
    /// the detailed description for each these control methods.
    /// <para/>
    /// <see cref="ParseIntegerOnly"/> : only affects parsing, e.g.
    /// if <c>true</c>,  "3456.78" -&gt; 3456 (and leaves the parse position just after '6')
    /// if <c>false</c>, "3456.78" -&gt; 3456.78 (and leaves the parse position just after '8')
    /// This is independent of formatting.  If you want to not show a decimal point
    /// where there might be no digits after the decimal point, set
    /// <see cref="DecimalFormat.DecimalSeparatorAlwaysShown"/> on <see cref="DecimalFormat"/>.
    /// <para/>
    /// You can also use forms of the <see cref="Parse(string, ParsePosition)"/> and <see cref="Format(long, StringBuffer, FieldPosition)"/>
    /// overloads with <see cref="ParsePosition"/> and <see cref="FieldPosition"/> to
    /// allow you to:
    /// <list type="bullet">
    ///     <item><decription>progressively parse through pieces of a string</decription></item>
    ///     <item><decription>align the decimal point and other areas</decription></item>
    /// </list>
    /// For example, you can align numbers in two ways:
    /// <list type="number">
    ///     <item><description>If you are using a monospaced font with spacing for alignment,
    ///       you can pass the <see cref="FieldPosition"/> in your Format() call, with
    ///       <c>field</c> = <see cref="IntegerField"/>. On output,
    ///       <see cref="FieldPosition.EndIndex"/> will be set to the offset between the
    ///       last character of the integer and the decimal. Add
    ///       (desiredSpaceCount - <see cref="FieldPosition.EndIndex"/>) spaces at the front of the string.
    ///     </description></item>
    ///     <item><description>If you are using proportional fonts,
    ///       instead of padding with spaces, measure the width
    ///       of the string in pixels from the start to <see cref="FieldPosition.EndIndex"/>.
    ///       Then move the pen by
    ///       (desiredPixelWidth - widthToAlignmentPoint) before drawing the text.
    ///       It also works where there is no decimal, but possibly additional
    ///       characters at the end, e.g., with parentheses in negative
    ///       numbers: "(12)" for -12.
    ///     </description></item>
    /// </list>
    /// 
    /// <h3>Synchronization</h3>
    /// <para/>
    /// Number formats are generally not synchronized. It is recommended to create
    /// separate format instances for each thread. If multiple threads access a format
    /// concurrently, it must be synchronized externally.
    /// 
    /// <h4>DecimalFormat</h4>
    /// <para/>
    /// <see cref="DecimalFormat"/> is the concrete implementation of <see cref="NumberFormat"/>, and the
    /// <see cref="NumberFormat"/> API is essentially an abstraction from DecimalFormat's API.
    /// Refer to <see cref="DecimalFormat"/> for more information about this API.
    /// </remarks>
    /// <seealso cref="DecimalFormat"/>
    /// <seealso cref="ChoiceFormat"/>
    /// <author>Mark Davis</author>
    /// <author>Helena Shih</author>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
    // ICU4N TODO: Update the docs above once FieldPosition and ParsePosition have been replaced with .NETified parameters.
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        abstract class NumberFormat : UFormat // ICU4N: Marked internal until implementation is completed
    {
        // ICU4N specific - moved constants to an enum named NumberFormatStyle

        /// <summary>
        /// Field constant used to construct a <see cref="FieldPosition"/> object. Signifies that
        /// the position of the integer part of a formatted number should be returned.
        /// </summary>
        /// <seealso cref="FieldPosition"/>
        /// <stable>ICU 2.0</stable>
        public const int IntegerField = 0;

        /// <summary>
        /// Field constant used to construct a <see cref="FieldPosition"/> object. Signifies that
        /// the position of the fraction part of a formatted number should be returned.
        /// </summary>
        /// <seealso cref="FieldPosition"/>
        /// <stable>ICU 2.0</stable>
        public const int FractionField = 1;

        /// <summary>
        /// Formats a number and appends the resulting text to the given <see cref="StringBuffer"/>.
        /// <icunote>Recognizes <see cref="Numerics.BigMath.BigInteger"/> and
        /// <see cref="Numerics.BigMath.BigDecimal"/> objects.</icunote>
        /// </summary>
        /// <param name="number">The object to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> to use to append the formatted number to.</param>
        /// <param name="pos">On input: an optional alignment field; On output: the offsets
        /// of the alignment field in the formatted text.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="number"/> is not a numeric type.</exception>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(object number,
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

        /// <summary>
        /// Parses text from a string to produce a number.
        /// </summary>
        /// <param name="source">The <see cref="string"/> to parse.</param>
        /// <param name="parsePosition">The position at which to start the parse.</param>
        /// <returns>The parsed number, or <c>null</c>.</returns>
        /// <stable>ICU 2.0</stable>
        public override sealed object ParseObject(string source,
                                        ParsePosition parsePosition)
        {
            return Parse(source, parsePosition);
        }

        /// <summary>
        /// Specialization of <see cref="Formatter.Format(object)"/>
        /// </summary>
        /// <seealso cref="Formatter.Format(object)"/>
        /// <stable>ICU 2.0</stable>
        public string Format(double number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// Specialization of <see cref="Formatter.Format(object)"/>
        /// </summary>
        /// <seealso cref="Formatter.Format(object)"/>
        /// <stable>ICU 2.0</stable>
        public string Format(long number)
        {
            StringBuffer buf = new StringBuffer(19);
            FieldPosition pos = new FieldPosition(0);
            Format(number, buf, pos);
            return buf.ToString();
        }

        /// <summary>
        /// <icu/> Convenience method to format a <see cref="System.Numerics.BigInteger"/>.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public string Format(System.Numerics.BigInteger number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// <icu/> Convenience method to format a <see cref="Numerics.BigMath.BigInteger"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            string Format(Numerics.BigMath.BigInteger number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// <icu/> Convenience method to format a <see cref="Numerics.BigMath.BigDecimal"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            string Format(Numerics.BigMath.BigDecimal number)
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// <icu/> Convenience method to format an ICU <see cref="Numerics.BigDecimal"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            string Format(Numerics.BigDecimal number) // ICU BigDecimal
        {
            return Format(number, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// <icu/> Convenience method to format a <see cref="CurrencyAmount"/>.
        /// </summary>
        /// <stable>ICU 3.0</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            string Format(CurrencyAmount currAmt)
        {
            return Format(currAmt, new StringBuffer(),
                          new FieldPosition(0)).ToString();
        }

        /// <summary>
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            abstract StringBuffer Format(double number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);

        /// <summary>
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            abstract StringBuffer Format(long number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);

        /// <summary>
        /// <icu/> Formats a <see cref="System.Numerics.BigInteger"/>. 
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// <draft>ICU 60.1</draft>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            virtual StringBuffer Format(System.Numerics.BigInteger number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos)
        {
            return Format(new Numerics.BigDecimal(number), toAppendTo, pos);
        }

        /// <summary>
        /// <icu/> Formats a <see cref="Numerics.BigMath.BigInteger"/>. 
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            abstract StringBuffer Format(Numerics.BigMath.BigInteger number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);


        /// <summary>
        /// <icu/> Formats a <see cref="Numerics.BigMath.BigDecimal"/>. 
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            abstract StringBuffer Format(Numerics.BigMath.BigDecimal number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos);

        /// <summary>
        /// <icu/> Formats an ICU <see cref="Numerics.BigDecimal"/>. 
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            abstract StringBuffer Format(Numerics.BigDecimal number,
                                            StringBuffer toAppendTo,
                                            FieldPosition pos); // ICU BigDecimal

        /// <summary>
        /// <icu/> Formats a <see cref="CurrencyAmount"/>. 
        /// Specialization of <see cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>.
        /// </summary>
        /// <seealso cref="Formatter.Format(object, StringBuffer, FieldPosition)"/>
        /// <stable>ICU 3.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            virtual StringBuffer Format(CurrencyAmount currAmt,
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
        /// <seealso cref="DecimalFormat.ParseToBigDecimal"/>
        /// <seealso cref="Formatter.ParseObject(string, ParsePosition)"/>
        /// <stable>ICU 2.0</stable>
        public abstract J2N.Numerics.Number Parse(string text, ParsePosition parsePosition);

        /// <summary>
        /// Parses text from the beginning of the given string to produce a number.
        /// The method might not use the entire text of the given string.
        /// </summary>
        /// <param name="text">A <see cref="string"/> whose beginning should be parsed.</param>
        /// <returns>A <see cref="J2N.Numerics.Number"/> parsed from the string.</returns>
        /// <exception cref="FormatException">The beginning of the specified string cannot be parsed.</exception>
        /// <seealso cref="Formatter.Format(object)"/>
        /// <stable>ICU 2.0</stable>
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

        /// <summary>
        /// Parses text from the given string as a <see cref="CurrencyAmount"/>. Unlike
        /// the <see cref="Parse(string, ParsePosition)"/> method, this method will attempt to parse a generic
        /// currency name, searching for a match of this object's locale's
        /// currency display names, or for a 3-letter ISO currency code.
        /// This method will fail if this format is not a currency format,
        /// that is, if it does not contain the currency pattern symbol
        /// (U+00A4) in its prefix or suffix.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="pos">Input-output position; on input, the position within
        /// text to match; must have 0 &lt;= pos.Index &lt; text.Length;
        /// on output, the position after the last matched character. If
        /// the parse fails, the position in unchanged upon output.
        /// </param>
        /// <returns>A <see cref="CurrencyAmount"/>, or <c>null</c> upon failure.</returns>
        /// <stable>ICU 49</stable>

#if FEATURE_FIELDPOSITION && FEATURE_PARSECURRENCY
        public
#else
        internal
#endif
            virtual CurrencyAmount ParseCurrency(string text, ParsePosition pos) // ICU4N - converted ICharSequence to string
        {
            ////CLOVER:OFF
            // Default implementation only -- subclasses should override
            J2N.Numerics.Number n = Parse(text.ToString(), pos);
#pragma warning disable CS0618 // Type or member is obsolete
            return n == null ? null : new CurrencyAmount(n, EffectiveCurrency);
#pragma warning restore CS0618 // Type or member is obsolete
            ////CLOVER:ON
        }

        /// <summary>
        /// Gets or sets whether this format will parse numbers as integers only
        /// (defaults to false). If a string contains a decimal point, parsing will stop before the decimal
        /// point. The decimal separator accepted
        /// by the parse operation is locale-dependent and determined by the
        /// subclass.
        /// 
        /// <para/>For example, in <em>en-US</em>, parsing the string "123.45" will return the number 123 and
        /// parse position 3. Parsing would stop at the "." character.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual bool ParseIntegerOnly
        {
            get => parseIntegerOnly;
            set => parseIntegerOnly = value;
        }

        /// <summary>
        /// <icu/> Gets or sets whether strict paring is in effect.
        /// <para/>
        /// When this is <c>true</c>, the string
        /// is required to be a stronger match to the pattern than when lenient parsing is in
        /// effect. More specifically, the following conditions cause a parse failure relative
        /// to lenient mode (examples use the pattern "#,##0.#"):
        /// 
        /// <list type="bullet">
        ///     <item><description>The presence and position of special symbols, including currency, must match the pattern.<br/>
        ///     '+123' fails (there is no plus sign in the pattern)</description></item>
        ///     <item><description>Leading or doubled grouping separators<br/>
        ///     ',123' and '1,,234' fail</description></item>
        ///     <item><description>Groups of incorrect length when grouping is used<br/>
        ///     '1,23' and '1234,567' fail, but '1234' passes</description></item>
        ///     <item><description>Grouping separators used in numbers followed by exponents<br/>
        ///     '1,234E5' fails, but '1234E5' and '1,234E' pass ('E' is not an exponent when not followed by a number)</description></item>
        /// </list>
        /// When strict parsing is off, all grouping separators are ignored.
        /// This is the default behavior.
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public virtual bool ParseStrict
        {
            get => parseStrict;
            set => parseStrict = value;
        }

        /// <summary>
        /// <icu/> Set a particular <see cref="DisplayContext"/> value in the formatter,
        /// such as <see cref="DisplayContext.CapitalizationForStandalone"/>.
        /// </summary>
        /// <param name="context">The <see cref="DisplayContext"/> value to set.</param>
        /// <stable>ICU 53</stable>
        public virtual void SetContext(DisplayContext context)
        {
            if (context.Type() == DisplayContextType.Capitalization)
            {
                capitalizationSetting = context;
            }
        }

        /// <summary>
        /// <icu/> Get the formatter's <see cref="DisplayContext"/> value for the specified <see cref="DisplayContextType"/>,
        /// such as <see cref="DisplayContextType.Capitalization"/>
        /// </summary>
        /// <param name="type">The <see cref="DisplayContextType"/> whose value to return.</param>
        /// <returns>The current <see cref="DisplayContext"/> setting for the specified type.</returns>
        /// <stable>ICU 53</stable>
        public virtual DisplayContext GetContext(DisplayContextType type)
        {
            // ICU4N note: capitalizationSetting not nullable.
            return (type == DisplayContextType.Capitalization /*&& capitalizationSetting != null*/) ?
                    capitalizationSetting : DisplayContext.CapitalizationNone;
        }


        //============== Locale Stuff =====================

        /// <summary>
        /// Returns the default number format for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// <para/>
        /// The default format is one of the styles provide by the other factory methods:
        /// <see cref="GetNumberInstance()"/>, <see cref="GetIntegerInstance()"/>, 
        /// <see cref="GetCurrencyInstance()"/> or <see cref="GetPercentInstance()"/>.
        /// Exactly which one is locale-dependent.
        /// </summary>
        /// <returns>The default number format for the <see cref="UCultureInfo.CurrentCulture"/>.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// Returns the default number format for the specified <paramref name="locale"/>.
        /// <para/>
        /// The default format is one of the styles provide by the other factory methods:
        /// <see cref="GetNumberInstance()"/>, <see cref="GetIntegerInstance()"/>, 
        /// <see cref="GetCurrencyInstance()"/> or <see cref="GetPercentInstance()"/>.
        /// Exactly which one is locale-dependent.
        /// </summary>
        /// <param name="locale">The locale to retrieve the format instance for.</param>
        /// <returns>The default number format for the specified <paramref name="locale"/>.</returns>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// <icu/> Returns the default number format for the specified <paramref name="locale"/>.
        /// <para/>
        /// The default format is one of the styles provide by the other factory methods:
        /// <see cref="GetNumberInstance()"/>, <see cref="GetIntegerInstance()"/>, 
        /// <see cref="GetCurrencyInstance()"/> or <see cref="GetPercentInstance()"/>.
        /// Exactly which one is locale-dependent.
        /// </summary>
        /// <param name="locale">The locale to retrieve the format instance for.</param>
        /// <returns>The default number format for the specified <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static NumberFormat GetInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// <icu/> Returns a specific style number format for <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="style">Number format style.</param>
        /// <returns>A specific style number format for <see cref="UCultureInfo.CurrentCulture"/>.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 4.2</stable>
        public static NumberFormat GetInstance(NumberFormatStyle style)
        {
            return GetInstance(UCultureInfo.CurrentCulture, style);
        }

        /// <summary>
        /// <icu/> Returns a specific style number format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The specific locale.</param>
        /// <param name="style">Number format style.</param>
        /// <returns>A specific style number format for the specified <paramref name="locale"/>.</returns>
        /// <stable>ICU 4.2</stable>
        public static NumberFormat GetInstance(CultureInfo locale, NumberFormatStyle style)
        {
            return GetInstance(locale.ToUCultureInfo(), style);
        }

        /// <summary>
        /// Returns a general-purpose number format for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>A general-purpose number format for the <see cref="UCultureInfo.CurrentCulture"/>.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetNumberInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// Returns a general-purpose number format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The specific locale.</param>
        /// <returns>A general-purpose number format for the specified <paramref name="locale"/>.</returns>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetNumberInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// <icu/> Returns a general-purpose number format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The specific locale.</param>
        /// <returns>A general-purpose number format for the specified <paramref name="locale"/>.</returns>
        /// <stable>ICU 3.2</stable>
        public static NumberFormat GetNumberInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.NumberStyle);
        }

        /// <summary>
        /// Returns an integer number format for the <see cref="UCultureInfo.CurrentCulture"/>. The
        /// returned number format is configured to round floating point numbers
        /// to the nearest integer using IEEE half-even rounding
        /// (see <see cref="Numerics.BigMath.RoundingMode.HalfEven"/>) for formatting,
        /// and to parse only the integer part of an input string (see <see cref="ParseIntegerOnly"/>).
        /// </summary>
        /// <returns>A number format for integer values.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetIntegerInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.IntegerStyle);
        }

        /// <summary>
        /// Returns an integer number format for the specified <paramref name="locale"/>. The
        /// returned number format is configured to round floating point numbers
        /// to the nearest integer using IEEE half-even rounding 
        /// (see <see cref="Numerics.BigMath.RoundingMode.HalfEven"/>) for formatting,
        /// and to parse only the integer part of an input string (see <see cref="ParseIntegerOnly"/>).
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for integer values.</returns>
        /// <stable>ICU 2.0</stable>
        //Bug 4408066 [Richard/GCL]
        public static NumberFormat GetIntegerInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.IntegerStyle);
        }

        /// <summary>
        /// <icu/> Returns an integer number format for the specified locale. The
        /// returned number format is configured to round floating point numbers
        /// to the nearest integer using IEEE half-even rounding
        /// (see <see cref="Numerics.BigMath.RoundingMode.HalfEven"/>) for formatting,
        /// and to parse only the integer part of an input string (see <see cref="ParseIntegerOnly"/>).
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for integer values.</returns>
        /// <stable>ICU 3.2</stable>
        public static NumberFormat GetIntegerInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.IntegerStyle);
        }

        /// <summary>
        /// Returns a currency format for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>A number format for currency.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            static NumberFormat GetCurrencyInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.CurrencyStyle);
        }

        /// <summary>
        /// Returns a currency format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for currency.</returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            static NumberFormat GetCurrencyInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.CurrencyStyle);
        }

        /// <summary>
        /// <icu/> Returns a currency format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for currency.</returns>
        /// <stable>ICU 3.2</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            static NumberFormat GetCurrencyInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.CurrencyStyle);
        }

        /// <summary>
        /// Returns a percentage format for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>A number format for percents.</returns>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetPercentInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.PercentStyle);
        }

        /// <summary>
        /// Returns a percentage format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for percents.</returns>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetPercentInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.PercentStyle);
        }

        /// <summary>
        /// Returns a percentage format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A number format for percents.</returns>
        /// <stable>ICU 3.2</stable>
        public static NumberFormat GetPercentInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.PercentStyle);
        }

        /// <summary>
        /// <icu/> Returns a scientific format for the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>A scientific number format.</returns>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetScientificInstance()
        {
            return GetInstance(UCultureInfo.CurrentCulture, NumberFormatStyle.ScientificStyle);
        }

        /// <summary>
        /// <icu/> Returns a scientific format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A scientific number format.</returns>
        /// <stable>ICU 2.0</stable>
        public static NumberFormat GetScientificInstance(CultureInfo locale)
        {
            return GetInstance(locale.ToUCultureInfo(), NumberFormatStyle.ScientificStyle);
        }

        /// <summary>
        /// <icu/> Returns a scientific format for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which a number format is needed.</param>
        /// <returns>A scientific number format.</returns>
        /// <stable>ICU 3.2</stable>
        public static NumberFormat GetScientificInstance(UCultureInfo locale)
        {
            return GetInstance(locale, NumberFormatStyle.ScientificStyle);
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

        /// <summary>
        /// Gets the list of <see cref="CultureInfo"/>s for which <see cref="NumberFormat"/>s are available.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <stable>ICU 2.0</stable>
        public static CultureInfo[] GetCultures(UCultureTypes types)
        {
            if (shim == null)
            {
                return ICUResourceBundle.GetCultures(types);
            }
            return GetShim().GetCultures(types);
        }

        /// <summary>
        /// <icu/> Gets the list of <see cref="CultureInfo"/>s for which <see cref="NumberFormat"/>s are available.
        /// </summary>
        /// <param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
        /// <returns>The list of available locales.</returns>
        /// <draft>ICU 3.2 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
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

        /// <summary>
        /// <icu/> Unregisters the factory or instance associated with this key (obtained from
        /// <see cref="RegisterFactory(NumberFormatFactory)"/>).
        /// </summary>
        /// <param name="registryKey">A key obtained from <see cref="RegisterFactory(NumberFormatFactory)"/>.</param>
        /// <returns><c>true</c> if the object was successfully unregistered.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="registryKey"/> is <c>null</c>.</exception>
        /// <stable>ICU 2.6</stable>
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

        /// <inheritdoc/>
        /// <stable>ICU 2.0</stable>
        public override int GetHashCode()
        {
            return maximumIntegerDigits * 37 + maxFractionDigits;
            // just enough fields for a reasonable distribution
        }

        /// <summary>
        /// Overrides <see cref="object.Equals(object?)"/>.
        /// <para/>
        /// Two <see cref="NumberFormat"/>s are equal if they are of the same type
        /// and the settings <see cref="IsGroupingUsed"/>, <see cref="ParseIntegerOnly"/>,
        /// <see cref="MaximumIntegerDigits"/>, etc. are equal.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns><c>true</c> if the <paramref name="obj"/> is equal to this.</returns>
        /// <stable>ICU 2.0</stable>
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

        /// <summary>
        /// Overrides Clone.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override object Clone()
        {
            NumberFormat other = (NumberFormat)base.Clone();
            return other;
        }

        /// <summary>
        /// Gets or sets whether grouping is used in this format. For example, in the
        /// en_US locale, with grouping on, the number 1234567 will be formatted
        /// as "1,234,567". The grouping separator as well as the size of each group
        /// is locale-dependent and is determined by subclasses of <see cref="NumberFormat"/>.
        /// Grouping affects both parsing and formatting.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual bool IsGroupingUsed
        {
            get => groupingUsed;
            set => groupingUsed = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of digits allowed in the integer portion of a
        /// number. The default value is 40, which subclasses can override.
        /// <para/>
        /// When formatting, if the number of digits exceeds this value, the highest-
        /// significance digits are truncated until the limit is reached, in accordance
        /// with UTS#35.
        /// <para/>
        /// This setting has no effect on parsing.
        /// <para/>
        /// Subclasses might enforce an upper limit to this value appropriate to the
        /// numeric type being formatted.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Setter <paramref name="value"/> is less than <see cref="MinimumIntegerDigits"/>.
        /// </exception>
        /// <stable>ICU 2.0</stable>
        public virtual int MaximumIntegerDigits
        {
            get => maximumIntegerDigits;
            set
            {
                // ICU4N specific - added guard clause instead of putting in "corrective" side effects
                if (value < MinimumIntegerDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MaxDigits, nameof(MaximumIntegerDigits), nameof(MinimumIntegerDigits)));

                maximumIntegerDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of digits allowed in the integer portion of a
        /// number. The default value is 1, which subclasses can override.
        /// <para/>
        /// When formatting, if this value is not reached, numbers are padded on the
        /// left with the locale-specific '0' character to ensure at least this
        /// number of integer digits.
        /// <para/>
        /// When parsing, this setting has no effect.
        /// <para/>
        /// Subclasses might enforce an upper limit to this value appropriate to
        /// the numeric type being formatted.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Setter <paramref name="value"/> is less than 0.
        /// <para/>
        /// -or-
        /// <para/>
        /// Setter <paramref name="value"/> is greater than <see cref="MaximumIntegerDigits"/>.
        /// </exception>
        /// <stable>ICU 2.0</stable>
        public virtual int MinimumIntegerDigits
        {
            get => minimumIntegerDigits;
            set
            {
                // ICU4N specific - added guard clauses instead of putting in "corrective" side effects
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
                if (value > MaximumIntegerDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MinDigits, nameof(MinimumIntegerDigits), nameof(MaximumIntegerDigits)));

                minimumIntegerDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of digits allowed in the fraction
        /// portion of a number. The default value is 3, which subclasses can override.
        /// <para/>
        /// When formatting, the exact behavior when this value is exceeded is subclass-specific.
        /// <para/>
        /// When parsing, this has no effect.
        /// <para/>
        /// The concrete subclass may enforce an upper limit to this value appropriate to the
        /// numeric type being formatted.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Setter <paramref name="value"/> is less than <see cref="MinimumFractionDigits"/>.
        /// </exception>
        /// <stable>ICU 2.0</stable>
        public virtual int MaximumFractionDigits
        {
            get => maximumFractionDigits;
            set
            {
                // ICU4N specific - added guard clause instead of putting in "corrective" side effects
                if (value < MinimumFractionDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MaxDigits, nameof(MaximumFractionDigits), nameof(MinimumFractionDigits)));

                maximumFractionDigits = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of digits allowed in the fraction portion of a
        /// number. The default value is 0, which subclasses can override.
        /// <para/>
        /// When formatting, if this value is not reached, numbers are padded on
        /// the right with the locale-specific '0' character to ensure at least
        /// this number of fraction digits.
        /// <para/>
        /// When parsing, this has no effect.
        /// <para/>
        /// Subclasses might enforce an upper limit to this value appropriate to the
        /// numeric type being formatted.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Setter <paramref name="value"/> is less than 0.
        /// <para/>
        /// -or-
        /// <para/>
        /// Setter <paramref name="value"/> is greater than <see cref="MaximumFractionDigits"/>.
        /// </exception>
        public virtual int MinimumFractionDigits
        {
            get => minimumFractionDigits;
            set
            {
                // ICU4N specific - added guard clauses instead of putting in "corrective" side effects
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
                if (value > MaximumFractionDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MinDigits, nameof(MinimumFractionDigits), nameof(MaximumFractionDigits)));

                minimumFractionDigits = value;
            }
        }

#nullable enable
        /// <summary>
        /// Gets or sets the <see cref="ICU4N.Util.Currency"/> object used to display currency
        /// amounts. This takes effect immediately, if this format is a
        /// currency format. If this format is not a currency format, then
        /// the currency object is used if and when this object becomes a
        /// currency format.
        /// <para/>
        /// May be <c>null</c> for some subclasses.
        /// </summary>
        /// <stable>ICU 2.6</stable>
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            virtual Currency? Currency
        {
            get => currency;
            set => currency = value;
        }

        /// <summary>
        /// Gets or sets the currency in effect for this formatter. Subclasses
        /// should override this method as needed. Unlike <see cref="Currency"/>,
        /// this property should never return <c>null</c>.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_CURRENCYFORMATTING
        public
#else
        internal
#endif
            virtual Currency EffectiveCurrency
        {
            get
            {
                Currency c = Currency;
                if (c is null)
                {
                    UCultureInfo uloc = ValidCulture;
                    if (uloc is null)
                    {
                        uloc = UCultureInfo.CurrentCulture;
                    }
                    c = Currency.GetInstance(uloc);
                }
                return c;
            }
        }
#nullable restore

        // ICU4N specific - no need to add properties that cannot be supported

        /// <summary>
        /// Gets or sets the <see cref="Numerics.BigMath.RoundingMode"/> used in this <see cref="NumberFormat"/>.
        /// The default implementation of this method in <see cref="NumberFormat"/> always
        /// throws <see cref="NotSupportedException"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown unless overridden.</exception>
        /// <stable>ICU 4.0</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            virtual Numerics.BigMath.RoundingMode RoundingMode
        {
            get => throw new NotSupportedException(string.Format(SR.NotSupported_MayOverride, nameof(RoundingMode)));
            set => throw new NotSupportedException(string.Format(SR.NotSupported_MayOverride, nameof(RoundingMode)));
        }

        /// <summary>
        /// Returns a specific style number format for a specific locale.
        /// </summary>
        /// <param name="desiredLocale">The specific locale.</param>
        /// <param name="choice">Number format style.</param>
        /// <returns>The new <see cref="NumberFormat"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="choice"/> is not defined in <see cref="NumberFormatStyle"/>.</exception>
        /// <stable>ICU 4.2</stable>
        public static NumberFormat GetInstance(UCultureInfo desiredLocale, NumberFormatStyle choice)
        {
            // ICU4N: Simplified validation.
            if (!choice.IsDefined())
                throw new ArgumentException(string.Format(SR.Arg_UndefinedEnumValue, choice, nameof(NumberFormatStyle)), nameof(choice));

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
                    if (nsRuleSetGroup.Equals("SpelloutRules", StringComparison.Ordinal))
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

        /// <summary>
        /// Returns the pattern for the provided <paramref name="locale"/> and <paramref name="choice"/>.
        /// </summary>
        /// <param name="locale">The locale of the data.</param>
        /// <param name="choice">The pattern format.</param>
        /// <returns>The pattern.</returns>
        [Obsolete("ICU 3.4 subclassers should override GetPattern(ULocale, int) instead of this method.")]
        internal static string GetPattern(CultureInfo locale, NumberFormatStyle choice) // ICU4N specific - made intenal instead of public
        {
            return GetPattern(locale.ToUCultureInfo(), choice);
        }

        /// <summary>
        /// Returns the pattern for the provided <paramref name="locale"/> and <paramref name="choice"/>.
        /// </summary>
        /// <param name="locale">The locale of the data.</param>
        /// <param name="choice">The pattern format.</param>
        /// <returns>The pattern.</returns>
        /// <stable>ICU 3.2</stable>
        protected internal static string GetPattern(UCultureInfo locale, NumberFormatStyle choice)
        {
#pragma warning disable 612, 618
            return GetPatternForStyle(locale, choice);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns the pattern for the provided <paramref name="locale"/> and <paramref name="choice"/>.
        /// </summary>
        /// <param name="locale">The locale of the data.</param>
        /// <param name="choice">The pattern format.</param>
        /// <returns>The pattern.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static string GetPatternForStyle(UCultureInfo locale, NumberFormatStyle choice) // ICU4N specific - made intenal instead of public
        {
            NumberingSystem ns = NumberingSystem.GetInstance(locale);
            string nsName = ns.Name;
            return GetPatternForStyleAndNumberingSystem(locale, nsName, choice);
        }

        /// <summary>
        /// Returns the pattern for the provided <paramref name="locale"/>, 
        /// numbering system, and <paramref name="choice"/>.
        /// </summary>
        /// <param name="locale">The locale of the data.</param>
        /// <param name="nsName">The name of the numbering system, like "latn".</param>
        /// <param name="choice">The pattern format.</param>
        /// <returns>The pattern.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static string GetPatternForStyleAndNumberingSystem(UCultureInfo locale, string nsName, NumberFormatStyle choice) // ICU4N specific - made intenal instead of public
        {
            /* for ISOCURRENCYSTYLE and PLURALCURRENCYSTYLE,
             * the pattern is the same as the pattern of CURRENCYSTYLE
             * but by replacing the single currency sign with
             * double currency sign or triple currency sign.
             */
            string patternKey;
            switch (choice)
            {
                case NumberFormatStyle.NumberStyle:
                case NumberFormatStyle.IntegerStyle:
                case NumberFormatStyle.PluralCurrencyStyle:
                    patternKey = "decimalFormat";
                    break;
                case NumberFormatStyle.CurrencyStyle:
                    patternKey = (locale.Keywords.TryGetValue("cf", out string cfKeyValue)
                        && cfKeyValue != null && cfKeyValue.Equals("account", StringComparison.Ordinal)) ?
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
                .GetBundleInstance(ICUData.IcuBaseName, locale);

            string result = rb.FindStringWithFallback(
                        "NumberElements/" + nsName + "/patterns/" + patternKey);
            if (result is null)
            {
                result = rb.GetStringWithFallback("NumberElements/latn/patterns/" + patternKey);
            }

            return result;
        }

        // ICU4N TODO: Serialization

        /////**
        //// * First, read in the default serializable data.
        //// *
        //// * Then, if <code>serialVersionOnStream</code> is less than 1, indicating that
        //// * the stream was written by JDK 1.1,
        //// * set the <code>int</code> fields such as <code>maximumIntegerDigits</code>
        //// * to be equal to the <code>byte</code> fields such as <code>maxIntegerDigits</code>,
        //// * since the <code>int</code> fields were not present in JDK 1.1.
        //// * Finally, set serialVersionOnStream back to the maximum allowed value so that
        //// * default serialization will work properly if this object is streamed out again.
        //// */
        ////private void ReadObject(Stream stream)
        ////{
        ////    //stream.defaultReadObject();
        ////    /////CLOVER:OFF
        ////    //// we don't have serialization data for this format
        ////    //if (serialVersionOnStream< 1) {
        ////    //    // Didn't have additional int fields, reassign to use them.
        ////    //    maximumIntegerDigits = maxIntegerDigits;
        ////    //    minimumIntegerDigits = minIntegerDigits;
        ////    //    maximumFractionDigits = maxFractionDigits;
        ////    //    minimumFractionDigits = minFractionDigits;
        ////    //}
        ////    //if (serialVersionOnStream< 2) {
        ////    //    // Didn't have capitalizationSetting, set it to default
        ////    //    capitalizationSetting = DisplayContext.CAPITALIZATION_NONE;
        ////    //}
        ////    /////CLOVER:ON
        ////    ////*Bug 4185761
        ////    //  Validate the min and max fields [Richard/GCL]
        ////    //*/
        ////    //if (minimumIntegerDigits > maximumIntegerDigits ||
        ////    //    minimumFractionDigits > maximumFractionDigits ||
        ////    //    minimumIntegerDigits< 0 || minimumFractionDigits< 0) {
        ////    //    throw new InvalidObjectException("Digit count range invalid");
        ////    //}
        ////    //serialVersionOnStream = currentSerialVersion;
        ////}

        /////**
        //// * Write out the default serializable data, after first setting
        //// * the <code>byte</code> fields such as <code>maxIntegerDigits</code> to be
        //// * equal to the <code>int</code> fields such as <code>maximumIntegerDigits</code>
        //// * (or to <code>Byte.MAX_VALUE</code>, whichever is smaller), for compatibility
        //// * with the JDK 1.1 version of the stream format.
        //// */
        ////private void WriteObject(Stream stream)
        ////{
        ////    //maxIntegerDigits = (maximumIntegerDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
        ////    //        (byte)maximumIntegerDigits;
        ////    //minIntegerDigits = (minimumIntegerDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
        ////    //        (byte)minimumIntegerDigits;
        ////    //maxFractionDigits = (maximumFractionDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
        ////    //        (byte)maximumFractionDigits;
        ////    //minFractionDigits = (minimumFractionDigits > Byte.MAX_VALUE) ? Byte.MAX_VALUE :
        ////    //        (byte)minimumFractionDigits;
        ////    //stream.defaultWriteObject();
        ////}

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

        /// <summary>
        /// True if the the grouping (i.e. thousands) separator is used when
        /// formatting and parsing numbers.
        /// </summary>
        /// <serial/>
        /// <seealso cref="IsGroupingUsed"/>
        private bool groupingUsed = true;

        /// <summary>
        /// The maximum number of digits allowed in the integer portion of a
        /// number. <see cref="maxIntegerDigits"/> must be greater than or equal to
        /// <see cref="minIntegerDigits"/>.
        /// <para/>
        /// <strong>Note:</strong> This field exists only for serialization
        /// compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        /// <see cref="int"/> field <see cref="maximumIntegerDigits"/> is used instead.
        /// When writing to a stream, <see cref="maxIntegerDigits"/> is set to
        /// <see cref="maximumIntegerDigits"/> or <see cref="byte.MaxValue"/>,
        /// whichever is smaller.  When reading from a stream, this field is used
        /// only if <see cref="serialVersionOnStream"/> is less than 1.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MaximumIntegerDigits"/>
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte maxIntegerDigits = 40; // ICU4N: Not used (for now)
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /// <summary>
        /// The minimum number of digits allowed in the integer portion of a
        /// number. <see cref="minimumIntegerDigits"/> must be less than or equal to
        /// <see cref="maximumIntegerDigits"/>.
        /// <para/>
        /// <strong>Note:</strong> This field exists only for serialization
        /// <see cref="int"/> field <see cref="minIntegerDigits"/> is set to
        /// <see cref="minimumIntegerDigits"/> or <see cref="byte.MaxValue"/>,
        /// whichever is smaller.  When reading from a stream, this field is used
        /// only if <see cref="serialVersionOnStream"/> is less than 1.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MinimumIntegerDigits"/>
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte minIntegerDigits = 1; // ICU4N: Not used (for now)
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /// <summary>
        /// The maximum number of digits allowed in the fractional portion of a
        /// number. <see cref="maximumFractionDigits"/> must be greater than or equal to
        /// see cref="minimumFractionDigits"/>.
        /// <para/>
        /// <strong>Note:</strong> This field exists only for serialization
        /// compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        /// <see cref="int"/> field <see cref="maximumFractionDigits"/> is used instead.
        /// When writing to a stream, <see cref="maxFractionDigits"/> is set to
        /// <see cref="maximumFractionDigits"/> or <see cref="byte.MaxValue"/>,
        /// whichever is smaller.  When reading from a stream, this field is used
        /// only if <see cref="serialVersionOnStream"/> is less than 1.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MaximumFractionDigits"/>
        private byte maxFractionDigits = 3;    // invariant, >= minFractionDigits

        /// <summary>
        /// The minimum number of digits allowed in the fractional portion of a
        /// number. <see cref="minimumFractionDigits"/> must be less than or equal to
        /// <see cref="maximumFractionDigits"/>.
        /// <para/>
        /// <strong>Note:</strong> This field exists only for serialization
        /// compatibility with JDK 1.1.  In JDK 1.2 and higher, the new
        /// <see cref="int"/> field <see cref="minimumFractionDigits"/> is used instead.
        /// When writing to a stream, <see cref="minFractionDigits"/> is set to
        /// <see cref="minimumFractionDigits"/> or <see cref="byte.MaxValue"/>,
        /// whichever is smaller.  When reading from a stream, this field is used
        /// only if <see cref="serialVersionOnStream"/> is less than 1.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MinimumFractionDigits"/>
#pragma warning disable CS0414, IDE0051 // Remove unused private members
        private byte minFractionDigits = 0;
#pragma warning restore CS0414, IDE0051 // Remove unused private members

        /// <summary>
        /// True if this format will parse numbers as integers only.
        /// </summary>
        /// <serial/>
        /// <seealso cref="ParseIntegerOnly"/>
        private bool parseIntegerOnly = false;

        // new fields for 1.2.  byte is too small for integer digits.

        /// <summary>
        /// The maximum number of digits allowed in the integer portion of a
        /// number. <see cref="maximumIntegerDigits"/> must be greater than or equal to
        /// <see cref="minimumIntegerDigits"/>
        /// </summary>
        /// <serial/>
        /// <seealso cref="MaximumIntegerDigits"/>
        private int maximumIntegerDigits = 40;

        /// <summary>
        /// The minimum number of digits allowed in the integer portion of a
        /// number. <see cref="minimumIntegerDigits"/> must be less than or equal to
        /// <see cref="maximumIntegerDigits"/>.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MinimumIntegerDigits"/>
        private int minimumIntegerDigits = 1;

        /// <summary>
        /// The maximum number of digits allowed in the fractional portion of a
        /// number. <see cref="maximumFractionDigits"/> must be greater than or equal to
        /// <see cref="minimumFractionDigits"/>.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MaximumFractionDigits"/>
        private int maximumFractionDigits = 3;    // invariant, >= minFractionDigits

        /// <summary>
        /// The minimum number of digits allowed in the fractional portion of a
        /// number. <see cref="minimumFractionDigits"/> must be less than or equal to
        /// <see cref="maximumFractionDigits"/>.
        /// </summary>
        /// <serial/>
        /// <seealso cref="MinimumFractionDigits"/>
        private int minimumFractionDigits = 0;

#nullable enable
        /// <summary>
        /// Currency object used to format currencies. Subclasses may
        /// ignore this if they are not currency formats. This will be
        /// <c>null</c> unless a subclass sets it to a non-null value.
        /// </summary>
        /// <since>ICU 2.6</since>
        private Currency? currency;
#nullable restore

        internal static readonly int currentSerialVersion = 2;

        /// <summary>
        /// Describes the version of <see cref="NumberFormat"/> present on the stream.
        /// Possible values are:
        /// <list type="bullet">
        ///     <item>
        ///         <term>0 (or uninitialized)</term>
        ///         <description>
        ///             the JDK 1.1 version of the stream format.
        ///             In this version, the <see cref="int"/> fields such as
        ///             <see cref="maximumIntegerDigits"/> were not present, and the <see cref="byte"/>
        ///             fields such as <see cref="maxIntegerDigits"/> are used instead.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description>
        ///             the JDK 1.2 version of the stream format.  The values of the
        ///             <see cref="byte"/> fields such as <see cref="maxIntegerDigits"/> are ignored,
        ///             and the <see cref="int"/> fields such as <see cref="maximumIntegerDigits"/>
        ///             are used instead.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>2</term>
        ///         <description>
        ///             adds capitalizationSetting.
        ///             
        ///         </description>
        ///     </item>
        /// </list>
        /// When streaming out a <see cref="NumberFormat"/>, the most recent format
        /// (corresponding to the highest allowable <see cref="serialVersionOnStream"/>)
        /// is always written.
        /// </summary>
        /// <serial/>
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

        internal static class SR
        {
            public const string Arg_UndefinedEnumValue = "'{0}' is not defined in '{1}'.";
            public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
            public const string ArgumentOutOfRange_MinDigits = "{0} must be less than or equal to {1}.";
            public const string ArgumentOutOfRange_MaxDigits = "{0} must be greater than or equal to {1}.";
            public const string NotSupported_MayOverride = "{0} is not supported, but may be implemented by a subclass.";
        }
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
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        abstract class NumberFormatFactory // ICU4N: Marked internal until implementation is completed
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
