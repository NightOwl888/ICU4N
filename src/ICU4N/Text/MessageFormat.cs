using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Collections.Generic.Extensions;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using JCG = J2N.Collections.Generic;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    // ICU4N TODO: Missing dependencies, DateFormat, DecimalFormat, RuleBasedNumberFormat, BigDecimal, others
    internal class DateFormat { } // ICU4N TODO: Remove when DateFormat is ported
    internal class DecimalFormat : NumberFormat // ICU4N TODO: Remove when DecimalFormat is ported
    {
        // ICU4N TODO: This class should have all synchronized properties to match ICU4J

        [NonSerialized]
        private volatile DecimalFormatSymbols symbols;

        //public DecimalFormat(string pattern)
        //{
        //    symbols = DefaultSymbols;
        //    //properties = new DecimalFormatProperties();
        //    //exportedProperties = new DecimalFormatProperties();
        //    //// Regression: ignore pattern rounding information if the pattern has currency symbols.
        //    //setPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
        //    //refreshFormatter();
        //}

        public DecimalFormat(string pattern, DecimalFormatSymbols symbols)
        {
            this.symbols = (DecimalFormatSymbols)symbols.Clone();
            //properties = new DecimalFormatProperties();
            //exportedProperties = new DecimalFormatProperties();
            //// Regression: ignore pattern rounding information if the pattern has currency symbols.
            //setPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            //refreshFormatter();
        }

        /// <summary>
        /// Internal constructor used by <see cref="NumberFormat"/>.
        /// </summary>
        internal DecimalFormat(string pattern, DecimalFormatSymbols symbols, NumberFormatStyle choice)
        {
            this.symbols = (DecimalFormatSymbols)symbols.Clone();
            //properties = new DecimalFormatProperties();
            //exportedProperties = new DecimalFormatProperties();
            //// If choice is a currency type, ignore the rounding information.
            //if (choice == CURRENCYSTYLE
            //    || choice == ISOCURRENCYSTYLE
            //    || choice == ACCOUNTINGCURRENCYSTYLE
            //    || choice == CASHCURRENCYSTYLE
            //    || choice == STANDARDCURRENCYSTYLE
            //    || choice == PLURALCURRENCYSTYLE)
            //{
            //    setPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_ALWAYS);
            //}
            //else
            //{
            //    setPropertiesFromPattern(pattern, PatternStringParser.IGNORE_ROUNDING_IF_CURRENCY);
            //}
            //refreshFormatter();
        }

        public bool ParseBigDecimal { get; set; }

        public bool DecimalSeparatorAlwaysShown { get; set; }

        /**
        * Sets the decimal format symbols used by this formatter. The formatter uses a copy of the
        * provided symbols.
        *
        * @param newSymbols desired DecimalFormatSymbols
        * @see DecimalFormatSymbols
        * @stable ICU 2.0
        */
        public /*synchronized*/ void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols)
        {
            symbols = (DecimalFormatSymbols)newSymbols.Clone();
            //refreshFormatter();
        }

        public /*synchronized*/ void ApplyPattern(string pattern)
        {
            // ICU4N TODO: Parse properties from pattern
        }

        public /*synchronized*/ string ToPattern()
        {
            return string.Empty; // ICU4N TODO: Finish implementation
        }

        public override J2N.Numerics.Number Parse(string text, ParsePosition parsePosition)
        {
            int startIndex = parsePosition.Index;

            // ICU4N TODO: Parse into long
            //throw new NotImplementedException();
            return Double.GetInstance(double.Parse(text.Substring(startIndex), NumberStyles.Integer, CultureInfo.InvariantCulture));
        }

        public override StringBuffer Format(long number, StringBuffer result, FieldPosition fieldPosition)
        {
            string formatted = number.ToString(CultureInfo.InvariantCulture);
            return result.Append(formatted);

            //int startIndex = fieldPosition.BeginIndex;
            //int length = fieldPosition.EndIndex - startIndex;
            //// ICU4N TODO: Format number
            ////throw new NotImplementedException();
            //char[] chars = new char[length];
            //string formatted = number.ToString(CultureInfo.InvariantCulture);
            //formatted.CopyTo(0, chars, 0, length);
            //return result.Insert(startIndex, chars, startIndex, length);
        }

        public override StringBuffer Format(double number, StringBuffer result, FieldPosition fieldPosition)
        {
            string formatted = number.ToString("#,##0.################", CultureInfo.InvariantCulture);
            return result.Append(formatted);

            //int startIndex = fieldPosition.BeginIndex;
            //int length = fieldPosition.EndIndex - startIndex;
            //// ICU4N TODO: Format number
            ////throw new NotImplementedException();
            //char[] chars = new char[length];
            //string formatted = number.ToString("#,##0.################", CultureInfo.InvariantCulture);
            //formatted.CopyTo(0, chars, 0, length);
            //return toAppendTo.Insert(startIndex, chars, startIndex, length);
        }

        public override StringBuffer Format(BigInteger number, StringBuffer result, FieldPosition fieldPosition)
        {
            string formatted = number.ToString(CultureInfo.InvariantCulture);
            return result.Append(formatted);

            //int startIndex = fieldPosition.BeginIndex;
            //int length = fieldPosition.EndIndex - startIndex;
            //// ICU4N TODO: Format number
            ////throw new NotImplementedException();
            //char[] chars = new char[length];
            //string formatted = number.ToString(CultureInfo.InvariantCulture);
            //formatted.CopyTo(0, chars, 0, length);
            //return toAppendTo.Insert(startIndex, chars, startIndex, length);
        }
    }

    /// <icuenhanced><see cref="MessageFormat"/></icuenhanced><icu>_usage_</icu>
    /// <summary>
    /// <see cref="MessageFormat"/> prepares strings for display to users,
    /// with optional arguments (variables/placeholders).
    /// The arguments can occur in any order, which is necessary for translation
    /// into languages with different grammars.
    /// <para/>
    /// A <see cref="MessageFormat"/> is constructed from a <em>pattern</em> string
    /// with arguments in {curly braces} which will be replaced by formatted values.
    /// </summary>
    /// <remarks>
    /// <see cref="MessageFormat"/> differs from the other <see cref="Formatter"/>
    /// classes in that you create a <see cref="MessageFormat"/> object with one
    /// of its constructors (not with a <c>GetInstance</c> style factory
    /// method). Factory methods aren't necessary because <see cref="MessageFormat"/>
    /// itself doesn't implement locale-specific behavior. Any locale-specific
    /// behavior is defined by the pattern that you provide and the
    /// subformats used for inserted arguments.
    /// <para/>
    /// Arguments can be named (using identifiers) or numbered (using small ASCII-digit integers).
    /// Some of the API methods work only with argument numbers and throw an exception
    /// if the pattern has named arguments (see <see cref="UsesNamedArguments"/>).
    /// <para/>
    /// An argument might not specify any format type. In this case,
    /// a numeric type value is formatted with a default (for the locale) <see cref="NumberFormat"/>,
    /// a <see cref="DateTime"/> value is formatted with a default (for the locale) <see cref="DateFormat"/>,
    /// and for any other value its <c>ToString()</c> value is used.
    /// <para/>
    /// An argument might specify a "simple" type for which the specified
    /// <see cref="Formatter"/> object is created, cached and used.
    /// <para/>
    /// An argument might have a "complex" type with nested MessageFormat sub-patterns.
    /// During formatting, one of these sub-messages is selected according to the argument value
    /// and recursively formatted.
    /// <para/>
    /// After construction, a custom <see cref="Formatter"/> object can be set for
    /// a top-level argument, overriding the default formatting and parsing behavior
    /// for that argument.
    /// However, custom formatting can be achieved more simply by writing
    /// a typeless argument in the pattern string
    /// and supplying it with a preformatted string value.
    /// <para/>
    /// When formatting, <see cref="NumberFormat"/> takes a collection of argument values
    /// and writes an output string.
    /// The argument values may be passed as an array
    /// (when the pattern contains only numbered arguments)
    /// or as a <see cref="IDictionary{String, Object}"/> (which works for both named and numbered arguments).
    /// <para/>
    /// Each argument is matched with one of the input values by array index or dictionary key
    /// and formatted according to its pattern specification
    /// (or using a custom <see cref="Formatter"/> object if one was set).
    /// A numbered pattern argument is matched with a map key that contains that number
    /// as an ASCII-decimal-digit string (without leading zero).
    /// 
    /// <h3><a name="patterns">Patterns and Their Interpretation</a></h3>
    /// 
    /// <see cref="MessageFormat"/> uses patterns of the following form:
    /// <code>
    /// message = messageText (argument messageText)*
    /// argument = noneArg | simpleArg | complexArg
    /// complexArg = choiceArg | pluralArg | selectArg | selectordinalArg
    /// 
    /// noneArg = '{' argNameOrNumber '}'
    /// simpleArg = '{' argNameOrNumber ',' argType [',' argStyle] '}'
    /// choiceArg = '{' argNameOrNumber ',' "choice" ',' choiceStyle '}'
    /// pluralArg = '{' argNameOrNumber ',' "plural" ',' pluralStyle '}'
    /// selectArg = '{' argNameOrNumber ',' "select" ',' selectStyle '}'
    /// selectordinalArg = '{' argNameOrNumber ',' "selectordinal" ',' pluralStyle '}'
    /// 
    /// choiceStyle: see <see cref="ChoiceFormat"/>
    /// pluralStyle: see <see cref="PluralFormat"/>
    /// selectStyle: see <see cref="SelectFormat"/>
    /// 
    /// argNameOrNumber = argName | argNumber
    /// argName = [^[[:Pattern_Syntax:][:Pattern_White_Space:]]]+
    /// argNumber = '0' | ('1'..'9' ('0'..'9')*)
    /// 
    /// argType = "number" | "date" | "time" | "spellout" | "ordinal" | "duration"
    /// argStyle = "short" | "medium" | "long" | "full" | "integer" | "currency" | "percent" | argStyleText
    /// </code>
    /// 
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             messageText can contain quoted literal strings including syntax characters.
    ///             A quoted literal string begins with an ASCII apostrophe and a syntax character
    ///             (usually a {curly brace}) and continues until the next single apostrophe.
    ///             A double ASCII apostrohpe inside or outside of a quoted string represents
    ///             one literal apostrophe.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Quotable syntax characters are the {curly braces} in all messageText parts,
    ///             plus the '#' sign in a messageText immediately inside a pluralStyle,
    ///             and the '|' symbol in a messageText immediately inside a choiceStyle.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             See also <see cref="MessagePattern.ApostropheMode"/>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             In argStyleText, every single ASCII apostrophe begins and ends quoted literal text,
    ///             and unquoted {curly braces} must occur in matched pairs.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// Recommendation: Use the real apostrophe (single quote) character \\u2019 for
    /// human-readable text, and use the ASCII apostrophe (\\u0027 ' )
    /// only in program syntax, like quoting in <see cref="MessagePattern"/>.
    /// See the annotations for U+0027 Apostrophe in The Unicode Standard.
    /// <para/>
    /// The <c>choice</c> argument type is deprecated.
    /// Use <c>plural</c> arguments for proper plural selection,
    /// and <c>select</c> arguments for simple selection among a fixed set of choices.
    /// <para/>
    /// The <c>argType</c> and <c>argStyle</c> values are used to create
    /// a <see cref="Formatter"/> instance for the format element. The following
    /// table shows how the values map to <see cref="Formatter"/> instances. Combinations not
    /// shown in the table are illegal. Any <c>argStyleText</c> must
    /// be a valid pattern string for the <see cref="Formatter"/> subclass used.
    /// 
    /// <list type="table">
    ///     <listheader>
    ///         <term>argType</term>
    ///         <term>argStyle</term>
    ///         <term>resulting <see cref="Formatter"/> object</term>
    ///     </listheader>
    ///     <item>
    ///         <term><i>(none)</i></term>
    ///         <term><i>(none)</i></term>
    ///         <term><c>null</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>number</c></term>
    ///         <term><i>(none)</i></term>
    ///         <term><c>NumberFormat.GetInstance(CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>number</c></term>
    ///         <term><c>integer</c></term>
    ///         <term><c>NumberFormat.GetIntegerInstance(CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>number</c></term>
    ///         <term><c>currency</c></term>
    ///         <term><c>NumberFormat.GetCurrencyInstance(CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>number</c></term>
    ///         <term><c>percent</c></term>
    ///         <term><c>NumberFormat.GetPercentInstance(CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>number</c></term>
    ///         <term><i>argStyleText</i></term>
    ///         <term><c>new DecimalFormat(argStyleText, new DecimalFormatSymbols(CultureInfo.CurrentCulture))</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><i>(none)</i></term>
    ///         <term><c>DateFormat.GetDateInstance(DateFormat.Default, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><c>short</c></term>
    ///         <term><c>DateFormat.GetDateInstance(DateFormat.Short, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><c>medium</c></term>
    ///         <term><c>DateFormat.GetDateInstance(DateFormat.Default, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><c>long</c></term>
    ///         <term><c>DateFormat.GetDateInstance(DateFormat.Long, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><c>full</c></term>
    ///         <term><c>DateFormat.GetDateInstance(DateFormat.Full, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>date</c></term>
    ///         <term><i>argStyleText</i></term>
    ///         <term><c>new SimpleDateFormat(argStyleText, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><i>(none)</i></term>
    ///         <term><c>DateFormat.GetTimeInstance(DateFormat.Default, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><c>short</c></term>
    ///         <term><c>DateFormat.GetTimeInstance(DateFormat.Short, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><c>medium</c></term>
    ///         <term><c>DateFormat.GetTimeInstance(DateFormat.Default, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><c>long</c></term>
    ///         <term><c>DateFormat.GetTimeInstance(DateFormat.Long, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><c>full</c></term>
    ///         <term><c>DateFormat.GetTimeInstance(DateFormat.Full, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>time</c></term>
    ///         <term><i>argStyleText</i></term>
    ///         <term><c>new SimpleDateFormat(argStyleText, CultureInfo.CurrentCulture)</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>spellout</c></term>
    ///         <term><i>argStyleText (optional)</i></term>
    ///         <term><c>new RuleBasedNumberFormat(CultureInfo.CurrentCulture, RuleBasedNumberFormat.SpellOut)<br/>
    ///                     .SetDefaultRuleset(argStyleText);</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>ordinal</c></term>
    ///         <term><i>argStyleText (optional)</i></term>
    ///         <term><c>new RuleBasedNumberFormat(CultureInfo.CurrentCulture, RuleBasedNumberFormat.Ordinal)<br/>
    ///                     .SetDefaultRuleset(argStyleText);</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>duration</c></term>
    ///         <term><i>argStyleText (optional)</i></term>
    ///         <term><c>new RuleBasedNumberFormat(CultureInfo.CurrentCulture, RuleBasedNumberFormat.Duration)<br/>
    ///                     .SetDefaultRuleset(argStyleText);</c></term>
    ///     </item>
    /// </list>
    /// 
    /// <h4><a name="diffsjdk">Differences from <c>java.text.MessageFormat</c></a></h4>
    /// 
    /// <para/>
    /// The ICU MessageFormat supports both named and numbered arguments,
    /// while the JDK MessageFormat only supports numbered arguments.
    /// Named arguments make patterns more readable.
    /// <para/>
    /// ICU implements a more user-friendly apostrophe quoting syntax.
    /// In message text, an apostrophe only begins quoting literal text
    /// if it immediately precedes a syntax character (mostly {curly braces}).<br/>
    /// In the JDK MessageFormat, an apostrophe always begins quoting,
    /// which requires common text like "don't" and "aujourd'hui"
    /// to be written with doubled apostrophes like "don''t" and "aujourd''hui".
    /// For more details see <see cref="MessagePattern.ApostropheMode"/>.
    /// <para/>
    /// ICU does not create a ChoiceFormat object for a choiceArg, pluralArg or selectArg
    /// but rather handles such arguments itself.
    /// The JDK MessageFormat does create and use a ChoiceFormat object
    /// (<c>new ChoiceFormat(argStyleText)</c>).
    /// The JDK does not support plural and select arguments at all.
    /// 
    /// <h4>Usage Information</h4>
    /// 
    /// <para/>
    /// Here are some examples of usage:
    /// <code>
    /// object[] arguments = {
    ///     7,
    ///     DateTime.Now,
    ///     "a disturbance in the Force"
    /// };
    /// 
    /// string result = MessageFormat.Format(
    ///     "At {1,time} on {1,date}, there was {2} on planet {0,number,integer}.",
    ///     arguments);
    /// 
    /// // output: At 12:30 PM on Jul 3, 2053, there was a disturbance in the Force on planet 7.
    /// </code>
    /// Typically, the message format will come from resources, and the
    /// arguments will be dynamically set at runtime.
    /// 
    /// <para/>Example 2:
    /// <code>
    /// object[] testArgs = { 3, "MyDisk" };
    /// 
    /// MessageFormat format = new MessageFormat(
    ///     "The disk \"{1}\" contains {0} file(s).");
    ///     
    /// Console.WriteLine(format.Format(testArgs));
    /// 
    /// // output, with different testArgs
    /// // output: The disk "MyDisk" contains 0 file(s).
    /// // output: The disk "MyDisk" contains 1 file(s).
    /// // output: The disk "MyDisk" contains 1,273 file(s).
    /// </code>
    /// 
    /// <para/>For messages that include plural forms, you can use a plural argument:
    /// <code>
    /// MessageFormat msgFmt = new MessageFormat(
    ///     "{num_files, plural, " +
    ///     "=0{There are no files on disk \"{disk_name}\".}" +
    ///     "=1{There is one file on disk \"{disk_name}\".}" +
    ///     "other{There are # files on disk \"{disk_name}\".}}",
    ///     CultureInfo.CurrentCulture);
    /// var args = new Dictionary&lt;string, object&gt;();
    /// args["num_files"] = 0;
    /// args["disk_name"] = "MyDisk";
    /// Console.WriteLine(msgFmt.Format(args));
    /// args["num_files"] = 3;
    /// Console.WriteLine(msgFmt.Format(args));
    /// 
    /// // output:
    /// // There are no files on disk "MyDisk".
    /// // There are 3 files on "MyDisk".
    /// </code>
    /// See <see cref="PluralFormat"/> and <see cref="PluralRules"/> for details.
    /// 
    /// <h4><a name="synchronization">Synchronization</a></h4>
    /// 
    /// <para/>
    /// MessageFormats are not synchronized.
    /// It is recommended to create separate format instances for each thread.
    /// If multiple threads access a format concurrently, it must be synchronized
    /// externally.
    /// </remarks>
    /// <seealso cref="CultureInfo"/>
    /// <seealso cref="Formatter"/>
    /// <seealso cref="NumberFormat"/>
    /// <seealso cref="DecimalFormat"/>
    /// <seealso cref="ChoiceFormat"/>
    /// <seealso cref="PluralFormat"/>
    /// <seealso cref="SelectFormat"/>
    /// <author>Mark Davis</author>
    /// <author>Markus Scherer</author>
    /// <stable>ICU 3.0</stable>
    /// 
    // ICU4N TODO: API - Rework this class to eliminate member dependency on CultureInfo/UCultureInfo and
    // set it up to pass locale through Format/Parse/ApplyPattern methods as parameter overloads like what is
    // done in .NET, and make the default behavior use the CultureInfo.CurrentCulture for overloads that do
    // not accept the parameter. This class has no culture specific behavior, so there is no reason to
    // hold onto a reference of CultureInfo/UCultureInfo
    // After doing so, we need to update the docs above. We should try to ensure that our MessageFormat
    // is actually thread safe, rather than going the original route.
    internal class MessageFormat : UFormat // ICU4N: Marked internal until implementation is completed
    {
        // Incremented by 1 for ICU 4.8's new format.
        //internal static readonly long serialVersionUID = 7136212545847378652L;

        /// <summary>
        /// Constructs a <see cref="MessageFormat"/> for the default <see cref="UCultureInfo.CurrentCulture"/> locale and the
        /// specified pattern.
        /// Sets the locale and calls <see cref="ApplyPattern(string)"/> with <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern for this message format.</param>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 3.0</stable>
        public MessageFormat(string pattern)
        {
            this.uCulure = UCultureInfo.CurrentCulture;
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Constructs a <see cref="MessageFormat"/> for the specified locale and
        /// pattern.
        /// Sets the locale and calls <see cref="ApplyPattern(string)"/> with <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern for this message format.</param>
        /// <param name="locale">The <see cref="CultureInfo"/> for this message format.</param>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <stable>ICU 3.0</stable>
        public MessageFormat(string pattern, CultureInfo locale) // ICU4N TODO: API - rework to pass in culture in transient methods instead of fields
            : this(pattern, locale.ToUCultureInfo())
        {
        }

        /// <summary>
        /// Constructs a <see cref="MessageFormat"/> for the specified locale and
        /// pattern.
        /// Sets the locale and calls <see cref="ApplyPattern(string)"/> with <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">The pattern for this message format.</param>
        /// <param name="locale">The <see cref="UCultureInfo"/> for this message format.</param>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <stable>ICU 3.2</stable>
        public MessageFormat(string pattern, UCultureInfo locale) // ICU4N TODO: API - In general, formatters in .NET should be unaware of the culture unless it is explictly passed to the Format() method. Need to rework this.
        {
            this.uCulure = locale;
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Sets the <see cref="CultureInfo"/> to be used for creating argument <see cref="Formatter"/> objects.
        /// This affects subsequent calls to the <see cref="ApplyPattern(string)"/>
        /// method as well as to the <c>Format</c> and
        /// <see cref="FormatToCharacterIterator(object)"/> methods.
        /// </summary>
        /// <param name="locale">The locale to be used when creating or comparing subformats.</param>
        /// <stable>ICU 3.0</stable>
        public virtual void SetCulture(CultureInfo locale) // ICU4N TODO: API - In general, formatters in .NET should be unaware of the culture unless it is explictly passed to the Format() method. Need to rework this.
        {
            SetCulture(locale.ToUCultureInfo());
        }

        /// <summary>
        /// Sets the <see cref="UCultureInfo"/> to be used for creating argument <see cref="Formatter"/> objects.
        /// This affects subsequent calls to the <see cref="ApplyPattern(string)"/>
        /// method as well as to the <c>Format</c> and
        /// <see cref="FormatToCharacterIterator(object)"/> methods.
        /// </summary>
        /// <param name="locale">The locale to be used when creating or comparing subformats.</param>
        /// <stable>ICU 3.2</stable>
        public virtual void SetCulture(UCultureInfo locale) // ICU4N TODO: API - rework to pass in culture in transient methods instead of fields
        {
            /* Save the pattern, and then reapply so that */
            /* we pick up any changes in locale specific */
            /* elements */
            string existingPattern = ToPattern();                       /*ibm.3550*/
            this.uCulure = locale;

            // ICU4N TODO: Stock formatters and providers
            //// Invalidate all stock formatters. They are no longer valid since
            //// the locale has changed.
            //stockDateFormatter = null;
            stockNumberFormatter = null;
            pluralProvider = null;
            ordinalProvider = null;
            ApplyPattern(existingPattern);                              /*ibm.3550*/
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> that's used when creating or comparing subformats.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public virtual CultureInfo Culture => uCulure.ToCultureInfo(); // ICU4N TODO: API - rework to pass in culture in transient methods instead of fields

        /// <summary>
        /// <icu/> Gets the <see cref="UCultureInfo"/> that's used when creating argument <see cref="Formatter"/> objects.
        /// It is the <see cref="UCultureInfo"/> used when creating or comparing subformats.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public virtual UCultureInfo UCulture // ICU4N TODO: API - rework to pass culture in transient methods instead of fields
            => uCulure;

        /// <summary>
        /// Sets the pattern used by this message format.
        /// Parses the pattern and caches <see cref="Formatter"/> objects for simple argument types.
        /// Patterns and their interpretation are specified in the
        /// </summary>
        /// <param name="pttrn">The pattern for this message format.</param>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual void ApplyPattern(string pttrn)
        {
            try
            {
                if (msgPattern == null)
                {
                    msgPattern = new MessagePattern(pttrn);
                }
                else
                {
                    msgPattern.Parse(pttrn);
                }
                // Cache the formats that are explicitly mentioned in the message pattern.
                CacheExplicitFormats();
            }
            catch
            {
                ResetPattern();
                throw;
            }
        }

        /// <summary>
        /// <icu/> Sets the ApostropheMode and the pattern used by this message format.
        /// Parses the pattern and caches Format objects for simple argument types.
        /// Patterns and their interpretation are specified in the documentation of
        /// <see cref="MessageFormat"/>.
        /// <para/>
        /// This method is best used only once on a given object to avoid confusion about the mode,
        /// and after constructing the object with an empty pattern string to minimize overhead.
        /// </summary>
        /// <param name="pattern">The pattern for this message format.</param>
        /// <param name="aposMode">The new <see cref="Text.ApostropheMode"/>.</param>
        /// <seealso cref="MessagePattern.ApostropheMode"/>
        /// <stable>ICU 4.8</stable>
        public virtual void ApplyPattern(string pattern, ApostropheMode aposMode)
        {
            if (msgPattern == null)
            {
                msgPattern = new MessagePattern(aposMode);
            }
            else if (aposMode != msgPattern.ApostropheMode)
            {
                msgPattern.ClearPatternAndSetApostropheMode(aposMode);
            }
            ApplyPattern(pattern);
        }

        /// <summary>
        /// <icu/> Gets this instance's <see cref="Text.ApostropheMode"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual ApostropheMode ApostropheMode
        {
            get
            {
                if (msgPattern == null)
                {
                    msgPattern = new MessagePattern();  // Sets the default mode.
                }
                return msgPattern.ApostropheMode;
            }
        }

        /// <summary>
        /// Returns the applied pattern string.
        /// </summary>
        /// <returns>The pattern string.</returns>
        /// <exception cref="InvalidOperationException">After custom <see cref="Formatter"/> objects have been set
        /// via <see cref="SetFormat(int, Formatter)"/> or similar APIs.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual string ToPattern()
        {
            // Return the original, applied pattern string, or else "".
            // Note: This does not take into account
            // - changes from setFormat() and similar methods, or
            // - normalization of apostrophes and arguments, for example,
            //   whether some date/time/number formatter was created via a pattern
            //   but is equivalent to the "medium" default format.
            if (customFormatArgStarts != null)
            {
                throw new InvalidOperationException(
                        "ToPattern() is not supported after custom Format objects " +
                        "have been set via SetFormat() or similar APIs");
            }
            if (msgPattern == null)
            {
                return "";
            }
            String originalPattern = msgPattern.PatternString;
            return originalPattern == null ? "" : originalPattern;
        }

        /// <summary>
        /// Returns the part index of the next <see cref="MessagePatternPartType.ArgStart"/> after 
        /// <paramref name="partIndex"/>, or -1 if there is none more.
        /// </summary>
        /// <param name="partIndex"><see cref="MessagePatternPart.Index"/> of the previous 
        /// <see cref="MessagePatternPartType.ArgStart"/> (initially 0).</param>
        private int NextTopLevelArgStart(int partIndex)
        {
            if (partIndex != 0)
            {
                partIndex = msgPattern.GetLimitPartIndex(partIndex);
            }
            for (; ; )
            {
                MessagePatternPartType type = msgPattern.GetPartType(++partIndex);
                if (type == MessagePatternPartType.ArgStart)
                {
                    return partIndex;
                }
                if (type == MessagePatternPartType.MsgLimit)
                {
                    return -1;
                }
            }
        }

        private bool ArgNameMatches(int partIndex, string argName, int argNumber)
        {
            MessagePatternPart part = msgPattern.GetPart(partIndex);
            return part.Type == MessagePatternPartType.ArgName ?
                msgPattern.PartSubstringMatches(part, argName) :
                part.Value == argNumber;  // ARG_NUMBER
        }

        private string GetArgName(int partIndex)
        {
            MessagePatternPart part = msgPattern.GetPart(partIndex);
            if (part.Type == MessagePatternPartType.ArgName)
            {
                return msgPattern.GetSubstring(part);
            }
            else
            {
                //return Integer.toString(part.Value);
                return ((int)part.Value).ToString();
            }
        }

        /// <summary>
        /// Sets the <see cref="Formatter"/> objects to use for the values passed into
        /// <c>Format</c> methods or returned from <c>Parse</c>
        /// methods. The indices of elements in <paramref name="newFormats"/>
        /// correspond to the argument indices used in the previously set
        /// pattern string.
        /// The order of formats in <paramref name="newFormats"/> thus corresponds to
        /// the order of elements in the <c>arguments</c> array passed
        /// to the <c>Format</c> methods or the result array returned
        /// by the <c>Parse</c> methods.
        /// <para/>
        /// If an argument index is used for more than one format element
        /// in the pattern string, then the corresponding new format is used
        /// for all such format elements. If an argument index is not used
        /// for any format element in the pattern string, then the
        /// corresponding new format is ignored. If fewer formats are provided
        /// than needed, then only the formats for argument indices less
        /// than <c>newFormats.Length</c> are replaced.
        /// <para/>
        /// This method is only supported if the format does not use
        /// named arguments, otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="newFormats">The new formats to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormats"/> is null.</exception>
        /// <exception cref="ArgumentException">If this formatter uses named arguments.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual void SetFormatsByArgumentIndex(Formatter[] newFormats)
        {
            if (newFormats == null)
                throw new ArgumentNullException(nameof(newFormats)); // ICU4N specific - don't fall back on NullReferenceException

            if (msgPattern.HasNamedArguments)
            {
                throw new ArgumentException(
                        "This method is not available in MessageFormat objects " +
                        "that use alphanumeric argument names.");
            }
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                int argNumber = msgPattern.GetPart(partIndex + 1).Value;
                if (argNumber < newFormats.Length)
                {
                    SetCustomArgStartFormat(partIndex, newFormats[argNumber]);
                }
            }
        }

        /// <summary>
        /// <icu/> Sets the <see cref="Formatter"/> objects to use for the values passed into
        /// <c>Format</c> methods or returned from <c>Parse</c>
        /// methods. The keys in <paramref name="newFormats"/> are the argument
        /// names in the previously set pattern string, and the values
        /// are the formats.
        /// <para/>
        /// Only argument names from the pattern string are considered.
        /// Extra keys in <paramref name="newFormats"/> that do not correspond
        /// to an argument name are ignored.  Similarly, if there is no
        /// format in <paramref name="newFormats"/> for an argument name, the formatter
        /// for that argument remains unchanged.
        /// <para/>
        /// This may be called on formats that do not use named arguments.
        /// In this case the map will be queried for key strings that
        /// represent argument indices, e.g. "0", "1", "2" etc.
        /// </summary>
        /// <param name="newFormats">A <see cref="IDictionary{String, Object}"/> from <see cref="string"/> to 
        /// <see cref="Formatter"/> providing new formats for named arguments.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormats"/> is null.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual void SetFormatsByArgumentName(IDictionary<string, Formatter> newFormats)
        {
            if (newFormats == null)
                throw new ArgumentNullException(nameof(newFormats)); // ICU4N specific - don't fall back on NullReferenceException

            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                string key = GetArgName(partIndex + 1);
                if (newFormats.ContainsKey(key))
                {
                    SetCustomArgStartFormat(partIndex, newFormats[key]);
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Formatter"/> objects to use for the format elements in the
        /// previously set pattern string.
        /// The order of formats in <paramref name="newFormats"/> corresponds to
        /// the order of format elements in the pattern string.
        /// <para/>
        /// If more formats are provided than needed by the pattern string,
        /// the remaining ones are ignored. If fewer formats are provided
        /// than needed, then only the first <c>newFormats.Length</c>
        /// formats are replaced.
        /// <para/>
        /// Since the order of format elements in a pattern string often
        /// changes during localization, it is generally better to use the
        /// <see cref="SetFormatByArgumentIndex(int, Formatter)"/>
        /// method, which assumes an order of formats corresponding to the
        /// order of elements in the <c>arguments</c> array passed to
        /// the <c>Format</c> methods or the result array returned by
        /// the <c>Parse</c> methods.
        /// </summary>
        /// <param name="newFormats">The new formats to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormats"/> is null.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual void SetFormats(Formatter[] newFormats)
        {
            if (newFormats == null)
                throw new ArgumentNullException(nameof(newFormats)); // ICU4N specific - don't fall back on NullReferenceException

            int formatNumber = 0;
            for (int partIndex = 0;
                    formatNumber < newFormats.Length &&
                    (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                SetCustomArgStartFormat(partIndex, newFormats[formatNumber]);
                ++formatNumber;
            }
        }

        /// <summary>
        /// Sets the Format object to use for the format elements within the
        /// previously set pattern string that use the given argument
        /// index.
        /// The <paramref name="argumentIndex"/> is part of the format element definition and
        /// represents an index into the <c>arguments</c> array passed
        /// to the <c>Format</c> methods or the result array returned
        /// by the <c>Parse</c> methods.
        /// <para/>
        /// If the argument index is used for more than one format element
        /// in the pattern string, then the new format is used for all such
        /// format elements. If the argument index is not used for any format
        /// element in the pattern string, then the new format is ignored.
        /// <para/>
        /// This method is only supported when exclusively numbers are used for
        /// argument names. Otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="argumentIndex">The argument index for which to use the new format.</param>
        /// <param name="newFormat">The new format to use.</param>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormat"/> is null.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual void SetFormatByArgumentIndex(int argumentIndex, Formatter newFormat)
        {
            if (newFormat == null)
                throw new ArgumentNullException(nameof(newFormat)); // ICU4N specific - don't fall back on NullReferenceException

            if (msgPattern.HasNamedArguments)
            {
                throw new ArgumentException(
                        "This method is not available in MessageFormat objects " +
                        "that use alphanumeric argument names.");
            }
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                if (msgPattern.GetPart(partIndex + 1).Value == argumentIndex)
                {
                    SetCustomArgStartFormat(partIndex, newFormat);
                }
            }
        }

        /// <summary>
        /// <icu/> Sets the <see cref="Formatter"/> object to use for the format elements within the
        /// previously set pattern string that use the given argument
        /// name.
        /// <para/>
        /// If the argument name is used for more than one format element
        /// in the pattern string, then the new format is used for all such
        /// format elements. If the argument name is not used for any format
        /// element in the pattern string, then the new format is ignored.
        /// <para/>
        /// This API may be used on formats that do not use named arguments.
        /// In this case <paramref name="argumentName"/> should be a string that names
        /// an argument index, e.g. "0", "1", "2"... etc.  If it does not name
        /// a valid index, the format will be ignored.  No error is thrown.
        /// </summary>
        /// <param name="argumentName">The name of the argument to change.</param>
        /// <param name="newFormat">The new format to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormat"/> is null.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual void SetFormatByArgumentName(string argumentName, Formatter newFormat)
        {
            if (newFormat == null)
                throw new ArgumentNullException(nameof(newFormat)); // ICU4N specific - don't fall back on NullReferenceException

            int argNumber = MessagePattern.ValidateArgumentName(argumentName);
            if (argNumber < MessagePattern.ArgNameNotNumber)
            {
                return;
            }
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                if (ArgNameMatches(partIndex + 1, argumentName, argNumber))
                {
                    SetCustomArgStartFormat(partIndex, newFormat);
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Formatter"/> object to use for the format element with the given
        /// format element index within the previously set pattern string.
        /// The format element index is the zero-based number of the format
        /// element counting from the start of the pattern string.
        /// <para/>
        /// Since the order of format elements in a pattern string often
        /// changes during localization, it is generally better to use the
        /// <see cref="SetFormatByArgumentIndex(int, Formatter)"/>
        /// method, which accesses format elements based on the argument
        /// index they specify.
        /// </summary>
        /// <param name="formatElementIndex">The index of a format element within the pattern.</param>
        /// <param name="newFormat">The format to use for the specified format element.</param>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="formatElementIndex"/> is equal to or
        /// larger than the number of format elements in the pattern string.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="newFormat"/> is null.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual void SetFormat(int formatElementIndex, Formatter newFormat)
        {
            if (newFormat == null)
                throw new ArgumentNullException(nameof(newFormat)); // ICU4N specific - don't fall back on NullReferenceException

            int formatNumber = 0;
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                if (formatNumber == formatElementIndex)
                {
                    SetCustomArgStartFormat(partIndex, newFormat);
                    return;
                }
                ++formatNumber;
            }
            throw new IndexOutOfRangeException(formatElementIndex.ToString());
        }

        /// <summary>
        /// Returns the <see cref="Formatter"/> objects used for the values passed into
        /// <c>Format</c> methods or returned from <c>Parse</c>
        /// methods. The indices of elements in the returned array
        /// correspond to the argument indices used in the previously set
        /// pattern string.
        /// The order of formats in the returned array thus corresponds to
        /// the order of elements in the <c>arguments</c> array passed
        /// to the <c>Format</c> methods or the result array returned
        /// by the <c>Parse</c> methods.
        /// <para/>
        /// If an argument index is used for more than one format element
        /// in the pattern string, then the format used for the last such
        /// format element is returned in the array. If an argument index
        /// is not used for any format element in the pattern string, then
        /// null is returned in the array.
        /// <para/>
        /// This method is only supported when exclusively numbers are used for
        /// argument names. Otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <returns>The formats used for the arguments within the pattern.</returns>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual Formatter[] GetFormatsByArgumentIndex()
        {
            if (msgPattern.HasNamedArguments)
            {
                throw new ArgumentException(
                        "This method is not available in MessageFormat objects " +
                        "that use alphanumeric argument names.");
            }
            IList<Formatter> list = new List<Formatter>();
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                int argNumber = msgPattern.GetPart(partIndex + 1).Value;
                while (argNumber >= list.Count)
                {
                    list.Add(null);
                }
                list[argNumber] = cachedFormatters == null ? null : cachedFormatters[partIndex];
            }
            return list.ToArray();
        }

        /// <summary>
        /// Returns the <see cref="Formatter"/> objects used for the format elements in the
        /// previously set pattern string.
        /// The order of formats in the returned array corresponds to
        /// the order of format elements in the pattern string.
        /// <para/>
        /// Since the order of format elements in a pattern string often
        /// changes during localization, it's generally better to use the
        /// <see cref="GetFormatsByArgumentIndex()"/>
        /// method, which assumes an order of formats corresponding to the
        /// order of elements in the <c>arguments</c> array passed to
        /// the <c>Format</c> methods or the result array returned by
        /// the <c>Parse</c> methods.
        /// <para/>
        /// This method is only supported when exclusively numbers are used for
        /// argument names. Otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <returns>The formats used for the format elements in the pattern.</returns>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual Formatter[] GetFormats()
        {
            IList<Formatter> list = new List<Formatter>();
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                list.Add(cachedFormatters == null ? null : cachedFormatters[partIndex]);
            }
            return list.ToArray();
        }

        /// <summary>
        /// <icu/> Returns the top-level argument names. For more details, see
        /// <see cref="SetFormatByArgumentName(string, Formatter)"/>.
        /// </summary>
        /// <returns>A <see cref="ICollection{String}"/> of argument names.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual ICollection<string> GetArgumentNames() // ICU4N specific - changed from ISet to ICollection
        {
            ICollection<string> result = new JCG.HashSet<string>();
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                result.Add(GetArgName(partIndex + 1));
            }
            return result;
        }

        /// <summary>
        /// <icu/> Returns the first top-level format associated with the given <paramref name="argumentName"/>.
        /// For more details, see <see cref="SetFormatByArgumentName(string, Formatter)"/>.
        /// </summary>
        /// <param name="argumentName">The name of the desired argument.</param>
        /// <returns>The <see cref="Formatter"/> associated with the name, or null if there isn't one.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual Formatter GetFormatByArgumentName(string argumentName)
        {
            if (cachedFormatters == null)
            {
                return null;
            }
            int argNumber = MessagePattern.ValidateArgumentName(argumentName);
            if (argNumber < MessagePattern.ArgNameNotNumber)
            {
                return null;
            }
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                if (ArgNameMatches(partIndex + 1, argumentName, argNumber))
                {
                    return cachedFormatters[partIndex];
                }
            }
            return null;
        }

        /// <summary>
        /// Formats an array of objects and appends the <see cref="MessageFormat"/>'s
        /// pattern, with arguments replaced by the formatted objects, to the
        /// provided <see cref="StringBuffer"/>.
        /// <para/>
        /// The text substituted for the individual format elements is derived from
        /// the current subformat of the format element and the
        /// <paramref name="arguments"/> element at the format element's argument index
        /// as indicated by the first matching line of the following table. An
        /// argument is <i>unavailable</i> if <paramref name="arguments"/> is
        /// <c>null</c> or has fewer than argumentIndex+1 elements.  When
        /// an argument is unavailable no substitution is performed.
        /// <list type="table">
        ///     <listheader>
        ///         <term>argType or Format</term>
        ///         <term>value object</term>
        ///         <term>Formatted Text</term>
        ///     </listheader>
        ///     <item>
        ///         <term><i>any</i></term>
        ///         <term><i>unavailable</i></term>
        ///         <term><c>"{" + argNameOrNumber + "}"</c></term>
        ///     </item>
        ///     <item>
        ///         <term><i>any</i></term>
        ///         <term><c>null</c></term>
        ///         <term><c>"null"</c></term>
        ///     </item>
        ///     <item>
        ///         <term>custom Formatter <c>!= null</c></term>
        ///         <term><i>any</i></term>
        ///         <term><c>customFormat.Format(argument)</c></term>
        ///     </item>
        ///     <item>
        ///         <term>noneArg, or custom Formatter <c>== null</c></term>
        ///         <term>is numeric type (<see cref="double"/>, <see cref="long"/>, etc)</term>
        ///         <term><c>NumberFormat.GetInstance(CultureInfo.CurrentCulture).Format(argument)</c></term>
        ///     </item>
        ///     <item>
        ///         <term>noneArg, or custom Formatter <c>== null</c></term>
        ///         <term><c>is DateTime</c></term>
        ///         <term><c>DateFormat.GetDateTimeInstance(DateFormat.Short, DateFormat.Short, 
        ///                     CultureInfo.CurrentCulture).Format(argument)</c></term>
        ///     </item>
        ///     <item>
        ///         <term>noneArg, or custom Formatter <c>== null</c></term>
        ///         <term><c>is string</c></term>
        ///         <term><c>argument</c></term>
        ///     </item>
        ///     <item>
        ///         <term>noneArg, or custom Formatter <c>== null</c></term>
        ///         <term><i>any</i></term>
        ///         <term><c>argument.ToString()</c></term>
        ///     </item>
        ///     <item>
        ///         <term>complexArg</term>
        ///         <term><i>any</i></term>
        ///         <term>result of recursive formatting of a selected sub-message</term>
        ///     </item>
        /// </list>
        /// <para/>
        /// If <paramref name="pos"/> is non-null, and refers to
        /// <see cref="MessageFormatField.Argument"/>, the location of the first formatted
        /// string will be returned.
        /// <para/>
        /// This method is only supported when the format does not use named
        /// arguments, otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="arguments">An array of objects to be formatted and substituted.</param>
        /// <param name="result">Where text is appended.</param>
        /// <param name="pos">On input: an alignment field, if desired.
        /// On output: the offsets of the alignment field.</param>
        /// <exception cref="ArgumentException">If a value in the
        /// <paramref name="arguments"/> array is not of the type
        /// expected by the corresponding argument or custom <see cref="Formatter"/> object.</exception>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="pos"/> argument is null.</exception>
        /// <stable>ICU 3.0</stable>
        public StringBuffer Format(object[] arguments, StringBuffer result,
                                         FieldPosition pos)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            Format(arguments, null, new AppendableWrapper(result), pos);
            return result;
        }

        /// <summary>
        /// Formats a dictionary of objects and appends the <see cref="MessageFormat"/>'s
        /// pattern, with <paramref name="arguments"/> replaced by the formatted objects, to the
        /// provided <see cref="StringBuffer"/>.
        /// <para/>
        /// The text substituted for the individual format elements is derived from
        /// the current subformat of the format element and the
        /// <paramref name="arguments"/> value corresopnding to the format element's
        /// argument name.
        /// <para/>
        /// A numbered pattern argument is matched with a map key that contains that number
        /// as an ASCII-decimal-digit string (without leading zero).
        /// <para/>
        /// An argument is <i>unavailable</i> if <paramref name="arguments"/> is
        /// <c>null</c> or does not have a value corresponding to an argument
        /// name in the pattern.  When an argument is unavailable no substitution
        /// is performed.
        /// </summary>
        /// <param name="arguments">A <see cref="IDictionary{String, Object}"/> to be formatted and substituted.</param>
        /// <param name="result">Where text is appended.</param>
        /// <param name="pos">On input: an alignment field, if desired.
        /// On output: the offsets of the alignment field.</param>
        /// <exception cref="ArgumentException">If a value in the
        /// <paramref name="arguments"/> array is not of the type
        /// expected by the corresponding argument or custom <see cref="Formatter"/> object.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="pos"/> argument is null.</exception>
        /// <returns>The passed-in <see cref="StringBuffer"/>.</returns>
        /// <stable>ICU 3.8</stable>
        public StringBuffer Format(IDictionary<string, object> arguments, StringBuffer result,
                                         FieldPosition pos)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            Format(null, arguments, new AppendableWrapper(result), pos);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="MessageFormat"/> with the given <paramref name="pattern"/> and uses it
        /// to format the given <paramref name="arguments"/>. This is equivalent to
        /// <code>
        /// Format(arguments, new StringBuilder(), null).ToString()
        /// </code>
        /// </summary>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <exception cref="ArgumentException">If a value in the
        /// <paramref name="arguments"/> array is not of the type
        /// expected by the corresponding argument or custom <see cref="Formatter"/> object.</exception>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <stable>ICU 3.0</stable>
        public static string Format(string pattern, params object[] arguments)
        {
            MessageFormat temp = new MessageFormat(pattern);
            return temp.Format(arguments);
        }

        /// <summary>
        /// Creates a <see cref="MessageFormat"/> with the given pattern and uses it to
        /// format the given arguments.  The pattern must identify arguments
        /// by name instead of by number.
        /// </summary>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        /// <exception cref="ArgumentException">If a value in the
        /// <paramref name="arguments"/> array is not of the type
        /// expected by the corresponding argument or custom <see cref="Formatter"/> object.</exception>
        /// <seealso cref="Format(IDictionary{string, object}, StringBuffer, FieldPosition)"/>
        /// <seealso cref="Format(string, object[])"/>
        /// <stable>ICU 3.8</stable>
        public static string Format(string pattern, IDictionary<string, object> arguments)
        {
            MessageFormat temp = new MessageFormat(pattern);
            return temp.Format(arguments);
        }

        /// <summary>
        /// <icu/> Returns true if this <see cref="MessageFormat"/> uses named arguments,
        /// and false otherwise. See <see cref="MessageFormat"/>.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual bool UsesNamedArguments => msgPattern.HasNamedArguments;

        // Overrides

        /// <summary>
        /// Formats a <see cref="IDictionary{String, Object}"/> or array of objects and appends the <see cref="MessageFormat"/>'s
        /// pattern, with format elements replaced by the formatted objects, to the
        /// provided <see cref="StringBuffer"/>.
        /// <para/>
        /// This is equivalent to either of <see cref="Format(object[], StringBuffer, FieldPosition)"/>
        /// or <see cref="Format(IDictionary{string, object}, StringBuffer, FieldPosition)"/>.
        /// A <see cref="IDictionary{String, Object}"/> must be provided if this format uses named arguments, otherwise
        /// an <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <param name="arguments">A <see cref="IDictionary{String, Object}"/> or array of <see cref="object"/>s to be formatted.</param>
        /// <param name="result">Where text is appended/</param>
        /// <param name="pos">On input: an alignment field, if desired
        /// On output: the offsets of the alignment field.</param>
        /// <exception cref="ArgumentException">If an argument in <paramref name="arguments"/> is not of the type
        /// expected by the format element(s) that use it.</exception>
        /// <exception cref="ArgumentException">If <paramref name="arguments"/> is
        /// an array of <see cref="object"/> and this format uses named arguments.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="pos"/> argument is null.</exception>
        /// <stable>ICU 3.0</stable>
        public override sealed StringBuffer Format(object arguments, StringBuffer result,
                                         FieldPosition pos)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            Format(arguments, new AppendableWrapper(result), pos);
            return result;
        }

        /// <summary>
        /// Formats an array of objects and inserts them into the
        /// <see cref="MessageFormat"/>'s pattern, producing an
        /// <see cref="AttributedCharacterIterator"/>.
        /// You can use the returned <see cref="AttributedCharacterIterator"/>
        /// to build the resulting <see cref="string"/>, as well as to determine information
        /// about the resulting <see cref="string"/>.
        /// <para/>
        /// The text of the returned <see cref="AttributedCharacterIterator"/> is
        /// the same that would be returned by
        /// <code>
        /// Format(arguments, new StringBuilder(), null).ToString()
        /// </code>
        /// In addition, the <see cref="AttributedCharacterIterator"/> contains at
        /// least attributes indicating where text was generated from an
        /// argument in the <paramref name="arguments"/> array. The keys of these attributes are of
        /// type <see cref="MessageFormatField"/>, their values are
        /// <see cref="int"/> objects indicating the index in the <paramref name="arguments"/>
        /// array of the argument from which the text was generated.
        /// <para/>
        /// The attributes/value from the underlying <see cref="Formatter"/>
        /// instances that <see cref="MessageFormat"/> uses will also be
        /// placed in the resulting <see cref="AttributedCharacterIterator"/>.
        /// This allows you to not only find where an argument is placed in the
        /// resulting <see cref="string"/>, but also which fields it contains in turn.
        /// </summary>
        /// <param name="arguments">An array of objects to be formatted and substituted.</param>
        /// <returns><see cref="AttributedCharacterIterator"/> describing the formatted value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="arguments"/> is null.</exception>
        /// <exception cref="ArgumentException">if a value in the
        /// <paramref name="arguments"/> array is not of the type
        /// expected by the corresponding argument or custom <see cref="Formatter"/> object.</exception>
        /// <stable>ICU 3.8</stable>
        public override AttributedCharacterIterator FormatToCharacterIterator(object arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(
                       "formatToCharacterIterator must be passed non-null object");
            }
            StringBuilder result = new StringBuilder();
            AppendableWrapper wrapper = new AppendableWrapper(result);
            wrapper.UseAttributes();
            Format(arguments, wrapper, null);
            AttributedString @as = new AttributedString(result.ToString());
            foreach (AttributeAndPosition a in wrapper.Attributes)
            {
                @as.AddAttribute(a.Key, a.Value, a.Start, a.Limit);
            }
            return @as.GetIterator();
        }

        /// <summary>
        /// Parses the string.
        /// </summary>
        /// <remarks>
        /// Caveats: The parse may fail in a number of circumstances.
        /// For example:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             If one of the arguments does not occur in the pattern.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             If the format of an argument loses information, such as
        ///             with a choice format where a large number formats to "many".
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             Does not yet handle recursion (where
        ///             the substituted strings contain {n} references.)
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             Will not always find a match (or the correct match)
        ///             if some part of the parse is ambiguous.
        ///             For example, if the pattern "{1},{2}" is used with the
        ///             string arguments {"a,b", "c"}, it will format as "a,b,c".
        ///             When the result is parsed, it will return {"a", "b,c"}.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             If a single argument is parsed more than once in the string,
        ///             then the later parse wins.
        ///         </description>
        ///     </item>
        /// </list>
        /// When the parse fails, use <see cref="ParsePosition.ErrorIndex"/> to find out
        /// where in the string did the parsing failed. The returned error
        /// index is the starting offset of the sub-patterns that the string
        /// is comparing with. For example, if the parsing string "AAA {0} BBB"
        /// is comparing against the pattern "AAD {0} BBB", the error index is
        /// 0. When an error occurs, the call to this method will return null.
        /// If the source is null, return an empty array.
        /// </remarks>
        /// <exception cref="ArgumentException">If this format uses named arguments</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="pos"/> argument is null.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual object[] Parse(string source, ParsePosition pos)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            if (msgPattern.HasNamedArguments)
            {
                throw new ArgumentException(
                        "This method is not available in MessageFormat objects " +
                        "that use named argument.");
            }

            // Count how many slots we need in the array.
            int maxArgId = -1;
            for (int partIndex = 0; (partIndex = NextTopLevelArgStart(partIndex)) >= 0;)
            {
                int argNumber = msgPattern.GetPart(partIndex + 1).Value;
                if (argNumber > maxArgId)
                {
                    maxArgId = argNumber;
                }
            }
            object[] resultArray = new object[maxArgId + 1];

            int backupStartPos = pos.Index;
            Parse(0, source, pos, resultArray, null);
            if (pos.Index == backupStartPos)
            { // unchanged, returned object is null
                return null;
            }

            return resultArray;
        }

        /// <summary>
        /// <icu/> Parses the string, returning the results in a <see cref="IDictionary{String, Object}"/>.
        /// This is similar to the version that returns an array
        /// of <see cref="object"/>.  This supports both named and numbered
        /// arguments-- if numbered, the keys in the map are the
        /// corresponding ASCII-decimal-digit strings (e.g. "0", "1", "2"...).
        /// </summary>
        /// <param name="source">The text to parse.</param>
        /// <param name="pos">The position at which to start parsing.  On return,
        /// contains the result of the parse.</param>
        /// <returns>A <see cref="IDictionary{String, Object}"/> containing key/value pairs for each parsed argument.</returns>
        /// <stable>ICU 3.8</stable>
        public virtual IDictionary<string, object> ParseToMap(string source, ParsePosition pos) // ICU4N TODO: API - rename ParseToDictionary()
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            IDictionary<string, object> result = new Dictionary<string, object>();
            int backupStartPos = pos.Index;
            Parse(0, source, pos, null, result);
            if (pos.Index == backupStartPos)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Parses text from the beginning of the given string to produce an object
        /// array.
        /// The method may not use the entire text of the given string.
        /// <para/>
        /// See the <see cref="Parse(string, ParsePosition)"/> method for more information
        /// on message parsing.
        /// </summary>
        /// <param name="source">A <see cref="string"/> whose beginning should be parsed.</param>
        /// <returns>An <see cref="object"/> array parsed from the string.</returns>
        /// <exception cref="FormatException">If the beginning of the specified string cannot be parsed.</exception>
        /// <exception cref="ArgumentException">If this format uses named arguments.</exception>
        /// <stable>ICU 3.0</stable>
        public virtual object[] Parse(string source)
        {
            ParsePosition pos = new ParsePosition(0);
            object[] result = Parse(source, pos);
            if (pos.Index == 0) // unchanged, returned object is null
                throw new FormatException("MessageFormat parse error!" +
                                         pos.ErrorIndex.ToString());

            return result;
        }

        /// <summary>
        /// Parses the string, filling either the <paramref name="argsMap"/> or the <paramref name="args"/>.
        /// This is a private method that all the public parsing methods call.
        /// This supports both named and numbered
        /// arguments-- if numbered, the keys in the map are the
        /// corresponding ASCII-decimal-digit strings (e.g. "0", "1", "2"...).
        /// </summary>
        /// <param name="msgStart">Index in the message pattern to start from.</param>
        /// <param name="source">The text to parse</param>
        /// <param name="pos">The position at which to start parsing.  On return,
        /// contains the result of the parse.</param>
        /// <param name="args">If not null, the parse results will be filled here (The pattern
        /// has to have numbered arguments in order for this to not be null).</param>
        /// <param name="argsMap">If not null, the parse results will be filled here.</param>
        private void Parse(int msgStart, string source, ParsePosition pos,
                           object[] args, IDictionary<string, object> argsMap)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            if (source == null)
            {
                return;
            }
            string msgString = msgPattern.PatternString;
            int prevIndex = msgPattern.GetPart(msgStart).Limit;
            int sourceOffset = pos.Index;
            ParsePosition tempStatus = new ParsePosition(0);

            for (int i = msgStart + 1; ; ++i)
            {
                MessagePatternPart part = msgPattern.GetPart(i);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                // Make sure the literal string matches.
                int len = index - prevIndex;
                if (len == 0 || msgString.RegionMatches(prevIndex, source, sourceOffset, len, StringComparison.Ordinal))
                {
                    sourceOffset += len;
                    prevIndex += len;
                }
                else
                {
                    pos.ErrorIndex = sourceOffset;
                    return; // leave index as is to signal error
                }
                if (type == MessagePatternPartType.MsgLimit)
                {
                    // Things went well! Done.
                    pos.Index = sourceOffset;
                    return;
                }
                if (type == MessagePatternPartType.SkipSyntax || type == MessagePatternPartType.InsertChar)
                {
                    prevIndex = part.Limit;
                    continue;
                }
                // We do not support parsing Plural formats. (No REPLACE_NUMBER here.)
                Debug.Assert(type == MessagePatternPartType.ArgStart, "Unexpected Part " + part + " in parsed message.");
                int argLimit = msgPattern.GetLimitPartIndex(i);

                MessagePatternArgType argType = part.ArgType;
                part = msgPattern.GetPart(++i);
                // Compute the argId, so we can use it as a key.
                object argId = null;
                int argNumber = 0;
                string key = null;
                if (args != null)
                {
                    argNumber = part.Value;  // ARG_NUMBER
                    //argId = Integer.valueOf(argNumber);
                    argId = argNumber;
                }
                else
                {
                    if (part.Type == MessagePatternPartType.ArgName)
                    {
                        key = msgPattern.GetSubstring(part);
                    }
                    else /* ARG_NUMBER */
                    {
                        //key = Integer.toString(part.Value);
                        key = ((int)part.Value).ToString();
                    }
                    argId = key;
                }

                ++i;
                Formatter formatter = null;
                bool haveArgResult = false;
                object argResult = null;
                if (cachedFormatters != null && (formatter = cachedFormatters[i - 2]) != null)
                {
                    // Just parse using the formatter.
                    tempStatus.Index = sourceOffset;
                    argResult = formatter.ParseObject(source, tempStatus);
                    if (tempStatus.Index == sourceOffset)
                    {
                        pos.ErrorIndex = sourceOffset;
                        return; // leave index as is to signal error
                    }
                    haveArgResult = true;
                    sourceOffset = tempStatus.Index;
                }
                else if (
                      argType == MessagePatternArgType.None ||
                      (cachedFormatters != null && cachedFormatters.ContainsKey(i - 2)))
                {
                    // Match as a string.
                    // if at end, use longest possible match
                    // otherwise uses first match to intervening string
                    // does NOT recursively try all possibilities
                    string stringAfterArgument = GetLiteralStringUntilNextArgument(argLimit);
                    int next;
                    if (stringAfterArgument.Length != 0)
                    {
                        next = source.IndexOf(stringAfterArgument, sourceOffset);
                    }
                    else
                    {
                        next = source.Length;
                    }
                    if (next < 0)
                    {
                        pos.ErrorIndex = sourceOffset;
                        return; // leave index as is to signal error
                    }
                    else
                    {
                        string strValue = source.Substring(sourceOffset, next - sourceOffset); // ICU4N: Corrected 2nd arg
                        if (!strValue.Equals("{" + argId.ToString() + "}"))
                        {
                            haveArgResult = true;
                            argResult = strValue;
                        }
                        sourceOffset = next;
                    }
                }
                else if (argType == MessagePatternArgType.Choice)
                {
                    tempStatus.Index = sourceOffset;
                    double choiceResult = ParseChoiceArgument(msgPattern, i, source, tempStatus);
                    if (tempStatus.Index == sourceOffset)
                    {
                        pos.ErrorIndex = sourceOffset;
                        return; // leave index as is to signal error
                    }
                    argResult = choiceResult;
                    haveArgResult = true;
                    sourceOffset = tempStatus.Index;
                }
                else if (argType.HasPluralStyle() || argType == MessagePatternArgType.Select)
                {
                    // No can do!
                    throw new InvalidOperationException(
                            "Parsing of plural/select/selectordinal argument is not supported.");
                }
                else
                {
                    // This should never happen.
                    throw new InvalidOperationException("unexpected argType " + argType);
                }
                if (haveArgResult)
                {
                    if (args != null)
                    {
                        args[argNumber] = argResult;
                    }
                    else if (argsMap != null)
                    {
                        argsMap[key] = argResult;
                    }
                }
                prevIndex = msgPattern.GetPart(argLimit).Limit;
                i = argLimit;
            }
        }

        /// <summary>
        /// <icu/> Parses text from the beginning of the given string to produce a <see cref="IDictionary{String, Object}"/> from
        /// argument to values. The method may not use the entire text of the given string.
        /// <para/>
        /// See the <see cref="Parse(string, ParsePosition)"/> method for more information on
        /// message parsing.
        /// </summary>
        /// <param name="source">A <see cref="string"/> whose beginning should be parsed.</param>
        /// <returns>A <see cref="IDictionary{String, Object}"/> parsed from the string.</returns>
        /// <exception cref="FormatException">If the beginning of the specified string cannot be parsed.</exception>
        /// <seealso cref="ParseToMap(string, ParsePosition)"/>
        /// <stable>ICU 3.8</stable>
        public virtual IDictionary<string, object> ParseToMap(string source)
        {
            ParsePosition pos = new ParsePosition(0);
            IDictionary<string, object> result = new Dictionary<string, object>();
            Parse(0, source, pos, null, result);
            if (pos.Index == 0) // unchanged, returned object is null
                throw new FormatException("MessageFormat parse error! " + pos.ErrorIndex.ToString());

            return result;
        }

        /// <summary>
        /// Parses text from a string to produce an object array or <see cref="IDictionary{String, Object}"/>.
        /// <para/>
        /// The method attempts to parse text starting at the index given by
        /// <paramref name="pos"/>.
        /// If parsing succeeds, then the index of <paramref name="pos"/> is updated
        /// to the index after the last character used (parsing does not necessarily
        /// use all characters up to the end of the string), and the parsed
        /// object array is returned. The updated <paramref name="pos"/> can be used to
        /// indicate the starting point for the next call to this method.
        /// If an error occurs, then the index of <paramref name="pos"/> is not
        /// changed, the error index of <paramref name="pos"/> is set to the index of
        /// the character where the error occurred, and null is returned.
        /// <para/>
        /// See the <see cref="Parse(string, ParsePosition)"/> method for more information
        /// on message parsing.
        /// </summary>
        /// <param name="source">A <see cref="string"/>, part of which should be parsed.</param>
        /// <param name="pos">A <see cref="ParsePosition"/> object with index and error
        /// index information as described above.</param>
        /// <returns>
        /// An <see cref="object"/> parsed from the string, either an
        /// array of <see cref="object"/>, or a <see cref="IDictionary{String, Object}"/>,
        /// depending on whether named
        /// arguments are used.  This can be queried using <see cref="UsesNamedArguments"/>.
        /// In case of error, returns null.
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="pos"/> is null.</exception>
        /// <stable>ICU 3.0</stable>
        public override object ParseObject(string source, ParsePosition pos)
        {
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            if (!msgPattern.HasNamedArguments)
            {
                return Parse(source, pos);
            }
            else
            {
                return ParseToMap(source, pos);
            }
        }

        /// <stable>ICU 3.0</stable>
        public override object Clone()
        {
            MessageFormat other = (MessageFormat)base.Clone();

            if (customFormatArgStarts != null)
            {
                other.customFormatArgStarts = new JCG.HashSet<int>(customFormatArgStarts);
            }
            else
            {
                other.customFormatArgStarts = null;
            }

            if (cachedFormatters != null)
            {
                other.cachedFormatters = new Dictionary<int, Formatter>();
                foreach (var entry in cachedFormatters)
                {
                    other.cachedFormatters[entry.Key] = entry.Value;
                }
            }
            else
            {
                other.cachedFormatters = null;
            }

            other.msgPattern = msgPattern == null ? null : (MessagePattern)msgPattern.Clone();

            // ICU4N TODO: Stock formatters and providers
            //other.stockDateFormatter =
            //        stockDateFormatter == null ? null : (DateFormat)stockDateFormatter.Clone();
            other.stockNumberFormatter =
                    stockNumberFormatter == null ? null : (NumberFormat)stockNumberFormatter.Clone();

            other.pluralProvider = null;
            other.ordinalProvider = null;
            return other;
        }

        /// <stable>ICU 3.0</stable>
        public override bool Equals(object obj)
        {
            if (this == obj)                      // quick check
                return true;
            if (obj == null || GetType() != obj.GetType())
                return false;
            MessageFormat other = (MessageFormat)obj;
            return Utility.ObjectEquals(uCulure, other.UCulture)
                    && Utility.ObjectEquals(msgPattern, other.msgPattern)
                    && Utility.ObjectEquals(cachedFormatters, other.cachedFormatters)
                    && Utility.ObjectEquals(customFormatArgStarts, other.customFormatArgStarts);
            // Note: It might suffice to only compare custom formatters
            // rather than all formatters.
        }

        /// <stable>ICU 3.0</stable>
        public override int GetHashCode()
        {
            return msgPattern.PatternString.GetHashCode(); // enough for reasonable distribution
        }

        // ICU4N specific - de-nested Field and renamed MessageFormatField

        internal IDictionary<int, Formatter> CachedFormatters
        {
            get => cachedFormatters;
            set => cachedFormatters = value;
        }

        // ===========================privates============================

        // *Important*: All fields must be declared *transient* so that we can fully
        // control serialization!
        // See for example Joshua Bloch's "Effective Java", chapter 10 Serialization.

        /// <summary>
        /// The locale to use for formatting numbers and dates.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private UCultureInfo uCulure;

        /// <summary>
        /// The <see cref="MessagePattern"/> which contains the parsed structure of the pattern string.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private MessagePattern msgPattern;

        /// <summary>
        /// Cached formatters so we can just use them whenever needed instead of creating
        /// them from scratch every time.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif

        private IDictionary<int, Formatter> cachedFormatters;

        /// <summary>
        /// Set of <see cref="MessagePatternPartType.ArgStart"/>part indexes where custom, user-provided <see cref="Formatter"/> objects
        /// have been set via <see cref="SetFormat(int, Formatter)"/> or similar API.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private ISet<int> customFormatArgStarts;

        // ICU4N TODO: Implementation
        ////        /**
        ////         * Stock formatters. Those are used when a format is not explicitly mentioned in
        ////         * the message. The format is inferred from the argument.
        ////         */
        ////#if FEATURE_SERIALIZABLE
        ////        [NonSerialized]
        ////#endif
        ////        private DateFormat stockDateFormatter;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private NumberFormat stockNumberFormatter;

