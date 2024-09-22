using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using static ICU4N.Text.PluralRules;
using Double = J2N.Numerics.Double;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="PluralFormat"/> supports the creation of internationalized
    /// messages with plural inflection. It is based on <i>plural
    /// selection</i>, i.e. the caller specifies messages for each
    /// plural case that can appear in the user's language and the
    /// <see cref="PluralFormat"/> selects the appropriate message based on
    /// the number.
    /// </summary>
    /// <remarks>
    /// <h3>The Problem of Plural Forms in Internationalized Messages</h3>
    /// <para/>
    /// Different languages have different ways to inflect
    /// plurals. Creating internationalized messages that include plural
    /// forms is only feasible when the framework is able to handle plural
    /// forms of <i>all</i> languages correctly. <see cref="ChoiceFormat"/>
    /// doesn't handle this well, because it attaches a number interval to
    /// each message and selects the message whose interval contains a
    /// given number. This can only handle a finite number of
    /// intervals. But in some languages, like Polish, one plural case
    /// applies to infinitely many intervals (e.g., the paucal case applies to
    /// numbers ending with 2, 3, or 4 except those ending with 12, 13, or
    /// 14). Thus <see cref="ChoiceFormat"/> is not adequate.
    /// <para/>
    /// <see cref="PluralFormat"/> deals with this by breaking the problem
    /// into two parts:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             It uses <code>PluralRules</code> that can define more complex
    ///             conditions for a plural case than just a single interval. These plural
    ///             rules define both what plural cases exist in a language, and to
    ///             which numbers these cases apply.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             It provides predefined plural rules for many languages. Thus, the programmer
    ///             need not worry about the plural cases of a language and
    ///             does not have to define the plural cases; they can simply
    ///             use the predefined keywords. The whole plural formatting of messages can
    ///             be done using localized patterns from resource bundles. For predefined plural
    ///             rules, see the CLDR <i>Language Plural Rules</i> page at
    ///             <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>
    ///         </description>
    ///     </item>
    /// </list>
    /// <h4>Usage of <see cref="PluralFormat"/></h4>
    /// <para/>
    /// Note: Typically, plural formatting is done via <see cref="MessageFormat"/>
    /// with a <c>plural</c> argument type,
    /// rather than using a stand-alone <see cref="PluralFormat"/>.
    /// <para/>
    /// This discussion assumes that you use <see cref="PluralFormat"/> with
    /// a predefined set of plural rules. You can create one using one of
    /// the constructors that takes a <see cref="UCultureInfo"/> object. To
    /// specify the message pattern, you can either pass it to the
    /// constructor or set it explicitly using the
    /// <see cref="ApplyPattern(string)"/> method. The <see cref="Format(double)"/>
    /// method takes a number and selects the message of the
    /// matching plural case. This message will be returned.
    /// <h5>Patterns and Their Interpretation</h5>
    /// <para/>
    /// The pattern text defines the message output for each plural case of the
    /// specified locale. Syntax:
    /// <code>
    /// pluralStyle = [offsetValue] (selector '{' message '}')+
    /// offsetValue = "offset:" number
    /// selector = explicitValue | keyword
    /// explicitValue = '=' number  // adjacent, no white space in between
    /// keyword = [^[[:Pattern_Syntax:][:Pattern_White_Space:]]]+
    /// message: see <see cref="MessageFormat"/>
    /// </code>
    /// <see cref="Globalization.UProperty.Pattern_White_Space"/> between syntax elements is ignored, except
    /// between the {curly braces} and their sub-message,
    /// and between the '=' and the number of an explicitValue.
    /// <para/>
    /// There are 6 predefined case keywords in CLDR/ICU - 'zero', 'one', 'two', 'few', 'many' and
    /// 'other'. You always have to define a message text for the default plural case
    /// "<c>other</c>" which is contained in every rule set.
    /// If you do not specify a message text for a particular plural case, the
    /// message text of the plural case "<c>other</c>" gets assigned to this
    /// plural case.
    /// <para/>
    /// When formatting, the input number is first matched against the explicitValue clauses.
    /// If there is no exact-number match, then a keyword is selected by calling
    /// the <see cref="PluralRules"/> with the input number <em>minus the offset</em>.
    /// (The offset defaults to 0 if it is omitted from the pattern string.)
    /// If there is no clause with that keyword, then the "other" clauses is returned.
    /// <para/>
    /// An unquoted pound sign (<c>#</c>) in the selected sub-message
    /// itself (i.e., outside of arguments nested in the sub-message)
    /// is replaced by the input number minus the offset.
    /// The number-minus-offset value is formatted using a
    /// <see cref="NumberFormat"/> for the <see cref="PluralFormat"/>'s locale. If you
    /// need special number formatting, you have to use a <see cref="MessageFormat"/>
    /// and explicitly specify a <see cref="NumberFormat"/> argument.
    /// <strong>Note:</strong> That argument is formatting without subtracting the offset!
    /// If you need a custom format and have a non-zero offset, then you need to pass the
    /// number-minus-offset value as a separate parameter.
    /// <para/>
    /// For a usage example, see the <see cref="MessageFormat"/> class documentation.
    /// 
    /// <h4>Defining Custom Plural Rules</h4>
    /// <para/>
    /// If you need to use <see cref="PluralFormat"/> with custom rules, you can
    /// create a <see cref="PluralRules"/> object and pass it to
    /// <see cref="PluralFormat"/>'s constructor. If you also specify a locale in this
    /// constructor, this locale will be used to format the number in the message
    /// texts.
    /// <para/>
    /// For more information about <see cref="PluralRules"/>, see <see cref="PluralRules"/>.
    /// </remarks>
    /// <author>tschumann (Tim Schumann)</author>
    /// <stable>ICU 3.8</stable>
    internal class PluralFormat : UFormat // ICU4N: Marked internal until implementation is completed
    {
        //private static readonly long serialVersionUID = 1L;

        /// <summary>
        /// The locale used for standard number formatting and getting the predefined
        /// plural rules (if they were not defined explicitely).
        /// </summary>
        /// <serial/>
        private UCultureInfo ulocale = null;

        /// <summary>
        /// The plural rules used for plural selection.
        /// </summary>
        /// <serial/>
        private PluralRules pluralRules = null;

        /// <summary>
        /// The applied pattern string.
        /// </summary>
        /// <serial/>
        private string pattern = null;

        /// <summary>
        /// The <see cref="MessagePattern"/> which contains the parsed structure of the pattern string.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private MessagePattern msgPattern;

        /// <summary>
        /// Obsolete with use of <see cref="MessagePattern"/> since ICU 4.8. Used to be:
        /// The format messages for each plural case. It is a mapping:
        /// <see cref="string"/>(plural case keyword) --&gt; <see cref="string"/>
        /// (message for this plural case).
        /// </summary>
        /// <serial/>
        private IDictionary<string, string> parsedValues = null;

        /// <summary>
        /// This <see cref="NumberFormat"/> is used for the standard formatting of
        /// the number inserted into the message.
        /// </summary>
        /// <serial/>
        private NumberFormat numberFormat = null;

        /// <summary>
        /// The offset to subtract before invoking plural rules.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private double offset = 0;

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for the default <see cref="UCultureInfo.CurrentCulture"/> locale.
        /// This locale will be used to get the set of plural rules and for standard
        /// number formatting.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public PluralFormat()
        {
            Init(null, PluralType.Cardinal, UCultureInfo.CurrentCulture, null);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given locale.
        /// </summary>
        /// <param name="ulocale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(UCultureInfo ulocale)
        {
            Init(null, PluralType.Cardinal, ulocale, null);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given
        /// <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="locale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <stable>ICU 54</stable>
        public PluralFormat(CultureInfo locale)
            : this(locale.ToUCultureInfo())
        {
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given set of rules.
        /// The standard number formatting will be done using the default <see cref="UCultureInfo.CurrentCulture"/> locale.
        /// </summary>
        /// <param name="rules">defines the behavior of the <see cref="PluralFormat"/>
        /// object.</param>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(PluralRules rules)
        {
            Init(rules, PluralType.Cardinal, UCultureInfo.CurrentCulture, null);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given set of rules.
        /// The standard number formatting will be done using the given locale.
        /// </summary>
        /// <param name="ulocale">the default number formatting will be done using this
        /// locale.</param>
        /// <param name="rules">defines the behavior of the <see cref="PluralFormat"/>
        /// object.</param>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(UCultureInfo ulocale, PluralRules rules)
        {
            Init(rules, PluralType.Cardinal, ulocale, null);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given set of rules.
        /// The standard number formatting will be done using the given locale.
        /// </summary>
        /// <param name="locale">the default number formatting will be done using this
        /// locale.</param>
        /// <param name="rules">defines the behavior of the <see cref="PluralFormat"/>
        /// object.</param>
        /// <stable>ICU 54</stable>
        public PluralFormat(CultureInfo locale, PluralRules rules)
            : this(locale.ToUCultureInfo(), rules)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PluralFormat"/> for the plural type.
        /// The standard number formatting will be done using the given locale.
        /// </summary>
        /// <param name="ulocale">the default number formatting will be done using this
        /// locale.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <stable>ICU 50</stable>
        public PluralFormat(UCultureInfo ulocale, PluralType type)
        {
            Init(null, type, ulocale, null);
        }

        /// <summary>
        /// Creates a new <see cref="PluralFormat"/> for the plural type.
        /// The standard number formatting will be done using the given <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="locale">the default number formatting will be done using this
        /// locale.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <stable>ICU 54</stable>
        public PluralFormat(CultureInfo locale, PluralType type)
            : this(locale.ToUCultureInfo(), type)
        {
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given pattern string.
        /// The default <see cref="UCultureInfo.CurrentCulture"/> locale will be used to get the set of plural rules and for
        /// standard number formatting.
        /// </summary>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(string pattern)
        {
            Init(null, PluralType.Cardinal, UCultureInfo.CurrentCulture, null);
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given pattern string and
        /// locale.
        /// The locale will be used to get the set of plural rules and for
        /// standard number formatting.
        /// </summary>
        /// <param name="ulocale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(UCultureInfo ulocale, string pattern)
        {
            Init(null, PluralType.Cardinal, ulocale, null);
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given set of rules and a
        /// pattern.
        /// The standard number formatting will be done using the default <see cref="UCultureInfo.CurrentCulture"/> locale.
        /// </summary>
        /// <param name="rules">defines the behavior of the <see cref="PluralFormat"/>
        /// object.</param>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(PluralRules rules, string pattern)
        {
            Init(rules, PluralType.Cardinal, UCultureInfo.CurrentCulture, null);
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Creates a new cardinal-number <see cref="PluralFormat"/> for a given set of rules, a
        /// pattern and a locale.
        /// </summary>
        /// <param name="ulocale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <param name="rules">defines the behavior of the <see cref="PluralFormat"/>
        /// object.</param>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <stable>ICU 3.8</stable>
        public PluralFormat(UCultureInfo ulocale, PluralRules rules, string pattern)
        {
            Init(rules, PluralType.Cardinal, ulocale, null);
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Creates a new <see cref="PluralFormat"/> for a plural type, a
        /// pattern and a locale.
        /// </summary>
        /// <param name="ulocale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <stable>ICU 50</stable>
        public PluralFormat(UCultureInfo ulocale, PluralType type, string pattern)
        {
            Init(null, type, ulocale, null);
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Creates a new <see cref="PluralFormat"/> for a plural type, a
        /// pattern and a locale.
        /// </summary>
        /// <param name="ulocale">the <see cref="PluralFormat"/> will be configured with
        /// rules for this locale. This locale will also be used for standard
        /// number formatting.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <param name="pattern">the pattern for this <see cref="PluralFormat"/>.</param>
        /// <param name="numberFormat">The number formatter to use.</param>
        /// <exception cref="ArgumentException">If the pattern is invalid.</exception>
        internal PluralFormat(UCultureInfo ulocale, PluralType type, string pattern, NumberFormat numberFormat)
        {
            Init(null, type, ulocale, numberFormat);
            ApplyPattern(pattern);
        }

        /*
         * Initializes the <code>PluralRules</code> object.
         * Postcondition:<br/>
         *   <code>ulocale</code>    :  is <code>locale</code><br/>
         *   <code>pluralRules</code>:  if <code>rules</code> != <code>null</code>
         *                              it's set to rules, otherwise it is the
         *                              predefined plural rule set for the locale
         *                              <code>ulocale</code>.<br/>
         *   <code>parsedValues</code>: is <code>null</code><br/>
         *   <code>pattern</code>:      is <code>null</code><br/>
         *   <code>numberFormat</code>: a <code>NumberFormat</code> for the locale
         *                              <code>ulocale</code>.
         */
        private void Init(PluralRules rules, PluralType type, UCultureInfo locale, NumberFormat numberFormat)
        {
            ulocale = locale;
            pluralRules = (rules == null) ? PluralRules.GetInstance(ulocale, type) // ICU4N TODO: Make extension method for UCultureInfo.GetPluralRules(PluralType)..?
                                          : rules;
            // ICU4N: Factored out pluralRulesWrapper by implementing IPluralSelector directly on PluralRules
            ResetPattern();
            this.numberFormat = (numberFormat == null) ? NumberFormat.GetInstance(ulocale) : numberFormat;
        }

        private void ResetPattern()
        {
            pattern = null;
            if (msgPattern != null)
            {
                msgPattern.Clear();
            }
            offset = 0;
        }

        /// <summary>
        /// Sets the pattern used by this plural format.
        /// The method parses the pattern and creates a map of format strings
        /// for the plural rules.
        /// Patterns and their interpretation are specified in the class description.
        /// </summary>
        /// <param name="pattern">the pattern for this plural format.</param>
        /// <exception cref="ArgumentException">if the pattern is invalid.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual void ApplyPattern(string pattern)
        {
            this.pattern = pattern;
            if (msgPattern == null)
            {
                msgPattern = new MessagePattern();
            }
            try
            {
                msgPattern.ParsePluralStyle(pattern);
                offset = msgPattern.GetPluralOffset(0);
            }
            catch (Exception)
            {
                ResetPattern();
                throw;
            }
        }

        /// <summary>
        /// Returns the pattern for this <see cref="PluralFormat"/>.
        /// </summary>
        /// <returns>the pattern string</returns>
        /// <stable>ICU 4.2</stable>
        public virtual string ToPattern()
        {
            return pattern;
        }

        /// <summary>
        /// Finds the <see cref="PluralFormat"/> sub-message for the given number, or the "other" sub-message.
        /// </summary>
        /// <param name="pattern">A <see cref="MessagePattern"/>.</param>
        /// <param name="partIndex">the index of the first <see cref="PluralFormat"/> argument style part.</param>
        /// <param name="selector">the <see cref="IPluralSelector"/> for mapping the number (minus offset) to a keyword.</param>
        /// <param name="context">worker object for the selector.</param>
        /// <param name="number">a number to be matched to one of the <see cref="PluralFormat"/> argument's explicit values,
        /// or mapped via the <see cref="IPluralSelector"/>.</param>
        /// <returns>the sub-message start part index.</returns>
        internal static int FindSubMessage(
            MessagePattern pattern, int partIndex,
            IPluralSelector selector, object context, double number)
        {
            int count = pattern.PartCount;
            double offset;
            MessagePatternPart part = pattern.GetPart(partIndex);
            if (part.Type.HasNumericValue())
            {
                offset = pattern.GetNumericValue(part);
                ++partIndex;
            }
            else
            {
                offset = 0;
            }
            // The keyword is null until we need to match against a non-explicit, not-"other" value.
            // Then we get the keyword from the selector.
            // (In other words, we never call the selector if we match against an explicit value,
            // or if the only non-explicit keyword is "other".)
            string keyword = null;
            // When we find a match, we set msgStart>0 and also set this boolean to true
            // to avoid matching the keyword again (duplicates are allowed)
            // while we continue to look for an explicit-value match.
            bool haveKeywordMatch = false;
            // msgStart is 0 until we find any appropriate sub-message.
            // We remember the first "other" sub-message if we have not seen any
            // appropriate sub-message before.
            // We remember the first matching-keyword sub-message if we have not seen
            // one of those before.
            // (The parser allows [does not check for] duplicate keywords.
            // We just have to make sure to take the first one.)
            // We avoid matching the keyword twice by also setting haveKeywordMatch=true
            // at the first keyword match.
            // We keep going until we find an explicit-value match or reach the end of the plural style.
            int msgStart = 0;
            // Iterate over (ARG_SELECTOR [ARG_INT|ARG_DOUBLE] message) tuples
            // until ARG_LIMIT or end of plural-only pattern.
            do
            {
                part = pattern.GetPart(partIndex++);
                MessagePatternPartType type = part.Type;
                if (type == MessagePatternPartType.ArgLimit)
                {
                    break;
                }
                Debug.Assert(type == MessagePatternPartType.ArgSelector);
                // part is an ARG_SELECTOR followed by an optional explicit value, and then a message
                if (pattern.GetPartType(partIndex).HasNumericValue())
                {
                    // explicit value like "=2"
                    part = pattern.GetPart(partIndex++);
                    if (number == pattern.GetNumericValue(part))
                    {
                        // matches explicit value
                        return partIndex;
                    }
                }
                else if (!haveKeywordMatch)
                {
                    // plural keyword like "few" or "other"
                    // Compare "other" first and call the selector if this is not "other".
                    if (pattern.PartSubstringMatches(part, "other"))
                    {
                        if (msgStart == 0)
                        {
                            msgStart = partIndex;
                            if (keyword != null && keyword.Equals("other"))
                            {
                                // This is the first "other" sub-message,
                                // and the selected keyword is also "other".
                                // Do not match "other" again.
                                haveKeywordMatch = true;
                            }
                        }
                    }
                    else
                    {
                        if (keyword == null)
                        {
                            keyword = selector.Select(context, number - offset);
                            if (msgStart != 0 && keyword.Equals("other"))
                            {
                                // We have already seen an "other" sub-message.
                                // Do not match "other" again.
                                haveKeywordMatch = true;
                                // Skip keyword matching but do getLimitPartIndex().
                            }
                        }
                        if (!haveKeywordMatch && pattern.PartSubstringMatches(part, keyword))
                        {
                            // keyword matches
                            msgStart = partIndex;
                            // Do not match this keyword again.
                            haveKeywordMatch = true;
                        }
                    }
                }
                partIndex = pattern.GetLimitPartIndex(partIndex);
            } while (++partIndex < count);
            return msgStart;
        }

        /// <summary>
        /// Interface for selecting <see cref="PluralFormat"/> keywords for numbers.
        /// The <see cref="PluralRules"/> class was intended to implement this interface,
        /// but there is no public API that uses a <see cref="IPluralSelector"/>,
        /// only <see cref="MessageFormat"/> and <see cref="PluralFormat"/> have <see cref="IPluralSelector"/> implementations.
        /// Therefore, <see cref="PluralRules"/> is not marked to implement this non-public interface,
        /// to avoid confusing users.
        /// </summary>
        /// <internal/>
        internal interface IPluralSelector
        {
            /// <summary>
            /// Given a number, returns the appropriate <see cref="PluralFormat"/> keyword.
            /// </summary>
            /// <param name="context">worker object for the selector.</param>
            /// <param name="number">The number to be plural-formatted.</param>
            /// <returns>The selected <see cref="PluralFormat"/> keyword.</returns>
            string Select(object context, double number);
        }

        // ICU4N: Factored out PluralSelectorAdapter class and pluralRulesWrapper field by implementing
        // IPluralSelector directly on PluralRules.

        /// <summary>
        /// Formats a plural message for a given number.
        /// </summary>
        /// <param name="number">a number for which the plural message should be formatted.
        /// If no pattern has been applied to this
        /// <see cref="PluralFormat"/> object yet, the formatted number will
        /// be returned.
        /// </param>
        /// <returns>the string containing the formatted plural message.</returns>
        /// <stable>ICU 4.0</stable>
        public string Format(double number)
        {
            return Format(Double.GetInstance(number), number);
        }

        /// <summary>
        /// Formats a plural message for a given number and appends the formatted
        /// message to the given <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="number">a number for which
        /// the plural message should be formatted. If no pattern has been
        /// applied to this <see cref="PluralFormat"/> object yet, the
        /// formatted number will be returned.
        /// Note: If this object is not a number type (<see cref="long"/>, <see cref="double"/>, etc),
        /// the <paramref name="toAppendTo"/> will not be modified.</param>
        /// <param name="toAppendTo">the formatted message will be appended to this <see cref="StringBuilder"/></param>
        /// <param name="pos">will be ignored by this method.</param>
        /// <returns>the <see cref="StringBuilder"/> passed in as <paramref name="toAppendTo"/>, with formatted text
        /// appended.</returns>
        /// <exception cref="ArgumentException">if number cannot be converted to a <see cref="double"/>.</exception>
        /// <stable>ICU 3.8</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(object number, StringBuffer toAppendTo,
                FieldPosition pos)
        {
            if (!(number is J2N.Numerics.Number num))
            {
                throw new ArgumentException("'" + number + "' is not a Number");
            }
            toAppendTo.Append(Format(num, num.ToInt64()));
            return toAppendTo;
        }

        private string Format(J2N.Numerics.Number numberObject, double number)
        {
            // If no pattern was applied, return the formatted number.
            if (msgPattern is null || msgPattern.PartCount == 0)
            {
                return numberFormat.Format(numberObject);
            }

            // Get the appropriate sub-message.
            // Select it based on the formatted number-offset.
            double numberMinusOffset = number - offset;
            string numberString;
            if (offset == 0)
            {
                numberString = numberFormat.Format(numberObject);  // could be BigDecimal etc.
            }
            else
            {
                numberString = numberFormat.Format(numberMinusOffset);
            }
#pragma warning disable 612, 618
            IFixedDecimal dec;
            if (numberFormat is DecimalFormat decimalFormat)
            {
                dec = decimalFormat.GetFixedDecimal(numberMinusOffset);
            }
            else
            {
                dec = new FixedDecimal(numberMinusOffset);
#pragma warning restore 612, 618
            }
            int partIndex = FindSubMessage(msgPattern, 0, pluralRules, dec, number);
            // Replace syntactic # signs in the top level of this sub-message
            // (not in nested arguments) with the formatted number-offset.
            StringBuilder result = null;
            int prevIndex = msgPattern.GetPart(partIndex).Limit;
            for (; ; )
            {
                MessagePatternPart part = msgPattern.GetPart(++partIndex);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    if (result == null)
                    {
                        return pattern.Substring(prevIndex, index - prevIndex); // ICU4N: Corrected 2nd arg
                    }
                    else
                    {
                        return result.Append(pattern, prevIndex, index - prevIndex).ToString(); // ICU4N: Corrected 3rd arg
                    }
                }
                else if (type == MessagePatternPartType.ReplaceNumber ||
                          // JDK compatibility mode: Remove SKIP_SYNTAX.
                          (type == MessagePatternPartType.SkipSyntax && msgPattern.JdkAposMode))
                {
                    if (result == null)
                    {
                        result = new StringBuilder();
                    }
                    result.Append(pattern, prevIndex, index - prevIndex); // ICU4N: Corrected 3rd arg
                    if (type == MessagePatternPartType.ReplaceNumber)
                    {
                        result.Append(numberString);
                    }
                    prevIndex = part.Limit;
                }
                else if (type == MessagePatternPartType.ArgStart)
                {
                    if (result == null)
                    {
                        result = new StringBuilder();
                    }
                    result.Append(pattern, prevIndex, index - prevIndex); // ICU4N: Corrected 3rd arg
                    prevIndex = index;
                    partIndex = msgPattern.GetLimitPartIndex(partIndex);
                    index = msgPattern.GetPart(partIndex).Limit;
                    MessagePattern.AppendReducedApostrophes(pattern, prevIndex, index, result);
                    prevIndex = index;
                }
            }
        }

        private const int CharStackBufferSize = 128;

#nullable enable
        internal static unsafe bool TryFormat(double value, Span<char> destination, out int charsWritten, MessagePattern msgPattern, LocalizedNumberFormatter numberFormatter, PluralRules pluralRules)
        {
            string numberString;
            // If no pattern was applied, return the formatted number.
            if (msgPattern is null || msgPattern.PartCount == 0)
            {
                numberString = numberFormatter.Format(value).ToString();// ICU4N TODO: Pass in temp destination on stack
                bool success = numberString.TryCopyTo(destination);
                charsWritten = success ? numberString.Length : 0;
                return success;
            }

            double offset = msgPattern.GetPluralOffset(0);
            // Get the appropriate sub-message.
            // Select it based on the formatted number-offset.
            double numberMinusOffset = value - offset;

            if (offset == 0)
            {
                // ICU4N TODO: Pass in temp destination on stack
                numberString = numberFormatter.Format(value).ToString();  // could be BigDecimal etc.
            }
            else
            {
                // ICU4N TODO: Pass in temp destination on stack
                numberString = numberFormatter.Format(numberMinusOffset).ToString();
            }
#pragma warning disable 612, 618
            IFixedDecimal dec = numberFormatter.Format(numberMinusOffset).FixedDecimal;
#pragma warning restore 612, 618
            string pattern = msgPattern.PatternString;
            int partIndex = FindSubMessage(msgPattern, 0, pluralRules, dec, value);
            // Replace syntactic # signs in the top level of this sub-message
            // (not in nested arguments) with the formatted number-offset.
            char* stackPtr = stackalloc char[CharStackBufferSize];
            ValueStringBuilder result = new ValueStringBuilder(new Span<char>(stackPtr, CharStackBufferSize));
            try
            {
                int prevIndex = msgPattern.GetPart(partIndex).Limit;
                while (true)
                {
                    MessagePatternPart part = msgPattern.GetPart(++partIndex);
                    MessagePatternPartType type = part.Type;
                    int index = part.Index;
                    if (type == MessagePatternPartType.MsgLimit)
                    {
                        result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                        break;
                    }
                    else if (type == MessagePatternPartType.ReplaceNumber ||
                              // JDK compatibility mode: Remove SKIP_SYNTAX.
                              (type == MessagePatternPartType.SkipSyntax && msgPattern.JdkAposMode))
                    {
                        result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                        if (type == MessagePatternPartType.ReplaceNumber)
                        {
                            result.Append(numberString);
                        }
                        prevIndex = part.Limit;
                    }
                    else if (type == MessagePatternPartType.ArgStart)
                    {
                        result.Append(pattern.AsSpan(prevIndex, index - prevIndex)); // ICU4N: Corrected 2nd arg
                        prevIndex = index;
                        partIndex = msgPattern.GetLimitPartIndex(partIndex);
                        index = msgPattern.GetPart(partIndex).Limit;
                        MessagePattern.AppendReducedApostrophes(pattern, prevIndex, index, ref result);
                        prevIndex = index;
                    }
                }
                return result.TryCopyTo(destination, out charsWritten);
            }
            finally
            {
                result.Dispose();
            }
        }

#nullable restore

        /// <summary>
        /// This method is not yet supported by <see cref="PluralFormat"/>.
        /// </summary>
        /// <param name="text">the string to be parsed.</param>
        /// <param name="parsePosition">defines the position where parsing is to begin,
        /// and upon return, the position where parsing left off.  If the position
        /// has not changed upon return, then parsing failed.</param>
        /// <returns>nothing because this method is not yet implemented.</returns>
        /// <exception cref="NotSupportedException">will always be thrown by this method.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual J2N.Numerics.Number Parse(string text, ParsePosition parsePosition)
        {
            // You get number ranges from this. You can't get an exact number.
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not yet supported by <see cref="PluralFormat"/>.
        /// </summary>
        /// <param name="source">the string to be parsed.</param>
        /// <param name="pos">defines the position where parsing is to begin,
        /// and upon return, the position where parsing left off.  If the position
        /// has not changed upon return, then parsing failed.</param>
        /// <returns>nothing because this method is not yet implemented.</returns>
        /// <exception cref="NotSupportedException">will always be thrown by this method.</exception>
        /// <stable>ICU 3.8</stable>
        public override object ParseObject(string source, ParsePosition pos)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method returns the <see cref="PluralRules"/> type found from parsing.
        /// </summary>
        /// <param name="source">The string to be parsed.</param>
        /// <param name="scanner"></param>
        /// <param name="pos">Defines the position where parsing is to begin,
        /// and upon return, the position where parsing left off. If the position
        /// is a negative index, then parsing failed.
        /// </param>
        /// <returns>Returns the <see cref="PluralRules"/> type. For example,
        /// it could be "zero", "one", "two", "few", "many" or "other".</returns>
        /*package*/
#pragma warning disable CS0618 // Type or member is obsolete
        internal string ParseType(string source, IRbnfLenientScanner scanner, FieldPosition pos)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            // If no pattern was applied, return null.
            if (msgPattern == null || msgPattern.PartCount == 0)
            {
                pos.BeginIndex = -1;
                pos.EndIndex = -1;
                return null;
            }
            int partIndex = 0;
            int currMatchIndex;
            int count = msgPattern.PartCount;
            int startingAt = pos.BeginIndex;
            if (startingAt < 0)
            {
                startingAt = 0;
            }

            // The keyword is null until we need to match against a non-explicit, not-"other" value.
            // Then we get the keyword from the selector.
            // (In other words, we never call the selector if we match against an explicit value,
            // or if the only non-explicit keyword is "other".)
            string keyword = null;
            string matchedWord = null;
            int matchedIndex = -1;
            // Iterate over (ARG_SELECTOR ARG_START message ARG_LIMIT) tuples
            // until the end of the plural-only pattern.
            while (partIndex < count)
            {
                var partSelector = msgPattern.GetPart(partIndex++);
                if (partSelector.Type != MessagePatternPartType.ArgSelector)
                {
                    // Bad Format
                    continue;
                }

                var partStart = msgPattern.GetPart(partIndex++);
                if (partStart.Type != MessagePatternPartType.MsgStart)
                {
                    // Bad Format
                    continue;
                }

                var partLimit = msgPattern.GetPart(partIndex++);
                if (partLimit.Type != MessagePatternPartType.MsgLimit)
                {
                    // Bad Format
                    continue;
                }

                string currArg = pattern.Substring(partStart.Limit, partLimit.Index - partStart.Limit); // ICU4N: Corrected 2nd arg
                if (scanner != null)
                {
                    // If lenient parsing is turned ON, we've got some time consuming parsing ahead of us.
#pragma warning disable CS0618 // Type or member is obsolete
                    int[] scannerMatchResult = scanner.FindText(source, currArg, startingAt);
#pragma warning restore CS0618 // Type or member is obsolete
                    currMatchIndex = scannerMatchResult[0];
                }
                else
                {
                    currMatchIndex = source.IndexOf(currArg, startingAt);
                }
                if (currMatchIndex >= 0 && currMatchIndex >= matchedIndex && (matchedWord == null || currArg.Length > matchedWord.Length))
                {
                    matchedIndex = currMatchIndex;
                    matchedWord = currArg;
                    keyword = pattern.Substring(partStart.Limit, partLimit.Index - partStart.Limit); // ICU4N: Corrected 2nd arg
                }
            }
            if (keyword != null)
            {
                pos.BeginIndex = matchedIndex;
                pos.EndIndex = (matchedIndex + matchedWord.Length);
                return keyword;
            }

            // Not found!
            pos.BeginIndex = -1;
            pos.EndIndex = -1;
            return null;
        }

        /// <summary>
        /// Sets the locale used by this <see cref="PluralFormat"/> object.
        /// Note: Calling this method resets this <see cref="PluralFormat"/> object,
        /// i.e., a pattern that was applied previously will be removed,
        /// and the NumberFormat is set to the default number format for
        /// the locale.  The resulting format behaves the same as one
        /// constructed from <see cref="PluralFormat(UCultureInfo, PluralType)"/>
        /// with <see cref="PluralType.Cardinal"/>.
        /// </summary>
        /// <param name="ulocale">The <see cref="UCultureInfo"/> used to configure the
        /// formatter. If <paramref name="ulocale"/> is <c>null</c>, the
        /// default <see cref="UCultureInfo.CurrentCulture"/> locale will be used.
        /// </param>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        [Obsolete("ICU 50 This method clears the pattern and might create a different kind of " +
                "PluralRules instance; use one of the constructors to create a new instance instead.")]
        public virtual void SetCulture(UCultureInfo ulocale) // ICU4N TODO: API - In general, formatters in .NET should be unaware of the culture unless it is explictly passed to the Format() method. Need to rework this.
        {
            if (ulocale == null)
            {
                ulocale = UCultureInfo.CurrentCulture;
            }
            Init(null, PluralType.Cardinal, ulocale, null);
        }

        /// <summary>
        /// Sets the number format used by this formatter.  You only need to
        /// call this if you want a different number format than the default
        /// formatter for the locale.
        /// </summary>
        /// <param name="format">the number format to use.</param>
        /// <stable>ICU 3.8</stable>
        public virtual void SetNumberFormat(NumberFormat format)
        {
            numberFormat = format;
        }

        /// <stable>ICU 3.8</stable>
        public override bool Equals(object rhs)
        {
            if (this == rhs)
            {
                return true;
            }
            if (rhs == null || GetType() != rhs.GetType())
            {
                return false;
            }
            PluralFormat pf = (PluralFormat)rhs;
            return
                Utility.ObjectEquals(ulocale, pf.ulocale) &&
                Utility.ObjectEquals(pluralRules, pf.pluralRules) &&
                Utility.ObjectEquals(msgPattern, pf.msgPattern) &&
                Utility.ObjectEquals(numberFormat, pf.numberFormat);
        }

        /// <summary>
        /// Returns true if this equals the provided <see cref="PluralFormat"/>.
        /// </summary>
        /// <param name="rhs">the PluralFormat to compare against</param>
        /// <returns>true if this equals <paramref name="rhs"/></returns>
        /// <stable>ICU 3.8</stable>
        public virtual bool Equals(PluralFormat rhs)
        {
            return Equals((Object)rhs);
        }

        /// <stable>ICU 3.8</stable>
        public override int GetHashCode()
        {
            return pluralRules.GetHashCode() ^ parsedValues.GetHashCode();
        }

        /// <stable>ICU 3.8</stable>
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("locale=" + ulocale);
            buf.Append(", rules='" + pluralRules + "'");
            buf.Append(", pattern='" + pattern + "'");
            buf.Append(", format='" + numberFormat + "'");
            return buf.ToString();
        }

        private void ReadObject(Stream @in)
        {
            // ICU4N TODO: Object serialization
            //@in.defaultReadObject();
            // ICU4N: Factored out PluralSelectorAdapter by implementing IPluralSelector directly on PluralRules
            //pluralRulesWrapper = new PluralSelectorAdapter(pluralRules);
            // Ignore the parsedValues from an earlier class version (before ICU 4.8)
            // and rebuild the msgPattern.
            parsedValues = null;
            if (pattern != null)
            {
                ApplyPattern(pattern);
            }
        }
    }
}