#if FEATURE_SERIALIZABLE
                [NonSerialized]
#endif
        private PluralSelectorProvider pluralProvider;
#if FEATURE_SERIALIZABLE
                [NonSerialized]
#endif
        private PluralSelectorProvider ordinalProvider;

        ////        private DateFormat GetStockDateFormatter()
        ////        {
        ////            if (stockDateFormatter == null)
        ////            {
        ////                stockDateFormatter = DateFormat.getDateTimeInstance(
        ////                        DateFormat.SHORT, DateFormat.SHORT, ulocale);//fix
        ////            }
        ////            return stockDateFormatter;
        ////        }
        private NumberFormat GetStockNumberFormatter()
        {
            if (stockNumberFormatter == null)
            {
                stockNumberFormatter = NumberFormat.GetInstance(uCulure);
            }
            return stockNumberFormatter;
        }

        // *Important*: All fields must be declared *transient*.
        // See the longer comment above ulocale.

        /// <summary>
        /// Formats the arguments and writes the result into the
        /// <see cref="AppendableWrapper"/>, updates the field position.
        /// <para/>
        /// Exactly one of args and <paramref name="argsMap"/> must be null, the other non-null.
        /// </summary>
        /// <param name="msgStart">Index to msgPattern part to start formatting from.</param>
        /// <param name="pluralNumber">null except when formatting a plural argument sub-message
        /// where a '#' is replaced by the format string for this number.</param>
        /// <param name="args">The formattable objects array. Non-null iff numbered values are used.</param>
        /// <param name="argsMap">The key-value map of formattable objects. Non-null iff named values are used.</param>
        /// <param name="dest">Output parameter to receive the result.
        /// The result (string &amp; attributes) is appended to existing contents.</param>
        /// <param name="fp">Field position status.</param>
        private void Format(int msgStart, PluralSelectorContext pluralNumber,
                            object[] args, IDictionary<string, object> argsMap,
                            AppendableWrapper dest, FieldPosition fp)
        {
            string msgString = msgPattern.PatternString;
            int prevIndex = msgPattern.GetPart(msgStart).Limit;
            for (int i = msgStart + 1; ; ++i)
            {
                MessagePatternPart part = msgPattern.GetPart(i);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                dest.Append(msgString, prevIndex, index - prevIndex); // ICU4N: Fixed 3rd parameter
                if (type == MessagePatternPartType.MsgLimit)
                {
                    return;
                }
                prevIndex = part.Limit;
                if (type == MessagePatternPartType.ReplaceNumber)
                {
                    if (pluralNumber.forReplaceNumber)
                    {
                        // number-offset was already formatted.
                        dest.FormatAndAppend(pluralNumber.formatter,
                                pluralNumber.number, pluralNumber.numberString);
                    }
                    else
                    {
                        dest.FormatAndAppend(GetStockNumberFormatter(), pluralNumber.number);
                    }
                    continue;
                }
                if (type != MessagePatternPartType.ArgStart)
                {
                    continue;
                }
                int argLimit = msgPattern.GetLimitPartIndex(i);
                MessagePatternArgType argType = part.ArgType;
                part = msgPattern.GetPart(++i);
                object arg;
                bool noArg = false;
                object argId = null;
                string argName = msgPattern.GetSubstring(part);
                if (args != null)
                {
                    int argNumber = part.Value;  // ARG_NUMBER
                    if (dest.Attributes != null)
                    {
                        // We only need argId if we add it into the attributes.
                        //argId = Integer.valueOf(argNumber);
                        argId = argNumber;
                    }
                    if (0 <= argNumber && argNumber < args.Length)
                    {
                        arg = args[argNumber];
                    }
                    else
                    {
                        arg = null;
                        noArg = true;
                    }
                }
                else
                {
                    argId = argName;
                    if (argsMap != null && argsMap.ContainsKey(argName))
                    {
                        arg = argsMap[argName];
                    }
                    else
                    {
                        arg = null;
                        noArg = true;
                    }
                }
                ++i;
                int prevDestLength = dest.Length;
                Formatter formatter = null;
                if (noArg)
                {
                    dest.Append("{" + argName + "}");
                }
                else if (arg == null)
                {
                    dest.Append("null");
                }
                else if (pluralNumber != null && pluralNumber.numberArgIndex == (i - 2))
                {
                    if (pluralNumber.offset == 0)
                    {
                        // The number was already formatted with this formatter.
                        dest.FormatAndAppend(pluralNumber.formatter, pluralNumber.number, pluralNumber.numberString);
                    }
                    else
                    {
                        // Do not use the formatted (number-offset) string for a named argument
                        // that formats the number without subtracting the offset.
                        dest.FormatAndAppend(pluralNumber.formatter, arg);
                    }
                }
                else if (cachedFormatters != null && (cachedFormatters.TryGetValue(i - 2, out formatter) && formatter != null)) //(formatter = cachedFormatters.get(i - 2)) != null)
                {
                    // Handles all ArgType.SIMPLE, and formatters from setFormat() and its siblings.
                    if (formatter is ChoiceFormat ||
                        //formatter is PluralFormat ||
                        formatter is SelectFormat)
                    {
                        // We only handle nested formats here if they were provided via setFormat() or its siblings.
                        // Otherwise they are not cached and instead handled below according to argType.
                        string subMsgString = formatter.Format(arg);
                        if (subMsgString.IndexOf('{') >= 0 ||
                                (subMsgString.IndexOf('\'') >= 0 && !msgPattern.JdkAposMode))
                        {
                            MessageFormat subMsgFormat = new MessageFormat(subMsgString, uCulure);
                            subMsgFormat.Format(0, null, args, argsMap, dest, null);
                        }
                        else if (dest.Attributes == null)
                        {
                            dest.Append(subMsgString);
                        }
                        else
                        {
                            // This formats the argument twice, once above to get the subMsgString
                            // and then once more here.
                            // It only happens in formatToCharacterIterator()
                            // on a complex Format set via setFormat(),
                            // and only when the selected subMsgString does not need further formatting.
                            // This imitates ICU 4.6 behavior.
                            dest.FormatAndAppend(formatter, arg);
                        }
                    }
                    else
                    {
                        dest.FormatAndAppend(formatter, arg);
                    }
                }
                else if (argType == MessagePatternArgType.None ||
                    (cachedFormatters != null && cachedFormatters.ContainsKey(i - 2)))
                {
                    // ICU4N TODO: Implementation
                    // ArgType.NONE, or
                    // any argument which got reset to null via setFormat() or its siblings.
                    //if (arg is Number)
                    //if (arg.IsNumber())
                    //{
                    //    // format number if can
                    //    dest.FormatAndAppend(GetStockNumberFormatter(), arg);
                    //}
                    ////else if (arg is Date)
                    //else if (arg is DateTime)
                    //{
                    //    // format a Date if can
                    //    dest.FormatAndAppend(GetStockDateFormatter(), arg);
                    //}
                    //else
                    //{
                        dest.Append(arg.ToString());
                    //}
                }
                else if (argType == MessagePatternArgType.Choice)
                {
                    //if (!(arg id Number)) 
                    if (!arg.IsNumber())
                    {
                        throw new ArgumentException("'" + arg + "' is not a Number");
                    }
                    double number = Convert.ToDouble(arg); //((Number)arg).doubleValue();
                    int subMsgStart = FindChoiceSubMessage(msgPattern, i, number);
                    FormatComplexSubMessage(subMsgStart, null, args, argsMap, dest);
                }
                else if (argType.HasPluralStyle())
                {
                    //if (!(arg is Number))
                    if (!arg.IsNumber())
                    {
                        throw new ArgumentException("'" + arg + "' is not a Number");
                    }
                    PluralSelectorProvider selector;
                    if (argType == MessagePatternArgType.Plural)
                    {
                        if (pluralProvider == null)
                        {
                            pluralProvider = new PluralSelectorProvider(this, PluralType.Cardinal);
                        }
                        selector = pluralProvider;
                    }
                    else
                    {
                        if (ordinalProvider == null)
                        {
                            ordinalProvider = new PluralSelectorProvider(this, PluralType.Ordinal);
                        }
                        selector = ordinalProvider;
                    }
                    //Number number = (Number)arg;
                    decimal number = (decimal)arg;
                    double offset = msgPattern.GetPluralOffset(i);
                    PluralSelectorContext context =
                            new PluralSelectorContext(i, argName, number, offset);
                    int subMsgStart = PluralFormat.FindSubMessage(
                            msgPattern, i, selector, context, (double)number);
                    FormatComplexSubMessage(subMsgStart, context, args, argsMap, dest);
                }
                else if (argType == MessagePatternArgType.Select)
                {
                    int subMsgStart = SelectFormat.FindSubMessage(msgPattern, i, arg.ToString());
                    FormatComplexSubMessage(subMsgStart, null, args, argsMap, dest);
                }
                else
                {
                    // This should never happen.
                    throw new InvalidOperationException("unexpected argType " + argType);
                }
                fp = UpdateMetaData(dest, prevDestLength, fp, argId);
                prevIndex = msgPattern.GetPart(argLimit).Limit;
                i = argLimit;
            }
        }

        private void FormatComplexSubMessage(
                int msgStart, PluralSelectorContext pluralNumber,
                object[] args, IDictionary<string, object> argsMap,
                AppendableWrapper dest)
        {
            if (!msgPattern.JdkAposMode)
            {
                Format(msgStart, pluralNumber, args, argsMap, dest, null);
                return;
            }
            // JDK compatibility mode: (see JDK MessageFormat.format() API docs)
            // - remove SKIP_SYNTAX; that is, remove half of the apostrophes
            // - if the result string contains an open curly brace '{' then
            //   instantiate a temporary MessageFormat object and format again;
            //   otherwise just append the result string
            string msgString = msgPattern.PatternString;
            string subMsgString;
            StringBuilder sb = null;
            int prevIndex = msgPattern.GetPart(msgStart).Limit;
            for (int i = msgStart; ;)
            {
                MessagePatternPart part = msgPattern.GetPart(++i);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    if (sb == null)
                    {
                        subMsgString = msgString.Substring(prevIndex, index - prevIndex); // ICU4N: Corrected 2nd arg
                    }
                    else
                    {
                        subMsgString = sb.Append(msgString, prevIndex, index).ToString();
                    }
                    break;
                }
                else if (type == MessagePatternPartType.ReplaceNumber || type == MessagePatternPartType.SkipSyntax)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    sb.Append(msgString, prevIndex, index);
                    if (type == MessagePatternPartType.ReplaceNumber)
                    {
                        if (pluralNumber.forReplaceNumber)
                        {
                            // number-offset was already formatted.
                            sb.Append(pluralNumber.numberString);
                        }
                        else
                        {
                            sb.Append(GetStockNumberFormatter().Format(pluralNumber.number));
                        }
                    }
                    prevIndex = part.Limit;
                }
                else if (type == MessagePatternPartType.ArgStart)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    sb.Append(msgString, prevIndex, index);
                    prevIndex = index;
                    i = msgPattern.GetLimitPartIndex(i);
                    index = msgPattern.GetPart(i).Limit;
                    MessagePattern.AppendReducedApostrophes(msgString, prevIndex, index, sb);
                    prevIndex = index;
                }
            }
            if (subMsgString.IndexOf('{') >= 0)
            {
                MessageFormat subMsgFormat = new MessageFormat("", uCulure);
                subMsgFormat.ApplyPattern(subMsgString, ApostropheMode.DoubleRequired);
                subMsgFormat.Format(0, null, args, argsMap, dest, null);
            }
            else
            {
                dest.Append(subMsgString);
            }
        }

        /// <summary>
        /// Read as much literal string from the pattern string as possible. This stops
        /// as soon as it finds an argument, or it reaches the end of the string.
        /// </summary>
        /// <param name="from">Index in the pattern string to start from.</param>
        /// <returns>A substring from the pattern string representing the longest possible
        /// substring with no arguments.</returns>
        private string GetLiteralStringUntilNextArgument(int from)
        {
            StringBuilder b = new StringBuilder();
            string msgString = msgPattern.PatternString;
            int prevIndex = msgPattern.GetPart(from).Limit;
            for (int i = from + 1; ; ++i)
            {
                MessagePatternPart part = msgPattern.GetPart(i);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                b.Append(msgString, prevIndex, index);
                if (type == MessagePatternPartType.ArgStart || type == MessagePatternPartType.MsgLimit)
                {
                    return b.ToString();
                }
                Debug.Assert(type == MessagePatternPartType.SkipSyntax || type == MessagePatternPartType.InsertChar,

                            "Unexpected Part " + part + " in parsed message.");
                prevIndex = part.Limit;
            }
        }

        private FieldPosition UpdateMetaData(AppendableWrapper dest, int prevLength,
                                             FieldPosition fp, object argId)
        {
            if (dest.Attributes != null && prevLength < dest.Length)
            {
                dest.Attributes.Add(new AttributeAndPosition(argId, prevLength, dest.Length));
            }
            if (fp != null && MessageFormatField.Argument.Equals(fp.FieldAttribute))
            {
                fp.BeginIndex = prevLength;
                fp.EndIndex = dest.Length;
                return null;
            }
            return fp;
        }

        // This lives here because ICU4J does not have its own ChoiceFormat class.
        /// <summary>
        /// Finds the <see cref="ChoiceFormat"/> sub-message for the given number.
        /// </summary>
        /// <param name="pattern">A <see cref="MessagePattern"/>.</param>
        /// <param name="partIndex">the index of the first <see cref="ChoiceFormat"/> argument style part.</param>
        /// <param name="number">a number to be mapped to one of the <see cref="ChoiceFormat"/> argument's intervals</param>
        /// <returns>the sub-message start part index.</returns>
        private static int FindChoiceSubMessage(MessagePattern pattern, int partIndex, double number)
        {
            int count = pattern.CountParts();
            int msgStart;
            // Iterate over (ARG_INT|DOUBLE, ARG_SELECTOR, message) tuples
            // until ARG_LIMIT or end of choice-only pattern.
            // Ignore the first number and selector and start the loop on the first message.
            partIndex += 2;
            for (; ; )
            {
                // Skip but remember the current sub-message.
                msgStart = partIndex;
                partIndex = pattern.GetLimitPartIndex(partIndex);
                if (++partIndex >= count)
                {
                    // Reached the end of the choice-only pattern.
                    // Return with the last sub-message.
                    break;
                }
                MessagePatternPart part = pattern.GetPart(partIndex++);
                MessagePatternPartType type = part.Type;
                if (type == MessagePatternPartType.ArgLimit)
                {
                    // Reached the end of the ChoiceFormat style.
                    // Return with the last sub-message.
                    break;
                }
                // part is an ARG_INT or ARG_DOUBLE
                Debug.Assert(type.HasNumericValue());
                double boundary = pattern.GetNumericValue(part);
                // Fetch the ARG_SELECTOR character.
                int selectorIndex = pattern.GetPatternIndex(partIndex++);
                char boundaryChar = pattern.PatternString[selectorIndex];
                if (boundaryChar == '<' ? !(number > boundary) : !(number >= boundary))
                {
                    // The number is in the interval between the previous boundary and the current one.
                    // Return with the sub-message between them.
                    // The !(a>b) and !(a>=b) comparisons are equivalent to
                    // (a<=b) and (a<b) except they "catch" NaN.
                    break;
                }
            }
            return msgStart;
        }

        // Ported from C++ ChoiceFormat::parse().
        private static double ParseChoiceArgument(
                MessagePattern pattern, int partIndex,
                string source, ParsePosition pos)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern)); // ICU4N specific - don't fall back on NullReferenceException
            if (pos == null)
                throw new ArgumentNullException(nameof(pos)); // ICU4N specific - don't fall back on NullReferenceException

            // find the best number (defined as the one with the longest parse)
            int start = pos.Index;
            int furthest = start;
            double bestNumber = double.NaN;
            double tempNumber = 0.0;
            while (pattern.GetPartType(partIndex) != MessagePatternPartType.ArgLimit)
            {
                tempNumber = pattern.GetNumericValue(pattern.GetPart(partIndex));
                partIndex += 2;  // skip the numeric part and ignore the ARG_SELECTOR
                int msgLimit = pattern.GetLimitPartIndex(partIndex);
                int len = MatchStringUntilLimitPart(pattern, partIndex, msgLimit, source, start);
                if (len >= 0)
                {
                    int newIndex = start + len;
                    if (newIndex > furthest)
                    {
                        furthest = newIndex;
                        bestNumber = tempNumber;
                        if (furthest == source.Length)
                        {
                            break;
                        }
                    }
                }
                partIndex = msgLimit + 1;
            }
            if (furthest == start)
            {
                pos.ErrorIndex = start;
            }
            else
            {
                pos.Index = furthest;
            }
            return bestNumber;
        }

        /// <summary>
        /// Matches the <paramref name="pattern"/> string from the end of the <paramref name="partIndex"/> to
        /// the beginning of the <paramref name="limitPartIndex"/>,
        /// including all syntax except <see cref="MessagePatternPartType.SkipSyntax"/>
        /// against the <paramref name="source"/> string starting at <paramref name="sourceOffset"/>.
        /// If they match, returns the length of the <paramref name="source"/> string match.
        /// Otherwise returns -1.
        /// </summary>
        private static int MatchStringUntilLimitPart(
                MessagePattern pattern, int partIndex, int limitPartIndex,
                string source, int sourceOffset)
        {
            int matchingSourceLength = 0;
            string msgString = pattern.PatternString;
            int prevIndex = pattern.GetPart(partIndex).Limit;
            for (; ; )
            {
                MessagePatternPart part = pattern.GetPart(++partIndex);
                if (partIndex == limitPartIndex || part.Type == MessagePatternPartType.SkipSyntax)
                {
                    int index = part.Index;
                    int length = index - prevIndex;
                    if (length != 0 && !source.RegionMatches(sourceOffset, msgString, prevIndex, length, StringComparison.Ordinal))
                    {
                        return -1;  // mismatch
                    }
                    matchingSourceLength += length;
                    if (partIndex == limitPartIndex)
                    {
                        return matchingSourceLength;
                    }
                    prevIndex = part.Limit;  // SKIP_SYNTAX
                }
            }
        }

        /// <summary>
        /// Finds the "other" sub-message.
        /// </summary>
        /// <param name="partIndex">the index of the first <see cref="PluralFormat"/> argument style part.</param>
        /// <returns>the "other" sub-message start part index.</returns>
        private int FindOtherSubMessage(int partIndex)
        {
            int count = msgPattern.CountParts();
            MessagePatternPart part = msgPattern.GetPart(partIndex);
            if (part.Type.HasNumericValue())
            {
                ++partIndex;
            }
            // Iterate over (ARG_SELECTOR [ARG_INT|ARG_DOUBLE] message) tuples
            // until ARG_LIMIT or end of plural-only pattern.
            do
            {
                part = msgPattern.GetPart(partIndex++);
                MessagePatternPartType type = part.Type;
                if (type == MessagePatternPartType.ArgLimit)
                {
                    break;
                }
                Debug.Assert(type == MessagePatternPartType.ArgSelector);
                // part is an ARG_SELECTOR followed by an optional explicit value, and then a message
                if (msgPattern.PartSubstringMatches(part, "other"))
                {
                    return partIndex;
                }
                if (msgPattern.GetPartType(partIndex).HasNumericValue())
                {
                    ++partIndex;  // skip the numeric-value part of "=1" etc.
                }
                partIndex = msgPattern.GetLimitPartIndex(partIndex);
            } while (++partIndex < count);
            return 0;
        }

        /// <summary>
        /// Returns the <see cref="MessagePatternPartType.ArgStart"/> index of the first occurrence of the plural number in a sub-message.
        /// Returns -1 if it is a <see cref="MessagePatternPartType.ReplaceNumber"/>.
        /// Returns 0 if there is neither.
        /// </summary>
        private int FindFirstPluralNumberArg(int msgStart, string argName)
        {
            for (int i = msgStart + 1; ; ++i)
            {
                MessagePatternPart part = msgPattern.GetPart(i);
                MessagePatternPartType type = part.Type;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    return 0;
                }
                if (type == MessagePatternPartType.ReplaceNumber)
                {
                    return -1;
                }
                if (type == MessagePatternPartType.ArgStart)
                {
                    MessagePatternArgType argType = part.ArgType;
                    if (argName.Length != 0 && (argType == MessagePatternArgType.None || argType == MessagePatternArgType.Simple))
                    {
                        part = msgPattern.GetPart(i + 1);  // ARG_NUMBER or ARG_NAME
                        if (msgPattern.PartSubstringMatches(part, argName))
                        {
                            return i;
                        }
                    }
                    i = msgPattern.GetLimitPartIndex(i);
                }
            }
        }

        /// <summary>
        /// Mutable input/output values for the <see cref="PluralSelectorProvider"/>.
        /// Separate so that it is possible to make MessageFormat Freezable.
        /// </summary>
        internal sealed class PluralSelectorContext
        {
            internal PluralSelectorContext(int start, string name, /*Number*/ decimal num, double off)
            {
                startIndex = start;
                argName = name;
                // number needs to be set even when select() is not called.
                // Keep it as a Number/Formattable:
                // For format() methods, and to preserve information (e.g., BigDecimal).
                if (off == 0)
                {
                    number = num;
                }
                else
                {
                    number = (double)num - off;
                }
                offset = off;
            }

            public override string ToString()
            {
                throw new /*AssertionError*/ InvalidOperationException("PluralSelectorContext being formatted, rather than its number");
            }

            // Input values for plural selection with decimals.
            internal int startIndex;
            internal string argName;
            /** argument number - plural offset */
            //Number number;
            internal object number;
            internal double offset;
            // Output values for plural selection with decimals.
            /** -1 if REPLACE_NUMBER, 0 arg not found, >0 ARG_START index */
            internal int numberArgIndex;
            internal Formatter formatter;
            /** formatted argument number - plural offset */
            internal string numberString;
            /** true if number-offset was formatted with the stock number formatter */
            internal bool forReplaceNumber;
        }

        /// <summary>
        /// This provider helps defer instantiation of a <see cref="PluralRules"/> object
        /// until we actually need to select a keyword.
        /// For example, if the number matches an explicit-value selector like "=1"
        /// we do not need any <see cref="PluralRules"/>.
        /// </summary>
        private sealed class PluralSelectorProvider : PluralFormat.IPluralSelector
        {
            public PluralSelectorProvider(MessageFormat mf, PluralType type)
            {
                msgFormat = mf;
                this.type = type;
            }
            public string Select(object ctx, double number)
            {
                if (rules == null)
                {
                    rules = PluralRules.ForLocale(msgFormat.UCulture, type);
                }
                // Select a sub-message according to how the number is formatted,
                // which is specified in the selected sub-message.
                // We avoid this circle by looking at how
                // the number is formatted in the "other" sub-message
                // which must always be present and usually contains the number.
                // Message authors should be consistent across sub-messages.
                PluralSelectorContext context = (PluralSelectorContext)ctx;
                int otherIndex = msgFormat.FindOtherSubMessage(context.startIndex);
                context.numberArgIndex = msgFormat.FindFirstPluralNumberArg(otherIndex, context.argName);
                if (context.numberArgIndex > 0 && msgFormat.cachedFormatters != null)
                {
                    //context.formatter = msgFormat.CachedFormatters.get(context.numberArgIndex);
                    msgFormat.CachedFormatters.TryGetValue(context.numberArgIndex, out context.formatter);
                }
                if (context.formatter == null)
                {
                    context.formatter = msgFormat.GetStockNumberFormatter();
                    context.forReplaceNumber = true;
                }
                Debug.Assert((double)context.number /*.doubleValue()*/ == number);  // argument number minus the offset
                context.numberString = context.formatter.Format(context.number);
                // ICU4N TODO: DecimalFormat
                //if (context.formatter is DecimalFormat)
                //{
                //    IFixedDecimal dec = ((DecimalFormat)context.formatter).GetFixedDecimal(number);
                //    return rules.Select(dec);
                //}
                //else
                //{
                return rules.Select(number);
                //}
            }
            private MessageFormat msgFormat;
            private PluralRules rules;
            private PluralType type;
        }

        private void Format(object arguments, AppendableWrapper result, FieldPosition fp)
        {
            if ((arguments == null || arguments is IDictionary<string, object>))
            {
                Format(null, (IDictionary<string, object>)arguments, result, fp);
            }
            else
            {
                Format((object[])arguments, null, result, fp);
            }
        }

        /// <summary>
        /// Internal routine used by format.
        /// </summary>
        /// <exception cref="ArgumentException">if an argument in the
        /// <paramref name="arguments"/> map is not of the type
        /// expected by the format element(s) that use it.</exception>
        private void Format(object[] arguments, IDictionary<string, object> argsMap,
                            AppendableWrapper dest, FieldPosition fp)
        {
            if (arguments != null && msgPattern.HasNamedArguments)
            {
                throw new ArgumentException(
                    "This method is not available in MessageFormat objects " +
                    "that use alphanumeric argument names.");
            }
            Format(0, null, arguments, argsMap, dest, fp);
        }

        private void ResetPattern()
        {
            if (msgPattern != null)
            {
                msgPattern.Clear();
            }
            if (cachedFormatters != null)
            {
                cachedFormatters.Clear();
            }
            customFormatArgStarts = null;
        }

        private static readonly string[] typeList =
               new string[] { "number", "date", "time", "spellout", "ordinal", "duration" };
        private const int
            TYPE_NUMBER = 0,
            TYPE_DATE = 1,
            TYPE_TIME = 2,
            TYPE_SPELLOUT = 3,
            TYPE_ORDINAL = 4,
            TYPE_DURATION = 5;

        private static readonly string[] modifierList =
                new string[] { "", "currency", "percent", "integer" };

        private const int
            MODIFIER_EMPTY = 0,
            MODIFIER_CURRENCY = 1,
            MODIFIER_PERCENT = 2,
            MODIFIER_INTEGER = 3;

        private static readonly string[] dateModifierList =
                new string[] { "", "short", "medium", "long", "full" };

        private const int
            DATE_MODIFIER_EMPTY = 0,
            DATE_MODIFIER_SHORT = 1,
            DATE_MODIFIER_MEDIUM = 2,
            DATE_MODIFIER_LONG = 3,
            DATE_MODIFIER_FULL = 4;

        
        // Creates an appropriate Format object for the type and style passed.
        // Both arguments cannot be null.
        private Formatter CreateAppropriateFormat(string type, string style)
        {
            Formatter newFormat = null;
            int subformatType = FindKeyword(type, typeList);
            switch (subformatType)
            {
                case TYPE_NUMBER:
                    switch (FindKeyword(style, modifierList))
                    {
                        case MODIFIER_EMPTY:
                            newFormat = NumberFormat.GetInstance(uCulure);
                            break;
                        case MODIFIER_CURRENCY:
                            newFormat = NumberFormat.GetCurrencyInstance(uCulure);
                            break;
                        case MODIFIER_PERCENT:
                            newFormat = NumberFormat.GetPercentInstance(uCulure);
                            break;
                        case MODIFIER_INTEGER:
                            newFormat = NumberFormat.GetIntegerInstance(uCulure);
                            break;
                        default: // pattern
                            newFormat = NumberFormat.GetInstance(uCulure);
                            // ICU4N TODO: Finish implementation
                            //newFormat = new DecimalFormat(style,
                            //        new DecimalFormatSymbols(uCulure));
                            break;
                    }
                    break;
                // ICU4N TODO: Finish implementation
                //case TYPE_DATE:
                //    switch (FindKeyword(style, dateModifierList))
                //    {
                //        case DATE_MODIFIER_EMPTY:
                //            newFormat = DateFormat.getDateInstance(DateFormat.DEFAULT, ulocale);
                //            break;
                //        case DATE_MODIFIER_SHORT:
                //            newFormat = DateFormat.getDateInstance(DateFormat.SHORT, ulocale);
                //            break;
                //        case DATE_MODIFIER_MEDIUM:
                //            newFormat = DateFormat.getDateInstance(DateFormat.DEFAULT, ulocale);
                //            break;
                //        case DATE_MODIFIER_LONG:
                //            newFormat = DateFormat.getDateInstance(DateFormat.LONG, ulocale);
                //            break;
                //        case DATE_MODIFIER_FULL:
                //            newFormat = DateFormat.getDateInstance(DateFormat.FULL, ulocale);
                //            break;
                //        default:
                //            newFormat = new SimpleDateFormat(style, ulocale);
                //            break;
                //    }
                //    break;
                //case TYPE_TIME:
                //    switch (FindKeyword(style, dateModifierList))
                //    {
                //        case DATE_MODIFIER_EMPTY:
                //            newFormat = DateFormat.getTimeInstance(DateFormat.DEFAULT, ulocale);
                //            break;
                //        case DATE_MODIFIER_SHORT:
                //            newFormat = DateFormat.getTimeInstance(DateFormat.SHORT, ulocale);
                //            break;
                //        case DATE_MODIFIER_MEDIUM:
                //            newFormat = DateFormat.getTimeInstance(DateFormat.DEFAULT, ulocale);
                //            break;
                //        case DATE_MODIFIER_LONG:
                //            newFormat = DateFormat.getTimeInstance(DateFormat.LONG, ulocale);
                //            break;
                //        case DATE_MODIFIER_FULL:
                //            newFormat = DateFormat.getTimeInstance(DateFormat.FULL, ulocale);
                //            break;
                //        default:
                //            newFormat = new SimpleDateFormat(style, ulocale);
                //            break;
                //    }
                //    break;
                //case TYPE_SPELLOUT:
                //    {
                //        RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(ulocale,
                //                RuleBasedNumberFormat.SPELLOUT);
                //        string ruleset = style.Trim();
                //        if (ruleset.Length != 0)
                //        {
                //            try
                //            {
                //                rbnf.DefaultRuleSet = ruleset;
                //            }
                //            catch (Exception e)
                //            {
                //                // warn invalid ruleset
                //            }
                //        }
                //        newFormat = rbnf;
                //    }
                //    break;
                //case TYPE_ORDINAL:
                //    {
                //        RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(ulocale,
                //                RuleBasedNumberFormat.ORDINAL);
                //        string ruleset = style.Trim();
                //        if (ruleset.Length != 0)
                //        {
                //            try
                //            {
                //                rbnf.setDefaultRuleSet(ruleset);
                //            }
                //            catch (Exception e)
                //            {
                //                // warn invalid ruleset
                //            }
                //        }
                //        newFormat = rbnf;
                //    }
                //    break;
                //case TYPE_DURATION:
                //    {
                //        RuleBasedNumberFormat rbnf = new RuleBasedNumberFormat(ulocale,
                //                RuleBasedNumberFormat.DURATION);
                //        String ruleset = style.Trim();
                //        if (ruleset.Length != 0)
                //        {
                //            try
                //            {
                //                rbnf.setDefaultRuleSet(ruleset);
                //            }
                //            catch (Exception e)
                //            {
                //                // warn invalid ruleset
                //            }
                //        }
                //        newFormat = rbnf;
                //    }
                //    break;
                default:
                    throw new ArgumentException("Unknown format type \"" + type + "\"");
            }
            return newFormat;
        }

        //private static readonly CultureInfo rootLocale = CultureInfo.InvariantCulture;  // Locale.ROOT only @since 1.6

        private static int FindKeyword(string s, string[] list)
        {
            s = PatternProps.TrimWhiteSpace(s).ToLowerInvariant();
            for (int i = 0; i < list.Length; ++i)
            {
                if (s.Equals(list[i]))
                    return i;
            }
            return -1;
        }

        /**
         * Custom serialization, new in ICU 4.8.
         * We do not want to use default serialization because we only have a small
         * amount of persistent state which is better expressed explicitly
         * rather than via writing field objects.
         * @param out The output stream.
         * @serialData Writes the locale as a BCP 47 language tag string,
         * the MessagePattern.ApostropheMode as an object,
         * and the pattern string (null if none was applied).
         * Followed by an int with the number of (int formatIndex, Object formatter) pairs,
         * and that many such pairs, corresponding to previous setFormat() calls for custom formats.
         * Followed by an int with the number of (int, Object) pairs,
         * and that many such pairs, for future (post-ICU 4.8) extension of the serialization format.
         */
        private void WriteObject(Stream @out)
        {
            // ICU4N TODO: Serialization
            //@out.defaultWriteObject();
            //// ICU 4.8 custom serialization.
            //// locale as a BCP 47 language tag
            //@out.writeObject(ulocale.toLanguageTag());
            //    // ApostropheMode
            //    if (msgPattern == null) {
            //    msgPattern = new MessagePattern();
            //}
            //@out.writeObject(msgPattern.ApostropheMode);
            //// message pattern string
            //@out.writeObject(msgPattern.GetPatternString());
            //    // custom formatters
            //    if (customFormatArgStarts == null || customFormatArgStarts.isEmpty()) {
            //    @out.writeInt(0);
            //} else {
            //    @out.writeInt(customFormatArgStarts.size());
            //    int formatIndex = 0;
            //    for (int partIndex = 0; (partIndex = nextTopLevelArgStart(partIndex)) >= 0;)
            //    {
            //        if (customFormatArgStarts.contains(partIndex))
            //        {
            //            @out.writeInt(formatIndex);
            //            @out.writeObject(cachedFormatters.get(partIndex));
            //        }
            //        ++formatIndex;
            //    }
            //}
            //    // number of future (int, Object) pairs
            //    @out.writeInt(0);
        }

        /// <summary>
        /// Custom deserialization, new in ICU 4.8. See comments on writeObject().
        /// </summary>
        /// <exception cref="InvalidOperationException">if the objects read from the stream is invalid.</exception>
        private void ReadObject(Stream @in)
        {

            // ICU4N TODO: Serialization
            //        //@in.defaultReadObject();
            //// ICU 4.8 custom deserialization.
            //String languageTag = (String)@in.readObject();
            //ulocale = UCultureInfo.GetCultureInfoByIetfLanguageTag(languageTag);
            //        MessagePattern.ApostropheMode aposMode = (MessagePattern.ApostropheMode)in.readObject();
            //        if (msgPattern == null || aposMode != msgPattern.getApostropheMode()) {
            //            msgPattern = new MessagePattern(aposMode);
            //        }
            //        String msg = (String)in.readObject();
            //        if (msg != null) {
            //            applyPattern(msg);
            //        }
            //        // custom formatters
            //        for (int numFormatters = in.readInt(); numFormatters > 0; --numFormatters) {
            //            int formatIndex = in.readInt();
            //Format formatter = (Format)in.readObject();
            //setFormat(formatIndex, formatter);
            //        }
            //        // skip future (int, Object) pairs
            //        for (int numPairs = in.readInt(); numPairs > 0; --numPairs) {
            //            in.readInt();
            //            in.readObject();
            //        }
        }

        private void CacheExplicitFormats()
        {
            if (cachedFormatters != null)
            {
                cachedFormatters.Clear();
            }
            customFormatArgStarts = null;
            // The last two "parts" can at most be ARG_LIMIT and MSG_LIMIT
            // which we need not examine.
            int limit = msgPattern.CountParts() - 2;
            // This loop starts at part index 1 because we do need to examine
            // ARG_START parts. (But we can ignore the MSG_START.)
            for (int i = 1; i < limit; ++i)
            {
                MessagePatternPart part = msgPattern.GetPart(i);
                if (part.Type != MessagePatternPartType.ArgStart)
                {
                    continue;
                }
                MessagePatternArgType argType = part.ArgType;
                if (argType != MessagePatternArgType.Simple)
                {
                    continue;
                }
                int index = i;
                i += 2;
                string explicitType = msgPattern.GetSubstring(msgPattern.GetPart(i++));
                string style = "";
                if ((part = msgPattern.GetPart(i)).Type == MessagePatternPartType.ArgStyle)
                {
                    style = msgPattern.GetSubstring(part);
                    ++i;
                }
                Formatter formatter = CreateAppropriateFormat(explicitType, style);
                SetArgStartFormat(index, formatter);
            }
        }

        /// <summary>
        /// Sets a formatter for a <see cref="MessagePattern"/> <see cref="MessagePatternPartType.ArgStart"/> part index.
        /// </summary>
        private void SetArgStartFormat(int argStart, Formatter formatter)
        {
            if (CachedFormatters == null)
            {
                CachedFormatters = new Dictionary<int, Formatter>();
            }
            CachedFormatters[argStart] = formatter;
        }

        /// <summary>
        /// Sets a custom formatter for a <see cref="MessagePattern"/> <see cref="MessagePatternPartType.ArgStart"/> part index.
        /// "Custom" formatters are provided by the user via <see cref="SetFormat(int, Formatter)"/> or similar APIs.
        /// </summary>
        /// <param name="argStart"></param>
        /// <param name="formatter"></param>
        private void SetCustomArgStartFormat(int argStart, Formatter formatter)
        {
            SetArgStartFormat(argStart, formatter);
            if (customFormatArgStarts == null)
            {
                customFormatArgStarts = new JCG.HashSet<int>(1);
            }
            customFormatArgStarts.Add(argStart);
        }

        private const char SINGLE_QUOTE = '\'';
        private const char CURLY_BRACE_LEFT = '{';
        private const char CURLY_BRACE_RIGHT = '}';

        private const int STATE_INITIAL = 0;
        private const int STATE_SINGLE_QUOTE = 1;
        private const int STATE_IN_QUOTE = 2;
        private const int STATE_MSG_ELEMENT = 3;

        /// <summary>
        /// <icu/> Converts an 'apostrophe-friendly' pattern into a standard
        /// pattern.
        /// <em>This is obsolete for ICU 4.8 and higher MessageFormat pattern strings.</em>
        /// </summary>
        /// <remarks>
        /// See the class description for more about apostrophes and quoting,
        /// and differences between ICU and java.text.MessageFormat.
        /// <para/>
        /// java.text.MessageFormat and ICU 4.6 and earlier <see cref="MessageFormat"/>
        /// treat all ASCII apostrophes as
        /// quotes, which is problematic in some languages, e.g.
        /// French, where apostrophe is commonly used.  This utility
        /// assumes that only an unpaired apostrophe immediately before
        /// a brace is a true quote.  Other unpaired apostrophes are paired,
        /// and the resulting standard pattern string is returned.
        /// <para/>
        /// <b>Note</b>: It is not guaranteed that the returned pattern
        /// is indeed a valid pattern.  The only effect is to convert
        /// between patterns having different quoting semantics.
        /// <para/>
        /// <b>Note</b>: This method only works on top-level messageText,
        /// not messageText nested inside a complexArg.
        /// </remarks>
        /// <param name="pattern">the 'apostrophe-friendly' pattern to convert</param>
        /// <returns>the standard equivalent of the original pattern</returns>
        /// <stable>ICU 3.4</stable>
        public static string AutoQuoteApostrophe(string pattern)
        {
            StringBuilder buf = new StringBuilder(pattern.Length * 2);
            int state = STATE_INITIAL;
            int braceCount = 0;
            for (int i = 0, j = pattern.Length; i < j; ++i)
            {
                char c = pattern[i];
                switch (state)
                {
                    case STATE_INITIAL:
                        switch (c)
                        {
                            case SINGLE_QUOTE:
                                state = STATE_SINGLE_QUOTE;
                                break;
                            case CURLY_BRACE_LEFT:
                                state = STATE_MSG_ELEMENT;
                                ++braceCount;
                                break;
                        }
                        break;
                    case STATE_SINGLE_QUOTE:
                        switch (c)
                        {
                            case SINGLE_QUOTE:
                                state = STATE_INITIAL;
                                break;
                            case CURLY_BRACE_LEFT:
                            case CURLY_BRACE_RIGHT:
                                state = STATE_IN_QUOTE;
                                break;
                            default:
                                buf.Append(SINGLE_QUOTE);
                                state = STATE_INITIAL;
                                break;
                        }
                        break;
                    case STATE_IN_QUOTE:
                        switch (c)
                        {
                            case SINGLE_QUOTE:
                                state = STATE_INITIAL;
                                break;
                        }
                        break;
                    case STATE_MSG_ELEMENT:
                        switch (c)
                        {
                            case CURLY_BRACE_LEFT:
                                ++braceCount;
                                break;
                            case CURLY_BRACE_RIGHT:
                                if (--braceCount == 0)
                                {
                                    state = STATE_INITIAL;
                                }
                                break;
                        }
                        break;
                    ////CLOVER:OFF
                    default: // Never happens.
                        break;
                        ////CLOVER:ON
                }
                buf.Append(c);
            }
            // End of scan
            if (state == STATE_SINGLE_QUOTE || state == STATE_IN_QUOTE)
            {
                buf.Append(SINGLE_QUOTE);
            }
            return buf.ToString(); //new string(buf);
        }

        /// <summary>
        /// Convenience wrapper for Appendable, tracks the result string length.
        /// </summary>
        internal sealed class AppendableWrapper
        {
            public AppendableWrapper(StringBuilder sb)
            {
                app = sb.AsAppendable();
                length = sb.Length;
                attributes = null;
            }

            // ICU4N: IN .NET, there is only one StringBuilder
            //public AppendableWrapper(StringBuffer sb)
            //{
            //    app = sb;
            //    length = sb.Length;
            //    attributes = null;
            //}

            public void UseAttributes()
            {
                attributes = new List<AttributeAndPosition>();
            }

            public void Append(string s)
            {
                try
                {
                    app.Append(s);
                    length += s.Length;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(string s, int startIndex, int count) // ICU4N: Changed 3rd parameter to count
            {
                try
                {
                    app.Append(s, startIndex, count);
                    length += count;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(StringBuilder s)
            {
                try
                {
                    app.Append(s);
                    length += s.Length;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(char[] s)
            {
                try
                {
                    app.Append(s);
                    length += s.Length;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(char[] s, int startIndex, int count) // ICU4N: Changed 3rd parameter to count
            {
                try
                {
                    app.Append(s, startIndex, count);
                    length += count;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(StringBuilder s, int startIndex, int count) // ICU4N: Changed 3rd parameter to count
            {
                try
                {
                    app.Append(s, startIndex, count);
                    length += count;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(ICharSequence s)
            {
                try
                {
                    app.Append(s);
                    length += s.Length;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(ICharSequence s, int startIndex, int count) // ICU4N: Changed 3rd parameter to count
            {
                try
                {
                    app.Append(s, startIndex, count);
                    length += count;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void Append(CharacterIterator iterator)
            {
                length += Append(app, iterator);
            }

            public static int Append(IAppendable result, CharacterIterator iterator)
            {
                try
                {
                    int start = iterator.BeginIndex;
                    int limit = iterator.EndIndex;
                    int length = limit - start;
                    if (start < limit)
                    {
                        result.Append(iterator.First());
                        while (++start < limit)
                        {
                            result.Append(iterator.Next());
                        }
                    }
                    return length;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }

            public void FormatAndAppend(Formatter formatter, object arg)
            {
                if (attributes == null)
                {
                    Append(formatter.Format(arg));
                }
                else
                {
                    AttributedCharacterIterator formattedArg = formatter.FormatToCharacterIterator(arg);
                    int prevLength = length;
                    Append(formattedArg);
                    // Copy all of the attributes from formattedArg to our attributes list.
                    formattedArg.First();
                    int start = formattedArg.Index;  // Should be 0 but might not be.
                    int limit = formattedArg.EndIndex;  // == start + length - prevLength
                    int offset = prevLength - start;  // Adjust attribute indexes for the result string.
                    while (start < limit)
                    {
                        IDictionary<AttributedCharacterIteratorAttribute, object> map = formattedArg.GetAttributes();
                        int runLimit = formattedArg.GetRunLimit();
                        if (map.Count != 0)
                        {
                            foreach (var entry in map)
                            {
                                attributes.Add(
                                    new AttributeAndPosition(
                                        entry.Key, entry.Value,
                                        offset + start, offset + runLimit));
                            }
                        }
                        start = runLimit;
                        formattedArg.SetIndex(start);
                    }
                }
            }

            public void FormatAndAppend(Formatter formatter, object arg, string argString)
            {
                if (attributes == null && argString != null)
                {
                    Append(argString);
                }
                else
                {
                    FormatAndAppend(formatter, arg);
                }
            }

            public int Length => length;
            public IList<AttributeAndPosition> Attributes => attributes;

            private IAppendable app;
            private int length;
            private IList<AttributeAndPosition> attributes;
        }

        internal sealed class AttributeAndPosition
        {
            /// <summary>
            /// Defaults the field to <see cref="MessageFormatField.Argument"/>.
            /// </summary>
            public AttributeAndPosition(object fieldValue, int startIndex, int limitIndex)
            {
                Init(MessageFormatField.Argument, fieldValue, startIndex, limitIndex);
            }

            public AttributeAndPosition(AttributedCharacterIteratorAttribute field, Object fieldValue, int startIndex, int limitIndex)
            {
                Init(field, fieldValue, startIndex, limitIndex);
            }

            public void Init(AttributedCharacterIteratorAttribute field, Object fieldValue, int startIndex, int limitIndex)
            {
                key = field;
                value = fieldValue;
                start = startIndex;
                limit = limitIndex;
            }

            public AttributedCharacterIteratorAttribute Key => key;
            public object Value => value;
            public int Start => start;
            public int Limit => limit;

            private AttributedCharacterIteratorAttribute key;
            private object value;
            private int start;
            private int limit;
        }
    }

    /// <summary>
    /// Defines constants that are used as attribute keys in the
    /// <see cref="AttributedCharacterIterator"/> returned
    /// from <see cref="MessageFormat.FormatToCharacterIterator(object)"/>.
    /// </summary>
    /// <stable>ICU 3.8</stable>
    internal class MessageFormatField : FormatField // ICU4N: Marked internal until implementation is completed
    {

        //private static readonly long serialVersionUID = 7510380454602616157L;

        /// <summary>
        /// Create a <see cref="MessageFormatField"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute</param>
        /// <stable>ICU 3.8</stable>
        protected MessageFormatField(string name)
            : base(name)
        {

        }

        /// <summary>
        /// Resolves instances being deserialized to the predefined constants.
        /// </summary>
        /// <returns>resolved <see cref="MessageFormatField"/> constant</returns>
        /// <exception cref="InvalidOperationException">if the constant could not be resolved.</exception>
        /// <stable>ICU 3.8</stable>
        protected override object ReadResolve()
        {
            if (this.GetType() != typeof(MessageFormatField))
            {
                throw new InvalidOperationException(
                    "A subclass of MessageFormat.Field must implement readResolve.");
            }
            if (this.Name.Equals(Argument.Name))
            {
                return Argument;
            }
            else
            {
                throw new InvalidOperationException("Unknown attribute name.");
            }
        }

        /// <summary>
        /// Constant identifying a portion of a message that was generated
        /// from an argument passed into <see cref="MessageFormat.FormatToCharacterIterator(object)"/>.
        /// The value associated with the key will be an <see cref="int"/>
        /// indicating the index in the <c>arguments</c> array of the
        /// argument from which the text was generated.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly MessageFormatField Argument = new MessageFormatField("message argument field");
    }
}
