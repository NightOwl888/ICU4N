using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support.Collections;
using J2N;
using J2N.Collections.Generic.Extensions;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JCG = J2N.Collections.Generic;
using Number = J2N.Numerics.Number;
using Double = J2N.Numerics.Double;
using Integer = J2N.Numerics.Int32;
using Long = J2N.Numerics.Int64;
using J2N.Globalization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ICU4N.Util;
using System.Runtime.InteropServices;
using static ICU4N.Text.PluralFormat;
#if FEATURE_ARRAYPOOL
using System.Buffers;
#endif

namespace ICU4N.Text
{
    /// <summary>
    /// Provides a factory for returning plural rules
    /// </summary>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    internal abstract class PluralRulesFactory // ICU4N: Marked internal, since it is obsolete anyway
    {
        /// <summary>
        /// Sole constructor
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected PluralRulesFactory()
        {
        }

        /// <summary>
        /// Provides access to the predefined <see cref="PluralRules"/> for a given locale and the plural type.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>. For these predefined
        /// rules, see CLDR page at 
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale for which a <see cref="PluralRules"/> object is returned.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <returns>The predefined <see cref="PluralRules"/> object for this locale. If there's no predefined rules for
        /// this locale, the rules for the closest parent in the locale hierarchy that has one will be returned.
        /// The final fallback always returns the default rules.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public virtual PluralRules GetInstance(UCultureInfo locale, PluralType type)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale));

            return GetInstance(locale.Name, type);
        }

        /// <summary>
        /// Provides access to the predefined <see cref="PluralRules"/> for a given locale and the plural type.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>. For these predefined
        /// rules, see CLDR page at 
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="localeName">The locale for which a <see cref="PluralRules"/> object is returned.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <returns>The predefined <see cref="PluralRules"/> object for this locale. If there's no predefined rules for
        /// this locale, the rules for the closest parent in the locale hierarchy that has one will be returned.
        /// The final fallback always returns the default rules.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="localeName"/> is <c>null</c>.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")] // ICU4N: Added overload to allow looking up without tight coupling with UCultureInfo
        public abstract PluralRules GetInstance(string localeName, PluralType type);

        /// <summary>
        /// Utility for getting <see cref="PluralType.Cardinal"/> rules.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns>Plural rules.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public PluralRules GetInstance(UCultureInfo locale)
        {
            return GetInstance(locale, PluralType.Cardinal);
        }

        /// <summary>
        /// Utility for getting <see cref="PluralType.Cardinal"/> rules.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="localeName">The locale name.</param>
        /// <returns>Plural rules.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="localeName"/> is <c>null</c>.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")] // ICU4N: Added overload to allow looking up without tight coupling with UCultureInfo
        public PluralRules GetInstance(string localeName)
        {
            return GetInstance(localeName, PluralType.Cardinal);
        }

        /// <summary>
        /// Returns the locales for which there is plurals data.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public abstract UCultureInfo[] GetUCultures();

        /// <summary>
        /// Returns the 'functionally equivalent' locale with respect to plural rules. Calling <see cref="PluralRules.GetInstance(UCultureInfo)"/> with
        /// the functionally equivalent locale, and with the provided locale, returns rules that behave the same. 
        /// All locales with the same functionally equivalent locale have plural rules that behave the same. This is not
        /// exaustive; there may be other locales whose plural rules behave the same that do not have the same equivalent
        /// locale.
        /// </summary>
        /// <param name="locale">The locale to check.</param>
        /// <param name="isAvailable">if not null and of length &gt; 0, this will hold 'true' at index 0 if locale is directly defined
        /// (without fallback) as having plural rules</param>
        /// <returns>the functionally-equivalent locale</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public abstract UCultureInfo GetFunctionalEquivalent(UCultureInfo locale, out bool isAvailable);

        /// <summary>
        /// Returns the 'functionally equivalent' locale with respect to plural rules. Calling <see cref="PluralRules.GetInstance(UCultureInfo)"/> with
        /// the functionally equivalent locale, and with the provided locale, returns rules that behave the same. 
        /// All locales with the same functionally equivalent locale have plural rules that behave the same. This is not
        /// exaustive; there may be other locales whose plural rules behave the same that do not have the same equivalent
        /// locale.
        /// </summary>
        /// <param name="locale">The locale to check.</param>
        /// <returns>the functionally-equivalent locale</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public abstract UCultureInfo GetFunctionalEquivalent(UCultureInfo locale); // ICU4N specific - instead of passing null, we have a separate overload with no out value.

        /// <summary>
        /// Returns the default factory.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static PluralRulesLoader DefaultFactory => PluralRulesLoader.Loader;

        /// <summary>
        /// Returns whether or not there are overrides.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public abstract bool HasOverride(UCultureInfo locale);
    }

    /// <summary>
    /// Defines rules for mapping non-negative numeric values onto a small set of keywords.
    /// </summary>
    /// <remarks>
    /// Rules are constructed from a text description, consisting of a series of keywords and conditions. The <see cref="Select(double)"/>
    /// method examines each condition in order and returns the keyword for the first condition that matches the number. If
    /// none match, <see cref="KeywordOther"/> is returned.
    /// <para/>
    /// A <see cref="PluralRules"/> object is immutable. It contains caches for sample values, but those are synchronized.
    /// <para/>
    /// <see cref="PluralRules"/> is Serializable (except in .NET Standard 1.x) so that it can be used in formatters, which are serializable.
    /// <para/>
    /// For more information, details, and tips for writing rules, see the <a
    /// href="http://www.unicode.org/draft/reports/tr35/tr35.html#Language_Plural_Rules">LDML spec, C.11 Language Plural
    /// Rules</a>
    /// <para/>
    /// Examples:
    /// <para/>
    /// <code>
    /// &quot;one: n is 1; few: n in 2..4&quot;
    /// </code>
    /// This defines two rules, for 'one' and 'few'. The condition for 'one' is "n is 1" which means that the number must be
    /// equal to 1 for this condition to pass. The condition for 'few' is "n in 2..4" which means that the number must be
    /// between 2 and 4 inclusive - and be an integer - for this condition to pass. All other numbers are assigned the
    /// keyword "other" by the default rule.
    /// <para/>
    /// <code>
    /// &quot;zero: n is 0; one: n is 1; zero: n mod 100 in 1..19&quot;
    /// </code>
    /// This illustrates that the same keyword can be defined multiple times. Each rule is examined in order, and the first
    /// keyword whose condition passes is the one returned. Also notes that a modulus is applied to n in the last rule. Thus
    /// its condition holds for 119, 219, 319...
    /// <para/>
    /// <code>
    /// &quot;one: n is 1; few: n mod 10 in 2..4 and n mod 100 not in 12..14&quot;
    /// </code>
    /// This illustrates conjunction and negation. The condition for 'few' has two parts, both of which must be met:
    /// "n mod 10 in 2..4" and "n mod 100 not in 12..14". The first part applies a modulus to n before the test as in the
    /// previous example. The second part applies a different modulus and also uses negation, thus it matches all numbers
    /// _not_ in 12, 13, 14, 112, 113, 114, 212, 213, 214...
    /// <para/>
    /// Syntax:
    /// <para/>
    /// <code>
    /// rules         = rule (';' rule)*
    /// rule          = keyword ':' condition
    /// keyword       = &lt;identifier&gt;
    /// condition     = and_condition ('or' and_condition)*
    /// and_condition = relation ('and' relation)*
    /// relation      = not? expr not? rel not? range_list
    /// expr          = ('n' | 'i' | 'f' | 'v' | 't') (mod value)?
    /// not           = 'not' | '!'
    /// rel           = 'in' | 'is' | '=' | '≠' | 'within'
    /// mod           = 'mod' | '%'
    /// range_list    = (range | value) (',' range_list)*
    /// value         = digit+
    /// digit         = 0|1|2|3|4|5|6|7|8|9
    /// range         = value'..'value
    /// </code>
    /// <para/>
    /// Each <b>not</b> term inverts the meaning; however, there should not be more than one of them.
    /// <para/>
    /// The i, f, t, and v values are defined as follows:
    /// <list type="bullet">
    ///     <item><description>i to be the integer digits.</description></item>
    ///     <item><description>f to be the visible decimal digits, as an integer.</description></item>
    ///     <item><description>t to be the visible decimal digits—without trailing zeros—as an integer.</description></item>
    ///     <item><description>v to be the number of visible fraction digits.</description></item>
    ///     <item><description>j is defined to only match integers. That is j is 3 fails if v != 0 (eg for 3.1 or 3.0).</description></item>
    /// </list>
    /// <para/>
    /// Examples are in the following table:
    /// <list type="table">
    ///     <listheader>
    ///         <term>n</term>
    ///         <term>i</term>
    ///         <term>f</term>
    ///         <term>v</term>
    ///     </listheader>
    ///     <item>
    ///         <term>1.0</term>
    ///         <term>1</term>
    ///         <term>0</term>
    ///         <term>1</term>
    ///     </item>
    ///     <item>
    ///         <term>1.00</term>
    ///         <term>1</term>
    ///         <term>0</term>
    ///         <term>2</term>
    ///     </item>
    ///     <item>
    ///         <term>1.3</term>
    ///         <term>1</term>
    ///         <term>3</term>
    ///         <term>1</term>
    ///     </item>
    ///     <item>
    ///         <term>1.03</term>
    ///         <term>1</term>
    ///         <term>3</term>
    ///         <term>2</term>
    ///     </item>
    ///     <item>
    ///         <term>1.23</term>
    ///         <term>1</term>
    ///         <term>23</term>
    ///         <term>2</term>
    ///     </item>
    /// </list>
    /// <para/>
    /// An "identifier" is a sequence of characters that do not have the Unicode 
    /// <see cref="Globalization.UProperty.Pattern_Syntax"/> or <see cref="Globalization.UProperty.Pattern_White_Space"/>
    /// properties.
    /// <para/>
    /// The difference between 'in' and 'within' is that 'in' only includes integers in the specified range, while 'within'
    /// includes all values. Using 'within' with a range_list consisting entirely of values is the same as using 'in' (it's
    /// not an error).
    /// </remarks>
    /// <stable>ICU 3.8</stable>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class PluralRules : IPluralSelector
    {
#if !FEATURE_SPAN
        internal static readonly UnicodeSet ALLOWED_ID = new UnicodeSet("[a-z]").Freeze();
#endif

        // TODO Remove RulesList by moving its API and fields into PluralRules.
        /// <internal/>
        [Obsolete("This API is ICU internal only")]
        internal const string CategorySeparator = ";  "; // ICU4N: Marked internal since it is obsolete anyway

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal const string KeywordRuleSeparator = ": "; // ICU4N: Marked internal since it is obsolete anyway

        //private static readonly long serialVersionUID = 1;

        private readonly RuleList rules;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private readonly ICollection<string> keywords; // ICU4N: Changed from Set<T> to ICollection<T>

        // ICU4N specific - de-nested Factory and renamed PluralRulesFactory

        // Standard keywords.

        /// <summary>
        /// Common name for the 'zero' plural form.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordZero = "zero";

        /// <summary>
        /// Common name for the 'singular' plural form.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordOne = "one";

        /// <summary>
        /// Common name for the 'dual' plural form.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordTwo = "two";

        /// <summary>
        /// Common name for the 'paucal' or other special plural form.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordFew = "few";

        /// <summary>
        /// Common name for the arabic (11 to 99) plural form.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordMany = "many";

        /// <summary>
        /// Common name for the default plural form.  This name is returned
        /// for values to which no other form in the rule applies.  It
        /// can additionally be assigned rules of its own.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly string KeywordOther = "other";

        /// <summary>
        /// Value returned by <see cref="GetUniqueKeywordValue(string)"/>
        /// when there is no unique value to return.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public static readonly double NoUniqueValue = -0.00123456777;

        // ICU4N specific - de-nested PluralType

#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        private class NoConstraintClass : IConstraint
        {
            //private static readonly long serialVersionUID = 9163464945387899416L;
#pragma warning disable 612, 618
            public bool IsFulfilled(IFixedDecimal n)
            {
                return true;
            }

            public bool IsLimited(PluralRulesSampleType sampleType)
            {
                return false;
            }
#pragma warning restore 612, 618

            public override string ToString()
            {
                return "";
            }
        }

        /// <summary>
        /// The default constraint that is always satisfied.
        /// </summary>
        private static readonly IConstraint NO_CONSTRAINT = new NoConstraintClass();


        /// <summary>
        /// 
        /// </summary>
        private static readonly Rule DEFAULT_RULE = new Rule("other", NO_CONSTRAINT, null, null);

#if FEATURE_SPAN
        /// <summary>
        /// Parses a plural rules <paramref name="description"/> and returns a <see cref="PluralRules"/>.
        /// <para/>
        /// NOTE: Unlike the <see cref="ParseDescription(string)"/> overload, .NET will implicitly convert
        /// a <c>null</c> to an empty <see cref="ReadOnlySpan{T}"/>. <see cref="ReadOnlySpan{T}.Empty"/> is a
        /// valid choice, but <c>null</c> is not. As a result of passing <c>null</c>, this overload will return
        /// <see cref="Default"/> rather than throwing an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <exception cref="FormatException">If the <paramref name="description"/> cannot be parsed.
        /// The exception index is typically not set, it will be -1.</exception>
        /// <stable>ICU 3.8</stable>
        public static PluralRules ParseDescription(ReadOnlySpan<char> description)
        {
            ParseRuleStatus status = TryParseDescription(description, out PluralRules result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context);
            if (status != ParseRuleStatus.OK)
                ThrowParseException(status, source.ToString(), context.ToString());
            return result;
        }
#endif
        /// <summary>
        /// Parses a plural rules <paramref name="description"/> and returns a <see cref="PluralRules"/>.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <exception cref="FormatException">If the <paramref name="description"/> cannot be parsed.
        /// The exception index is typically not set, it will be -1.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="description"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.8</stable>
        public static PluralRules ParseDescription(string description)
        {
            if (description is null)
                throw new ArgumentNullException(nameof(description));

#if FEATURE_SPAN
            ParseRuleStatus status = TryParseDescription(description.AsSpan(), out PluralRules result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context);
            if (status != ParseRuleStatus.OK)
                ThrowParseException(status, source.ToString(), context.ToString());
#else
            ParseRuleStatus status = TryParseDescription(description, out PluralRules result, out string source, out string context);
            if (status != ParseRuleStatus.OK)
                ThrowParseException(status, source, context);
#endif
            return result;
        }

#if FEATURE_SPAN
        /// <summary>
        /// Parses a plural rules <paramref name="description"/> to its <see cref="PluralRules"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// <para/>
        /// NOTE: Unlike the <see cref="TryParseDescription(string, out PluralRules)"/> overload, .NET will implicitly convert
        /// a <c>null</c> to an empty <see cref="ReadOnlySpan{T}"/>. <see cref="ReadOnlySpan{T}.Empty"/> is a
        /// valid choice, but <c>null</c> is not. As a result of passing <c>null</c>, this overload will return
        /// <see cref="Default"/> rather than throwing an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <param name="result">When this method returns, contains the <see cref="PluralRules"/> equivalent
        /// of the value contained in <paramref name="description"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="description"/>
        /// parameter is <see cref="string.Empty"/> or is not of the correct format. This
        /// parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
        /// <exception cref="FormatException">If the <paramref name="description"/> cannot be parsed.
        /// The exception index is typically not set, it will be -1.</exception>
        /// <stable>ICU 3.8</stable>
        public static bool TryParseDescription(ReadOnlySpan<char> description, out PluralRules result)
        {
            ParseRuleStatus status = TryParseDescription(description, out result, out ReadOnlySpan<char> _, out ReadOnlySpan<char> _);
            return status == ParseRuleStatus.OK;
        }
#endif
        /// <summary>
        /// Parses a plural rules <paramref name="description"/> to its <see cref="PluralRules"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <param name="result">When this method returns, contains the <see cref="PluralRules"/> equivalent
        /// of the value contained in <paramref name="description"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="description"/>
        /// parameter is <c>null</c> or <see cref="string.Empty"/> or is not of the correct format. This
        /// parameter is passed uninitialized; any value originally supplied in result will be overwritten.</param>
        /// <exception cref="FormatException">If the <paramref name="description"/> cannot be parsed.
        /// The exception index is typically not set, it will be -1.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="description"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.8</stable>
        public static bool TryParseDescription(string description, out PluralRules result)
        {
            if (description is null)
                throw new ArgumentNullException(nameof(description));

#if FEATURE_SPAN
            ParseRuleStatus status = TryParseDescription(description.AsSpan(), out result, out ReadOnlySpan<char> _, out ReadOnlySpan<char> _);
#else
            ParseRuleStatus status = TryParseDescription(description, out result, out string _, out string _);
#endif
            return status == ParseRuleStatus.OK;
        }

#if FEATURE_SPAN
        internal static ParseRuleStatus TryParseDescription(ReadOnlySpan<char> description, out PluralRules result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context)
        {
            result = default;
            source = default;
            context = description;
            description = description.Trim();
            if (description.Length == 0)
            {
                result = Default;
                return ParseRuleStatus.OK;
            }
            ParseRuleStatus status = TryParseRuleChain(description, out RuleList ruleList, out source, out context);
            if (status != ParseRuleStatus.OK)
                return status;

            result = new PluralRules(ruleList);
            return ParseRuleStatus.OK;
        }
#else
        internal static ParseRuleStatus TryParseDescription(string description, out PluralRules result, out string source, out string context)
        {
            result = default;
            source = default;
            context = description;
            description = description.Trim();
            if (description.Length == 0)
            {
                result = Default;
                return ParseRuleStatus.OK;
            }
            ParseRuleStatus status = TryParseRuleChain(description, out RuleList ruleList, out source, out context);
            if (status != ParseRuleStatus.OK)
                return status;

            result = new PluralRules(ruleList);
            return ParseRuleStatus.OK;
        }
#endif

#if FEATURE_SPAN
        /// <summary>
        /// Creates a <see cref="PluralRules"/> from a <paramref name="description"/> if it is parsable,
        /// otherwise returns <c>null</c>.
        /// <para/>
        /// NOTE: Unlike the <see cref="CreateRules(string)"/> overload, .NET will implicitly convert
        /// a <c>null</c> to an empty <see cref="ReadOnlySpan{T}"/>. <see cref="ReadOnlySpan{T}.Empty"/> is a
        /// valid choice, but <c>null</c> is not. As a result of passing <c>null</c>, this overload will return
        /// <see cref="Default"/> rather than throwing an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <returns>The <see cref="PluralRules"/>.</returns>
        /// <stable>ICU 3.8</stable>
        public static PluralRules CreateRules(ReadOnlySpan<char> description)
        {
            return TryParseDescription(description, out PluralRules result) ? result : null;
        }
#endif

        /// <summary>
        /// Creates a <see cref="PluralRules"/> from a <paramref name="description"/> if it is parsable,
        /// otherwise returns <c>null</c>.
        /// </summary>
        /// <param name="description">The rule description.</param>
        /// <returns>The <see cref="PluralRules"/>.</returns>
        /// <stable>ICU 3.8</stable>
        public static PluralRules CreateRules(string description)
        {
            if (description is null)
                return null;

            return TryParseDescription(description, out PluralRules result) ? result : null;
        }


        /// <summary>
        /// The default rules that accept any number and return
        /// <see cref="KeywordOther"/>.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly PluralRules Default = new PluralRules(new RuleList().AddRule(DEFAULT_RULE).Finish());

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal enum Operand // ICU4N: Marked internal since it is obsolete anyway
        {
            /// <summary>
            /// The double value of the entire number.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            n,

            /// <summary>
            /// The integer value, with the fraction digits truncated off.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            i,

            /// <summary>
            /// All visible fraction digits as an integer, including trailing zeros.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            f,

            /// <summary>
            /// Visible fraction digits as an integer, not including trailing zeros.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            t,

            /// <summary>
            /// Number of visible fraction digits.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            v,

            /// <summary>
            /// Number of visible fraction digits, not including trailing zeros.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            w,

            /// <summary>
            /// THIS OPERAND IS DEPRECATED AND HAS BEEN REMOVED FROM THE SPEC.
            /// <para/>
            /// Returns the integer value, but will fail if the number has fraction digits.
            /// That is, using "j" instead of "i" is like implicitly adding "v is 0".
            /// <para/>
            /// For example, "j is 3" is equivalent to "i is 3 and v is 0": it matches
            /// "3" but not "3.1" or "3.0".
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            j
        }

#nullable enable
        internal static class OperandExtensions
        {
            /// <summary>
            /// Converts the string representation of the name or numeric value of one or more enumerated constants
            /// to an equivalent symbol. A parameter specifies whether the operation is case-sensitive. The return
            /// value indicates whether the conversion succeeded.
            /// </summary>
            /// <param name="value">The string representation of the enumeration name or underlying value to convert.</param>
            /// <param name="ignoreCase"><c>true</c> to ignore case; <c>false</c> to consider case.</param>
            /// <param name="result">When this method returns, contains an object of type <see cref="Operand"/> whose
            /// value is represented by value if the parse operation succeeds. If the parse operation fails, contains
            /// the default value of the underlying type of <see cref="Operand"/>. This parameter is passed uninitialized.</param>
            /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
            [Obsolete("This API is ICU internal only.")]
            public static bool TryParse(string? value, bool ignoreCase, out Operand result)
            {
                result = default;
                if (value is null || value.Length != 1)
                    return false;

                char val = value[0];

                if (ignoreCase)
                    val = char.ToLowerInvariant(val);

                Operand? temp = ConvertFromChar(val);
                if (temp.HasValue)
                {
                    result = temp.Value;
                    return true;
                }
                
                return false;
            }

#if FEATURE_SPAN
            /// <summary>
            /// Converts the string representation of the name or numeric value of one or more enumerated constants
            /// to an equivalent symbol. A parameter specifies whether the operation is case-sensitive. The return
            /// value indicates whether the conversion succeeded.
            /// </summary>
            /// <param name="value">The span representation of the enumeration name or underlying value to convert.</param>
            /// <param name="ignoreCase"><c>true</c> to ignore case; <c>false</c> to consider case.</param>
            /// <param name="result">When this method returns, contains an object of type <see cref="Operand"/> whose
            /// value is represented by value if the parse operation succeeds. If the parse operation fails, contains
            /// the default value of the underlying type of <see cref="Operand"/>. This parameter is passed uninitialized.</param>
            /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
            [Obsolete("This API is ICU internal only.")]
            public static bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out Operand result)
            {
                result = default;
                if (value.Length != 1)
                    return false;

                char val = value[0];

                if (ignoreCase)
                    val = char.ToLowerInvariant(val);

                Operand? temp = ConvertFromChar(val);
                if (temp.HasValue)
                {
                    result = temp.Value;
                    return true;
                }

                return false;
            }
#endif

            // since all of the symbols are only 1 char, we use a char data type.
            [Obsolete("This API is ICU internal only.")]
            private static Operand? ConvertFromChar(char value) => value switch
                {
                    'n' => Operand.n,
                    'i' => Operand.i,
                    'f' => Operand.f,
                    't' => Operand.t,
                    'v' => Operand.v,
                    'w' => Operand.w,
                    'j' => Operand.j,
                    _ => null,
                };
        }
#nullable restore


        /// <summary>
        /// An interface to FixedDecimal, allowing for other implementations.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal interface IFixedDecimal // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            /// <summary>
            /// Returns the value corresponding to the specified operand (n, i, f, t, v, or w).
            /// If the operand is 'n', returns a double; otherwise, returns an integer.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            double GetPluralOperand(Operand operand);

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            bool IsNaN { get; }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            bool IsInfinity { get; }
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal class FixedDecimal : Number, IComparable<FixedDecimal>, IFixedDecimal // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            //private static readonly long serialVersionUID = -4756200506571685661L;

            internal readonly double source;

            internal readonly int visibleDecimalDigitCount;

            internal readonly int visibleDecimalDigitCountWithoutTrailingZeros;

            internal readonly long decimalDigits;

            internal readonly long decimalDigitsWithoutTrailingZeros;

            internal readonly long integerValue;

            internal readonly bool hasIntegerValue;

            internal readonly bool isNegative;

            private readonly int baseFactor;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual double Source => source;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual int VisibleDecimalDigitCount => visibleDecimalDigitCount;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual int VisibleDecimalDigitCountWithoutTrailingZeros => visibleDecimalDigitCountWithoutTrailingZeros;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual long DecimalDigits => decimalDigits;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual long DecimalDigitsWithoutTrailingZeros => decimalDigitsWithoutTrailingZeros;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual long IntegerValue => integerValue;

            // ICU4N: HasIntegerValue defined below

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual bool IsNegative => isNegative;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual int BaseFactor => baseFactor;

            private const long MAX = (long)1E18;

            /// <param name="n">is the original number</param>
            /// <param name="v">number of digits to the right of the decimal place. e.g 1.00 = 2 25. = 0</param>
            /// <param name="f">Corresponds to f in the plural rules grammar.
            /// The digits to the right of the decimal place as an integer. e.g 1.10 = 10</param>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public FixedDecimal(double n, int v, long f)
            {
                isNegative = n < 0;
                source = isNegative ? -n : n;
                visibleDecimalDigitCount = v;
                decimalDigits = f;
                integerValue = n > MAX
                        ? MAX
                                : (long)n;
                hasIntegerValue = source == integerValue;
                // check values. TODO make into unit test.
                //
                //            long visiblePower = (int) Math.pow(10, v);
                //            if (fractionalDigits > visiblePower) {
                //                throw new IllegalArgumentException();
                //            }
                //            double fraction = intValue + (fractionalDigits / (double) visiblePower);
                //            if (fraction != source) {
                //                double diff = Math.abs(fraction - source)/(Math.abs(fraction) + Math.abs(source));
                //                if (diff > 0.00000001d) {
                //                    throw new IllegalArgumentException();
                //                }
                //            }
                if (f == 0)
                {
                    decimalDigitsWithoutTrailingZeros = 0;
                    visibleDecimalDigitCountWithoutTrailingZeros = 0;
                }
                else
                {
                    long fdwtz = f;
                    int trimmedCount = v;
                    while ((fdwtz % 10) == 0)
                    {
                        fdwtz /= 10;
                        --trimmedCount;
                    }
                    decimalDigitsWithoutTrailingZeros = fdwtz;
                    visibleDecimalDigitCountWithoutTrailingZeros = trimmedCount;
                }
                baseFactor = (int)Math.Pow(10, v);
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public FixedDecimal(double n, int v)
                : this(n, v, GetFractionalDigits(n, v))
            {
            }

            private static int GetFractionalDigits(double n, int v)
            {
                if (v == 0)
                {
                    return 0;
                }
                else
                {
                    if (n < 0)
                    {
                        n = -n;
                    }
                    int baseFactor = (int)Math.Pow(10, v);
                    long scaled = (long)Math.Round(n * baseFactor); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
                    return (int)(scaled % baseFactor);
                }
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public FixedDecimal(double n)
                : this(n, Decimals(n))
            {

            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public FixedDecimal(long n)
                : this(n, 0)
            {

            }

            private const long MAX_INTEGER_PART = 1000000000;

            /// <summary>
            /// Return a guess as to the number of decimals that would be displayed. This is only a guess; callers should
            /// always supply the decimals explicitly if possible. Currently, it is up to 6 decimals (without trailing zeros).
            /// Returns 0 for infinities and nans.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public static int Decimals(double n)
            {
                // Ugly...
                if (double.IsInfinity(n) || double.IsNaN(n))
                {
                    return 0;
                }
                if (n < 0)
                {
                    n = -n;
                }
                if (n == Math.Floor(n))
                {
                    return 0;
                }
                if (n < MAX_INTEGER_PART)
                {
                    long temp = (long)(n * 1000000) % 1000000; // get 6 decimals
                    for (int mask = 10, digits = 6; digits > 0; mask *= 10, --digits)
                    {
                        if ((temp % mask) != 0)
                        {
                            return digits;
                        }
                    }
                    return 0;
                }
                else
                {
                    string buf = n.ToString("e15", CultureInfo.InvariantCulture);
                    int ePos = buf.LastIndexOf('e');
                    int expNumPos = ePos + 1;
                    if (buf[expNumPos] == '+')
                    {
                        expNumPos++;
                    }
                    string exponentStr = buf.Substring(expNumPos);
                    int exponent = int.Parse(exponentStr, NumberStyles.Integer, CultureInfo.InvariantCulture); // Integer.parseInt(exponentStr);
                    int numFractionDigits = ePos - 2 - exponent;
                    if (numFractionDigits < 0)
                    {
                        return 0;
                    }
                    for (int i = ePos - 1; numFractionDigits > 0; --i)
                    {
                        if (buf[i] != '0')
                        {
                            break;
                        }
                        --numFractionDigits;
                    }
                    return numFractionDigits;
                }
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            // ICU4N: Replaced constructor to make parsing safe in .NET
            public static bool TryParse(string value, out FixedDecimal result) // Ugly, but for samples we don't care.
            {
                if (!Double.TryParse(value, NumberStyle.Float, CultureInfo.InvariantCulture, out double n))
                {
                    result = default;
                    return false;
                }
                int v = GetVisibleFractionCount(value);
                result = new FixedDecimal(n, v);
                return true;
            }

            private static int GetVisibleFractionCount(string value)
            {
                value = value.Trim();
                int decimalPos = value.IndexOf('.') + 1;
                if (decimalPos == 0)
                {
                    return 0;
                }
                else
                {
                    return value.Length - decimalPos;
                }
            }

#if FEATURE_SPAN
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            // ICU4N: Replaced constructor to make parsing safe
            public static bool TryParse(ReadOnlySpan<char> value, out FixedDecimal result) // Ugly, but for samples we don't care.
            {
                if (!Double.TryParse(value, NumberStyle.Float, CultureInfo.InvariantCulture, out double n))
                {
                    result = default;
                    return false;
                }
                int v = GetVisibleFractionCount(value);
                result = new FixedDecimal(n, v);
                return true;
            }

            private static int GetVisibleFractionCount(ReadOnlySpan<char> value)
            {
                value = value.Trim();
                int decimalPos = value.IndexOf('.') + 1;
                if (decimalPos == 0)
                {
                    return 0;
                }
                else
                {
                    return value.Length - decimalPos;
                }
            }
#endif

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual double GetPluralOperand(Operand operand)
            {
                switch (operand)
                {
                    case Operand.n: return source;
                    case Operand.i: return integerValue;
                    case Operand.f: return decimalDigits;
                    case Operand.t: return decimalDigitsWithoutTrailingZeros;
                    case Operand.v: return visibleDecimalDigitCount;
                    case Operand.w: return visibleDecimalDigitCountWithoutTrailingZeros;
                    default: return source;
                }
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public static Operand GetOperand(string value)
            {
                return (Operand)Enum.Parse(typeof(Operand), value, true);
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public static bool TryGetOperand(string value, out Operand result) // ICU4N: Added to eliminate exceptions
            {
                return OperandExtensions.TryParse(value, ignoreCase: true, out result);
            }

#if FEATURE_SPAN
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public static bool TryGetOperand(ReadOnlySpan<char> value, out Operand result) // ICU4N: Added to eliminate exceptions
            {
                return OperandExtensions.TryParse(value, ignoreCase: true, out result);
            }
#endif

            /// <summary>
            /// We're not going to care about NaN.
            /// </summary>
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual int CompareTo(FixedDecimal other)
            {
                if (integerValue != other.integerValue)
                {
                    return integerValue < other.integerValue ? -1 : 1;
                }
                if (source != other.source)
                {
                    return source < other.source ? -1 : 1;
                }
                if (visibleDecimalDigitCount != other.visibleDecimalDigitCount)
                {
                    return visibleDecimalDigitCount < other.visibleDecimalDigitCount ? -1 : 1;
                }
                long diff = decimalDigits - other.decimalDigits;
                if (diff != 0)
                {
                    return diff < 0 ? -1 : 1;
                }
                return 0;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override bool Equals(object arg0)
#pragma warning restore 809
            {
                if (arg0 == null)
                {
                    return false;
                }
                if (arg0 == this)
                {
                    return true;
                }
                if (!(arg0 is FixedDecimal))
                {
                    return false;
                }
                FixedDecimal other = (FixedDecimal)arg0;
                return source == other.source && visibleDecimalDigitCount == other.visibleDecimalDigitCount && decimalDigits == other.decimalDigits;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override int GetHashCode()
#pragma warning restore 809
            {
                // TODO Auto-generated method stub
                return (int)(decimalDigits + 37 * (visibleDecimalDigitCount + (int)(37 * source)));
            }

#if FEATURE_SPAN
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual ReadOnlySpan<char> AsSpan() // ICU4N: Added to patch platforms that don't implicitly convert string to ReadOnlySpan<char>
            {
                return ToString()
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
                    .AsSpan()
#endif
                    ;
            }
#endif

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override string ToString()
#pragma warning restore 809
            {
                return source.ToString("f" + visibleDecimalDigitCount.ToString(CultureInfo.InvariantCulture));
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
            public override string ToString(string format, IFormatProvider provider)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
            {
                return ToString(); // We don't allow any customization - this is a "fixed" decimal with a set number of places
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual bool HasIntegerValue => hasIntegerValue;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            // TODO Auto-generated method stub
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
            public override int ToInt32() => Convert.ToInt32(integerValue); // ICU4N specific - renamed from intValue()

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public override long ToInt64() => integerValue; // ICU4N specific - renamed from longValue()

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public override float ToSingle() => Convert.ToSingle(source); // ICU4N specific - renamed from floatValue()

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public override double ToDouble() => isNegative ? -source : source; // ICU4N specific - renamed from doubleValue()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual long GetShiftedValue()
            {
                return integerValue * baseFactor + decimalDigits;
            }

            private void WriteObject(
                    Stream @out)
            {
                //throw new NotSerializableException();
                throw new Exception(); // ICU4N TODO: Not sure what exception to throw here
            }

            private void ReadObject(Stream @in
                    )
            {
                //throw new NotSerializableException();
                throw new Exception(); // ICU4N TODO: Not sure what exception to throw here
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual bool IsNaN => double.IsNaN(source);

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public virtual bool IsInfinity => double.IsInfinity(source); // ICU4N specific - renamed from IsInfinite
        }

        // ICU4N specific - de-nested SampleType enum and renamed PluralRulesSampleType

        /// <summary>
        /// A range of NumberInfo that includes all values with the same visibleFractionDigitCount.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal class FixedDecimalRange // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public readonly FixedDecimal start;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public readonly FixedDecimal end;

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            public FixedDecimalRange(FixedDecimal start, FixedDecimal end)
            {
                if (start.VisibleDecimalDigitCount != end.VisibleDecimalDigitCount)
                {
                    throw new ArgumentException("Ranges must have the same number of visible decimals: " + start + "~" + end);
                }
                this.start = start;
                this.end = end;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override string ToString()
#pragma warning restore 809
            {
                return start + (end == start ? "" : "~" + end);
            }
        }

        /// <summary>
        /// A list of NumberInfo that includes all values with the same visibleFractionDigitCount.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        internal class FixedDecimalSamples // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            internal readonly PluralRulesSampleType sampleType; // ICU4N: Marked internal since it is obsolete anyway

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            private readonly ICollection<FixedDecimalRange> samples; // ICU4N specific - made private because this is already exposed through the Samples property

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            internal readonly bool bounded; // ICU4N: Marked internal since it is obsolete anyway

            /// <summary>
            /// The samples must be immutable.
            /// </summary>
            private FixedDecimalSamples(PluralRulesSampleType sampleType, ICollection<FixedDecimalRange> samples, bool bounded)
            {
                this.sampleType = sampleType;
                this.samples = samples;
                this.bounded = bounded;
            }

#if FEATURE_SPAN
            /// <summary>
            /// Parse a list of the form described in CLDR. The <paramref name="text"/> must be trimmed.
            /// </summary>
            internal static ParseRuleStatus TryParse(ReadOnlySpan<char> text, out FixedDecimalSamples result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context)
            {
                result = default;
                source = default;
                context = text;
                PluralRulesSampleType sampleType2;
                bool bounded2 = true;
                bool haveBound = false;
                ISet<FixedDecimalRange> samples2 = new JCG.LinkedHashSet<FixedDecimalRange>();

                if (text.StartsWith("integer", StringComparison.Ordinal))
                {
                    sampleType2 = PluralRulesSampleType.Integer;
                }
                else if (text.StartsWith("decimal", StringComparison.Ordinal))
                {
                    sampleType2 = PluralRulesSampleType.Decimal;
                }
                else
                {
                    return ParseRuleStatus.SamplesMustStartWithIntOrDec;
                }
                text = text.Slice(7); // remove both

                foreach (var range in text.AsTokens(',', SplitTokenizerEnumerator.PatternWhiteSpace))
                {
                    // ICU4N specific - Ensure we remove any empty entries to match Java Pattern.
                    // SplitTokenizerEnumerator trims whitespace automatically, so they will always be empty.
                    if (range.Text.IsEmpty)
                    {
                        continue;
                    }

                    if (range.Text.Equals("…", StringComparison.Ordinal) || range.Text.Equals("...", StringComparison.Ordinal))
                    {
                        bounded2 = false;
                        haveBound = true;
                        continue;
                    }
                    if (haveBound)
                    {
                        source = range;
                        return ParseRuleStatus.MisplacedEndRangeBound;
                    }
                    SplitTokenizerEnumerator rangePartEnumerator = range.Text.AsTokens('~', SplitTokenizerEnumerator.PatternWhiteSpace);
                    // ICU4N: Check to ensure there are exactly 1 or 2 parts to the range
                    ReadOnlySpan<char> rangePart0 = rangePartEnumerator.MoveNext() ? rangePartEnumerator.Current : ReadOnlySpan<char>.Empty;
                    ReadOnlySpan<char> rangePart1 = rangePartEnumerator.MoveNext() ? rangePartEnumerator.Current : ReadOnlySpan<char>.Empty;
                    if (rangePart0.IsEmpty || rangePartEnumerator.MoveNext())
                    {
                        source = range;
                        return ParseRuleStatus.IllformedNumberRange;
                    }

                    if (rangePart1.IsEmpty) // 1 range part
                    {
                        if (!FixedDecimal.TryParse(rangePart0, out FixedDecimal sample))
                        {
                            source = rangePart0;
                            return ParseRuleStatus.RangeBoundMustBeFloat;
                        }
                        if (!TryCheckDecimal(sampleType2, sample)) // ICU4N TODO: This can never fail - it should be an assert rather than an error.
                        {
                            source = sample.AsSpan();
                            return ParseRuleStatus.IllformedNumberRange;
                        }
                        samples2.Add(new FixedDecimalRange(sample, sample));
                    }
                    else // 2 range parts
                    {
                        if (!FixedDecimal.TryParse(rangePart0, out FixedDecimal start))
                        {
                            source = rangePart0;
                            return ParseRuleStatus.RangeBoundMustBeFloat;
                        }
                        if (!FixedDecimal.TryParse(rangePart1, out FixedDecimal end))
                        {
                            source = rangePart1;
                            return ParseRuleStatus.RangeBoundMustBeFloat;
                        }
                        if (!TryCheckDecimal(sampleType2, start)) // ICU4N TODO: This can never fail - it should be an assert rather than an error.
                        {
                            source = start.AsSpan();
                            return ParseRuleStatus.IllformedNumberRange;
                        }
                        if (!TryCheckDecimal(sampleType2, end)) // ICU4N TODO: This can never fail - it should be an assert rather than an error.
                        {
                            source = end.AsSpan();
                            return ParseRuleStatus.IllformedNumberRange;
                        }
                        samples2.Add(new FixedDecimalRange(start, end));
                    }
                }
                result = new FixedDecimalSamples(sampleType2, samples2.AsReadOnly(), bounded2);
                return ParseRuleStatus.OK;
            }
#else
            /// <summary>
            /// Parse a list of the form described in CLDR. The <paramref name="source"/> must be trimmed.
            /// </summary>
            internal static ParseRuleStatus TryParse(string text, out FixedDecimalSamples result, out string source, out string context)
            {
                result = default;
                source = default;
                context = text;
                PluralRulesSampleType sampleType2;
                bool bounded2 = true;
                bool haveBound = false;
                ISet<FixedDecimalRange> samples2 = new JCG.LinkedHashSet<FixedDecimalRange>();

                if (text.StartsWith("integer", StringComparison.Ordinal))
                {
                    sampleType2 = PluralRulesSampleType.Integer;
                }
                else if (text.StartsWith("decimal", StringComparison.Ordinal))
                {
                    sampleType2 = PluralRulesSampleType.Decimal;
                }
                else
                {
                    return ParseRuleStatus.SamplesMustStartWithIntOrDec;
                }
                text = text.Substring(7).Trim(); // remove both

                foreach (string range in COMMA_SEPARATED.Split(text))
                {
                    // ICU4N specific - .NET Split() doesn't remove empty entries
                    // from the end of the array. So, we skip them here.
                    if (string.IsNullOrWhiteSpace(range))
                    {
                        continue;
                    }

                    if (range.Equals("…", StringComparison.Ordinal) || range.Equals("...", StringComparison.Ordinal))
                    {
                        bounded2 = false;
                        haveBound = true;
                        continue;
                    }
                    if (haveBound)
                    {
                        source = range;
                        return ParseRuleStatus.MisplacedEndRangeBound;
                    }
                    string[] rangeParts = TILDE_SEPARATED.Split(range);
                    switch (rangeParts.Length)
                    {
                        case 1:
                            if (!FixedDecimal.TryParse(rangeParts[0], out FixedDecimal sample))
                            {
                                source = rangeParts[0];
                                return ParseRuleStatus.RangeBoundMustBeFloat;
                            }
                            if (!TryCheckDecimal(sampleType2, sample))
                            {
                                source = sample.ToString();
                                return ParseRuleStatus.IllformedNumberRange;
                            }
                            samples2.Add(new FixedDecimalRange(sample, sample));
                            break;
                        case 2:
                            if (!FixedDecimal.TryParse(rangeParts[0], out FixedDecimal start))
                            {
                                source = rangeParts[0];
                                return ParseRuleStatus.RangeBoundMustBeFloat;
                            }
                            if (!FixedDecimal.TryParse(rangeParts[1], out FixedDecimal end))
                            {
                                source = rangeParts[1];
                                return ParseRuleStatus.RangeBoundMustBeFloat;
                            }
                            if (!TryCheckDecimal(sampleType2, start))
                            {
                                source = start.ToString();
                                return ParseRuleStatus.IllformedNumberRange;
                            }
                            if (!TryCheckDecimal(sampleType2, end))
                            {
                                source = end.ToString();
                                return ParseRuleStatus.IllformedNumberRange;
                            }
                            samples2.Add(new FixedDecimalRange(start, end));
                            break;
                        default:
                            source = range;
                            return ParseRuleStatus.IllformedNumberRange;
                    }
                }
                result =  new FixedDecimalSamples(sampleType2, samples2.AsReadOnly(), bounded2);
                return ParseRuleStatus.OK;
            }
#endif

            private static bool TryCheckDecimal(PluralRulesSampleType sampleType2, FixedDecimal sample)
            {
                if ((sampleType2 == PluralRulesSampleType.Integer) != (sample.VisibleDecimalDigitCount == 0))
                {
                    return false;
                }
                return true;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            internal virtual ISet<double> AddSamples(ISet<double> result) // ICU4N: Marked internal since it is obsolete anyway
            {
                foreach (FixedDecimalRange item in samples)
                {
                    // we have to convert to longs so we don't get strange double issues
                    long startDouble = item.start.GetShiftedValue();
                    long endDouble = item.end.GetShiftedValue();

                    for (long d = startDouble; d <= endDouble; d += 1)
                    {
                        result.Add(d / (double)item.start.BaseFactor);
                    }
                }
                return result;
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override string ToString()
#pragma warning restore 809
            {
                StringBuilder b = new StringBuilder("@").Append(sampleType.ToString().ToLowerInvariant());
                bool first = true;
                foreach (FixedDecimalRange item in samples)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        b.Append(",");
                    }
                    b.Append(' ').Append(item);
                }
                if (!bounded)
                {
                    b.Append(", …");
                }
                return b.ToString();
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            internal virtual ICollection<FixedDecimalRange> Samples => samples; // ICU4N: Marked internal since it is obsolete anyway

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
            internal virtual void GetStartEndSamples(ICollection<FixedDecimal> target) // ICU4N: Marked internal since it is obsolete anyway
            {
                foreach (FixedDecimalRange item in samples)
                {
                    target.Add(item.start);
                    target.Add(item.end);
                }
            }
        }

        /// <summary>
        /// A constraint on a number.
        /// </summary>
        internal interface IConstraint
        {
#pragma warning disable 612, 618
            /// <summary>
            /// Returns true if the number fulfills the constraint.
            /// </summary>
            /// <param name="n">the number to test, &gt;= 0.</param>
            /// <returns></returns>
            bool IsFulfilled(IFixedDecimal n);

            /// <summary>
            /// Returns false if an unlimited number of values fulfills the
            /// constraint.
            /// </summary>
            bool IsLimited(PluralRulesSampleType sampleType);
#pragma warning restore 612, 618
        }

        internal class SimpleTokenizer
        {
            private static readonly UnicodeSet BREAK_AND_IGNORE = new UnicodeSet(0x09, 0x0a, 0x0c, 0x0d, 0x20, 0x20).Freeze();
            private static readonly UnicodeSet BREAK_AND_KEEP = new UnicodeSet('!', '!', '%', '%', ',', ',', '.', '.', '=', '=').Freeze();
            internal static string[] Split(string source)
            {
                int last = -1;
                List<string> result = new List<string>();
                for (int i = 0; i < source.Length; ++i)
                {
                    char ch = source[i];
                    if (BREAK_AND_IGNORE.Contains(ch))
                    {
                        if (last >= 0)
                        {
                            result.Add(source.Substring(last, i - last)); // ICU4N:: Corrected 2nd arg
                            last = -1;
                        }
                    }
                    else if (BREAK_AND_KEEP.Contains(ch))
                    {
                        if (last >= 0)
                        {
                            result.Add(source.Substring(last, i - last)); // ICU4N:: Corrected 2nd arg
                        }
                        result.Add(source.Substring(i, 1)); // ICU4N:: Corrected 2nd arg
                        last = -1;
                    }
                    else if (last < 0)
                    {
                        last = i;
                    }
                }
                if (last >= 0)
                {
                    result.Add(source.Substring(last));
                }
                return result.ToArray();
            }
        }

#if FEATURE_SPAN
        internal ref struct SimpleTokenizerEnumerator
        {
            private static readonly UnicodeSet BreakAndIgnore = new UnicodeSet(0x09, 0x0a, 0x0c, 0x0d, 0x20, 0x20).Freeze();
            private static readonly UnicodeSet BreakAndKeep = new UnicodeSet('!', '!', '%', '%', ',', ',', '.', '.', '=', '=').Freeze();

            private ReadOnlySpan<char> source;

            public SimpleTokenizerEnumerator(ReadOnlySpan<char> source)
            {
                this.source = source;
                Current = default;
            }

            public SimpleTokenizerEnumerator(string source)
                : this(source.AsSpan())
            {
            }

            // Needed to be compatible with the foreach operator
            public SimpleTokenizerEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                int last = -1;
                var span = source;
                if (span.Length == 0) // Reached the end of the string
                    return false;

                for (int i = 0; i < source.Length; ++i)
                {
                    char ch = span[i];
                    bool ignore = BreakAndIgnore.Contains(ch);
                    if (ignore || BreakAndKeep.Contains(ch))
                    {
                        if (last >= 0)
                        {
                            Current = Trim(span.Slice(last, i - last));
                            source = span.Slice(i);
                            return true;
                        }
                        if (!ignore)
                        {
                            Current = span.Slice(i, 1);
                            source = span.Slice(i + 1);
                            return true;
                        }
                    }
                    else if (last < 0)
                    {
                        last = i;
                    }
                }
                if (last >= 0)
                {
                    source = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                    Current = Trim(span.Slice(last));
                }
                return true;
            }

            private static ReadOnlySpan<char> Trim(ReadOnlySpan<char> text)
            {
                if (text.Length == 0)
                    return text;

                
                int start = 0;
                int end = text.Length;

                // Trim start
                while (BreakAndIgnore.Contains(text[start]))
                {
                    start++;
                }

                // Trim end
                while (BreakAndIgnore.Contains(text[end - 1]))
                {
                    end--;
                }

                if (start == 0 && end == text.Length)
                    return text;

                return text.Slice(start, end - start);
            }

            public ReadOnlySpan<char> Current { get; private set; }

            public bool HasNext => source.Length > 0;
        }

        /*
         * syntax:
         * condition :       or_condition
         *                   and_condition
         * or_condition :    and_condition 'or' condition
         * and_condition :   relation
         *                   relation 'and' relation
         * relation :        in_relation
         *                   within_relation
         * in_relation :     not? expr not? in not? range
         * within_relation : not? expr not? 'within' not? range
         * not :             'not'
         *                   '!'
         * expr :            'n'
         *                   'n' mod value
         * mod :             'mod'
         *                   '%'
         * in :              'in'
         *                   'is'
         *                   '='
         *                   '≠'
         * value :           digit+
         * digit :           0|1|2|3|4|5|6|7|8|9
         * range :           value'..'value
         */
#pragma warning disable CS9091 // This returns local 'enumerator' by reference but it is not a ref local
        private unsafe static ParseRuleStatus TryParseConstraint(ReadOnlySpan<char> description, out IConstraint result, out ReadOnlySpan<char> token, out ReadOnlySpan<char> condition)
        {
            result = default;
            token = null;
            condition = null;
            foreach (var or_together_token in description.AsTokens("or", SplitTokenizerEnumerator.PatternWhiteSpace))
            {
                IConstraint andConstraint = null;
                foreach (var and_together_token in or_together_token.Text.AsTokens("and", SplitTokenizerEnumerator.PatternWhiteSpace))
                {
                    IConstraint newConstraint = NO_CONSTRAINT;

                    condition = and_together_token; // ICU4N: Already trimmed
                    var enumerator = new SimpleTokenizerEnumerator(condition);

                    int mod = 0;
                    bool inRange = true;
                    bool integersOnly = true;
                    double lowBound = long.MaxValue;
                    double highBound = long.MinValue;
                    long[] vals;
                    bool hackForCompatibility = false;
#pragma warning disable 612, 618
                    // ICU4N: Added check for empty string
                    if (!TryGetNextToken(ref enumerator, out token) || !FixedDecimal.TryGetOperand(token, out Operand operand))
                    {
                        return ParseRuleStatus.ConstraintInvalidOperand;
                    }
#pragma warning restore 612, 618
                    if (TryGetNextToken(ref enumerator, out token))
                    {
                        if ((token.Equals("mod", StringComparison.Ordinal) || token.Equals("%", StringComparison.Ordinal)) && TryGetNextToken(ref enumerator, out token))
                        {
                            // ICU4N NOTE: This overload allows non-ASCII digits
                            if (!Integer.TryParse(token, radix: 10, out mod))
                            {
                                return ParseRuleStatus.ConstraintModulusMustBeDigits;
                            }
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        if (token.Equals("not", StringComparison.Ordinal))
                        {
                            inRange = !inRange;
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                            if (token.Equals("=", StringComparison.Ordinal))
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                        }
                        else if (token.Equals("!", StringComparison.Ordinal))
                        {
                            inRange = !inRange;
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                            if (!token.Equals("=", StringComparison.Ordinal)) // ICU4N NOTE: This is the inverse of "not" above
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                        }
                        if (token.Equals("is", StringComparison.Ordinal) || token.Equals("in", StringComparison.Ordinal) || token.Equals("=", StringComparison.Ordinal))
                        {
                            hackForCompatibility = token.Equals("is", StringComparison.Ordinal);
                            if (hackForCompatibility && !inRange)
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        else if (token.Equals("within", StringComparison.Ordinal))
                        {
                            integersOnly = false;
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        else
                        {
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }
                        if (token.Equals("not", StringComparison.Ordinal))
                        {
                            if (!hackForCompatibility && !inRange)
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            inRange = !inRange;
                            if (!TryGetNextToken(ref enumerator, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }

                        List<long> valueList = new List<long>();

                        // the token is always one item ahead
                        while (true)
                        {
                            // ICU4N NOTE: This overload allows non-ASCII digits
                            if (!Long.TryParse(token, radix: 10, out long low))
                            {
                                return ParseRuleStatus.ConstraintValueMustBeDigits;
                            }
                            long high = low;
                            if (enumerator.HasNext) // ICU4N: Don't advance the enumerator if we are at the end so we can do after work
                            {
                                TryGetNextToken(ref enumerator, out token);
                                if (token.Equals(".", StringComparison.Ordinal))
                                {
                                    if (!TryGetNextToken(ref enumerator, out token))
                                    {
                                        return ParseRuleStatus.ConstraintMissingToken;
                                    }
                                    if (!token.Equals(".", StringComparison.Ordinal))
                                    {
                                        return ParseRuleStatus.ConstraintUnexpectedToken;
                                    }
                                    if (!TryGetNextToken(ref enumerator, out token))
                                    {
                                        return ParseRuleStatus.ConstraintMissingToken;
                                    }
                                    // ICU4N NOTE: This overload allows non-ASCII digits
                                    if (!Long.TryParse(token, radix: 10, out high))
                                    {
                                        return ParseRuleStatus.ConstraintValueMustBeDigits;
                                    }
                                    if (enumerator.HasNext)
                                    {
                                        TryGetNextToken(ref enumerator, out token);
                                        if (!token.Equals(",", StringComparison.Ordinal))
                                        {
                                            // adjacent number: 1 2
                                            // no separator, fail
                                            return ParseRuleStatus.ConstraintUnexpectedToken;
                                        }
                                    }
                                }
                                else if (!token.Equals(",", StringComparison.Ordinal))
                                {
                                    // adjacent number: 1 2
                                    // no separator, fail
                                    return ParseRuleStatus.ConstraintUnexpectedToken;
                                }
                            }
                            // at this point, either we are out of tokens, or t is ','
                            if (low > high)
                            {
                                token = string.Concat(low.ToString(CultureInfo.InvariantCulture), "~", high.ToString(CultureInfo.InvariantCulture)).AsSpan();
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            else if (mod != 0 && high >= mod)
                            {
                                token = string.Concat(low.ToString(CultureInfo.InvariantCulture), ">mod=", mod.ToString(CultureInfo.InvariantCulture)).AsSpan();
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            valueList.Add(low);
                            valueList.Add(high);
                            lowBound = Math.Min(lowBound, low);
                            highBound = Math.Max(highBound, high);
                            if (!enumerator.HasNext) // ICU4N: Don't advance the enumerator if we are at the end so we can do after work
                            {
                                break;
                            }
                            TryGetNextToken(ref enumerator, out token);
                        }

                        if (token.Equals(",", StringComparison.Ordinal))
                        {
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }

                        if (valueList.Count == 2)
                        {
                            vals = null;
                        }
                        else
                        {
                            vals = valueList.ToArray();
                        }

                        // Hack to exclude "is not 1,2"
                        if (lowBound != highBound && hackForCompatibility && !inRange)
                        {
                            token = "is not <range>"
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
                                .AsSpan()
#endif
                                ;
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }

                        newConstraint = new RangeConstraint(mod, inRange, operand, integersOnly, lowBound, highBound, vals);
                    }

                    if (andConstraint is null)
                    {
                        andConstraint = newConstraint;
                    }
                    else
                    {
                        andConstraint = new AndConstraint(andConstraint, newConstraint);
                    }
                }

                if (result is null)
                {
                    result = andConstraint;
                }
                else
                {
                    result = new OrConstraint(result, andConstraint);
                }
            }
            return ParseRuleStatus.OK;
        }
#pragma warning restore CS9091 // This returns local 'enumerator' by reference but it is not a ref local

#else

        /*
        * syntax:
        * condition :       or_condition
        *                   and_condition
        * or_condition :    and_condition 'or' condition
        * and_condition :   relation
        *                   relation 'and' relation
        * relation :        in_relation
        *                   within_relation
        * in_relation :     not? expr not? in not? range
        * within_relation : not? expr not? 'within' not? range
        * not :             'not'
        *                   '!'
        * expr :            'n'
        *                   'n' mod value
        * mod :             'mod'
        *                   '%'
        * in :              'in'
        *                   'is'
        *                   '='
        *                   '≠'
        * value :           digit+
        * digit :           0|1|2|3|4|5|6|7|8|9
        * range :           value'..'value
        */
        private static ParseRuleStatus TryParseConstraint(string description, out IConstraint result, out string token, out string condition)
        {
            result = default;
            token = null;
            condition = null;
            string[] or_together = OR_SEPARATED.Split(description);
            for (int i = 0; i < or_together.Length; ++i)
            {
                IConstraint andConstraint = null;
                string[] and_together = AND_SEPARATED.Split(or_together[i]);
                for (int j = 0; j < and_together.Length; ++j)
                {
                    IConstraint newConstraint = NO_CONSTRAINT;

                    condition = and_together[j].Trim();
                    string[] tokens = SimpleTokenizer.Split(condition);

                    int mod = 0;
                    bool inRange = true;
                    bool integersOnly = true;
                    double lowBound = long.MaxValue;
                    double highBound = long.MinValue;
                    long[] vals;

                    int x = 0;
                    token = tokens[x++];
                    bool hackForCompatibility = false;
#pragma warning disable 612, 618
                    if (!FixedDecimal.TryGetOperand(token, out Operand operand))
                    {
                        return ParseRuleStatus.ConstraintInvalidOperand;
                    }
#pragma warning restore 612, 618
                    if (x < tokens.Length)
                    {
                        token = tokens[x++];
                        if (("mod".Equals(token, StringComparison.Ordinal) || "%".Equals(token, StringComparison.Ordinal)) && TryGetNextToken(tokens, ref x, out token))
                        {
                            // ICU4N NOTE: This overload allows non-ASCII digits
                            if (!Integer.TryParse(token, radix: 10, out mod))
                            {
                                return ParseRuleStatus.ConstraintModulusMustBeDigits;
                            }
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        if ("not".Equals(token, StringComparison.Ordinal))
                        {
                            inRange = !inRange;
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                            if ("=".Equals(token, StringComparison.Ordinal))
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                        }
                        else if ("!".Equals(token))
                        {
                            inRange = !inRange;
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                            if (!"=".Equals(token, StringComparison.Ordinal)) // ICU4N NOTE: This is the inverse of "not" above
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                        }
                        if ("is".Equals(token, StringComparison.Ordinal) || "in".Equals(token, StringComparison.Ordinal) || "=".Equals(token, StringComparison.Ordinal))
                        {
                            hackForCompatibility = "is".Equals(token);
                            if (hackForCompatibility && !inRange)
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        else if ("within".Equals(token, StringComparison.Ordinal))
                        {
                            integersOnly = false;
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }
                        else
                        {
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }
                        if ("not".Equals(token, StringComparison.Ordinal))
                        {
                            if (!hackForCompatibility && !inRange)
                            {
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            inRange = !inRange;
                            if (!TryGetNextToken(tokens, ref x, out token))
                            {
                                return ParseRuleStatus.ConstraintMissingToken;
                            }
                        }

                        List<long> valueList = new List<long>();

                        // the token t is always one item ahead
                        while (true)
                        {
                            // ICU4N NOTE: This overload allows non-ASCII digits
                            if (!Long.TryParse(token, radix: 10, out long low))
                            {
                                return ParseRuleStatus.ConstraintValueMustBeDigits;
                            }
                            long high = low;
                            if (x < tokens.Length)
                            {
                                if (!TryGetNextToken(tokens, ref x, out token))
                                {
                                    return ParseRuleStatus.ConstraintMissingToken;
                                }
                                if (token.Equals(".", StringComparison.Ordinal))
                                {
                                    if (!TryGetNextToken(tokens, ref x, out token))
                                    {
                                        return ParseRuleStatus.ConstraintMissingToken;
                                    }
                                    if (!token.Equals(".", StringComparison.Ordinal))
                                    {
                                        return ParseRuleStatus.ConstraintUnexpectedToken;
                                    }
                                    if (!TryGetNextToken(tokens, ref x, out token))
                                    {
                                        return ParseRuleStatus.ConstraintMissingToken;
                                    }
                                    // ICU4N NOTE: This overload allows non-ASCII digits
                                    if (!Long.TryParse(token, radix: 10, out high))
                                    {
                                        return ParseRuleStatus.ConstraintValueMustBeDigits;
                                    }
                                    if (x < tokens.Length)
                                    {
                                        if (!TryGetNextToken(tokens, ref x, out token))
                                        {
                                            return ParseRuleStatus.ConstraintMissingToken;
                                        }
                                        if (!token.Equals(",", StringComparison.Ordinal))
                                        {
                                            // adjacent number: 1 2
                                            // no separator, fail
                                            return ParseRuleStatus.ConstraintUnexpectedToken;
                                        }
                                    }
                                }
                                else if (!token.Equals(",", StringComparison.Ordinal))
                                {
                                    // adjacent number: 1 2
                                    // no separator, fail
                                    return ParseRuleStatus.ConstraintUnexpectedToken;
                                }
                            }
                            // at this point, either we are out of tokens, or t is ','
                            if (low > high)
                            {
                                token = low + "~" + high;
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            else if (mod != 0 && high >= mod)
                            {
                                token = high + ">mod=" + mod;
                                return ParseRuleStatus.ConstraintUnexpectedToken;
                            }
                            valueList.Add(low);
                            valueList.Add(high);
                            lowBound = Math.Min(lowBound, low);
                            highBound = Math.Max(highBound, high);
                            if (x >= tokens.Length)
                            {
                                break;
                            }
                            TryGetNextToken(tokens, ref x, out token);
                        }

                        if (token.Equals(",", StringComparison.Ordinal))
                        {
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }

                        if (valueList.Count == 2)
                        {
                            vals = null;
                        }
                        else
                        {
                            vals = new long[valueList.Count];
                            for (int k = 0; k < vals.Length; ++k)
                            {
                                vals[k] = valueList[k];
                            }
                        }

                        // Hack to exclude "is not 1,2"
                        if (lowBound != highBound && hackForCompatibility && !inRange)
                        {
                            token = "is not <range>";
                            return ParseRuleStatus.ConstraintUnexpectedToken;
                        }

                        newConstraint = new RangeConstraint(mod, inRange, operand, integersOnly, lowBound, highBound, vals);
                    }

                    if (andConstraint == null)
                    {
                        andConstraint = newConstraint;
                    }
                    else
                    {
                        andConstraint = new AndConstraint(andConstraint, newConstraint);
                    }
                }

                if (result == null)
                {
                    result = andConstraint;
                }
                else
                {
                    result = new OrConstraint(result, andConstraint);
                }
            }
            return ParseRuleStatus.OK;
        }

        private static readonly Regex AT_SEPARATED = new Regex("\\s*@\\s*", RegexOptions.Compiled); // ICU4N: \E and \Q are not supported in .NET
        private static readonly Regex OR_SEPARATED = new Regex("\\s*or\\s*", RegexOptions.Compiled);
        private static readonly Regex AND_SEPARATED = new Regex("\\s*and\\s*", RegexOptions.Compiled);
        private static readonly Regex COMMA_SEPARATED = new Regex("\\s*,\\s*", RegexOptions.Compiled);
        private static readonly Regex DOTDOT_SEPARATED = new Regex("\\s*\\.\\.\\s*", RegexOptions.Compiled); // ICU4N: \E and \Q are not supported in .NET
        private static readonly Regex TILDE_SEPARATED = new Regex("\\s*~\\s*", RegexOptions.Compiled);
        private static readonly Regex SEMI_SEPARATED = new Regex("\\s*;\\s*", RegexOptions.Compiled);
#endif

        // ICU4N: Removed Unexpected method and replaced with ThrowParseException()

#if FEATURE_SPAN
        /// <summary>
        /// Retrieves the next token in <paramref name="enumerator"/>. If there are no tokens remaining, returns <c>false</c>.
        /// </summary>
        // ICU4N specific - converted NextToken method to TryGetNextToken for .NET
        private static bool TryGetNextToken(ref SimpleTokenizerEnumerator enumerator, out ReadOnlySpan<char> result)
        {
            if (enumerator.MoveNext())
            {
                result = enumerator.Current;
                return true;
            }
            result = ReadOnlySpan<char>.Empty;
            return false;
        }
#else
        /// <summary>
        /// Retrieves the token at <paramref name="x"/> if it doesn't go past the end of the <paramref name="tokens"/> array.
        /// If <paramref name="x"/> is outside of the bounds of , returns <c>false</c>. Incrments the value of <paramref name="x"/>
        /// after the token is retrieved.
        /// </summary>
        // ICU4N specific - converted NextToken method to TryGetNextToken for .NET
        private static bool TryGetNextToken(string[] tokens, ref int x, out string result)
        {
            if (x >= 0 && x < tokens.Length)
            {
                result = tokens[x++];
                return true;
            }
            result = default;
            return false;
        }
#endif

#if FEATURE_SPAN
        /// <summary>
        /// Syntax:
        /// rule : keyword ':' condition
        /// keyword: &lt;identifier&gt;
        /// </summary>
        // ICU4N: Refactored ParseRule into TryParseRule for compatibility with .NET
        private static ParseRuleStatus TryParseRule(ReadOnlySpan<char> description, out Rule result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context)
        {
            result = default;
            source = default;
            context = description;

            if (description.Length == 0)
            {
                result = DEFAULT_RULE;
                return ParseRuleStatus.OK;
            }

            int x = description.IndexOf(':');
            if (x == -1)
            {
                return ParseRuleStatus.MissingColonInRule;
            }

            // ICU4N: Checked 2nd arg
            return TryParseRule(description.Slice(0, x).Trim(), description.Slice(x + 1).Trim(), out result, out source, out context);
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        internal static bool TryParseRule(string keyword, string description, out Rule result)
            => TryParseRule(keyword.AsSpan(), description.AsSpan(), out result);
#endif

        // ICU4N: Added overload for use by PluralRulesLoader so it doesn't have to use StringBuilder
        internal static bool TryParseRule(ReadOnlySpan<char> keyword, ReadOnlySpan<char> description, out Rule result)
        {
            return TryParseRule(keyword, description, out result, out ReadOnlySpan<char> _, out ReadOnlySpan<char> _) == ParseRuleStatus.OK;
        }

        /// <summary>
        /// Syntax:
        /// rule : keyword ':' condition
        /// keyword: &lt;identifier&gt;
        /// </summary>
        // ICU4N: Added overload for use by PluralRulesLoader so it doesn't have to use StringBuilder
        private static ParseRuleStatus TryParseRule(ReadOnlySpan<char> keyword, ReadOnlySpan<char> description, out Rule result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context)
        {
            result = default;
            source = default;
            context = description;

            if (description.Length == 0)
            {
                result = DEFAULT_RULE;
                return ParseRuleStatus.OK;
            }

            keyword = ToLowerInvariant(keyword);
            if (!IsValidKeyword(keyword))
            {
                source = keyword;
                return ParseRuleStatus.KeywordInvalid;
            }

            description = ToLowerInvariant(description);

            SplitTokenizerEnumerator constraintOrSamplesEnumerator = description.AsTokens('@', SplitTokenizerEnumerator.PatternWhiteSpace);
            // ICU4N: Check to ensure there are exactly 2, or 3 parts to the range.
            // Note that constraintOrSamples0 is always 0 length due to a quirk with using Regex match, which always returns at least 1 element.
            ReadOnlySpan<char> constraintOrSamples0 = constraintOrSamplesEnumerator.MoveNext() ? constraintOrSamplesEnumerator.Current : ReadOnlySpan<char>.Empty;
            ReadOnlySpan<char> constraintOrSamples1 = constraintOrSamplesEnumerator.MoveNext() ? constraintOrSamplesEnumerator.Current : ReadOnlySpan<char>.Empty;
            ReadOnlySpan<char> constraintOrSamples2 = constraintOrSamplesEnumerator.MoveNext() ? constraintOrSamplesEnumerator.Current : ReadOnlySpan<char>.Empty;
            if (constraintOrSamplesEnumerator.MoveNext())
            {
                source = keyword;
                return ParseRuleStatus.TooManySamples;
            }

            bool sampleFailure = false;
#pragma warning disable 612, 618
            FixedDecimalSamples integerSamples = null, decimalSamples = null;
            ParseRuleStatus status;
            // ICU4N: 0 or 1 part will pass through
            if (!constraintOrSamples1.IsEmpty && constraintOrSamples2.IsEmpty) // 2 parts
            {
                if ((status = FixedDecimalSamples.TryParse(constraintOrSamples1, out integerSamples, out source, out context)) != ParseRuleStatus.OK)
                    return status;
                if (integerSamples.sampleType == PluralRulesSampleType.Decimal)
                {
                    decimalSamples = integerSamples;
                    integerSamples = null;
                }
            }
            else if (!constraintOrSamples1.IsEmpty && !constraintOrSamples2.IsEmpty) // 3 parts
            {
                if ((status = FixedDecimalSamples.TryParse(constraintOrSamples1, out integerSamples, out source, out context)) != ParseRuleStatus.OK)
                    return status;
                if ((status = FixedDecimalSamples.TryParse(constraintOrSamples2, out decimalSamples, out source, out context)) != ParseRuleStatus.OK)
                    return status;
                if (integerSamples.sampleType != PluralRulesSampleType.Integer || decimalSamples.sampleType != PluralRulesSampleType.Decimal) // ICU4N TODO: This can never fail - should assert
                {
                    return ParseRuleStatus.StringMustHaveIntOrDec;
                }
            }
#pragma warning restore 612, 618

            // ICU4N TODO: This is completely unused
            if (sampleFailure)
            {
                return ParseRuleStatus.IllformedSamplesAtChar;
            }

            // 'other' is special, and must have no rules; all other keywords must have rules.
            bool isOther = keyword.Equals("other", StringComparison.Ordinal);
            if (isOther != constraintOrSamples0.IsEmpty)
            {
                context = description;
                return ParseRuleStatus.KeywordOtherMustNotHaveConstraints;
            }

            IConstraint constraint;
            if (isOther)
            {
                constraint = NO_CONSTRAINT;
            }
            else
            {
                if ((status = TryParseConstraint(constraintOrSamples0, out constraint, out source, out context)) != ParseRuleStatus.OK)
                    return status;
            }
            result = new Rule(keyword.ToString(), constraint, integerSamples, decimalSamples);
            return ParseRuleStatus.OK;
        }

        private static ReadOnlySpan<char> ToLowerInvariant(ReadOnlySpan<char> value)
        {
            // ICU4N: This is definitely slower, but will prevent an allocation of the
            // whole span if there are no uppercase chars.
            bool hasUpper = false;
#if FEATURE_RUNE
            foreach (var rune in value.EnumerateRunes())
            {
                if (Character.IsUpper(rune.Value))
                {
                    hasUpper = true;
                    break;
                }
            }
#else
            for (int i = 0; i < value.Length;)
            {
                int codePoint = value.CodePointAt(i);
                if (Character.IsUpper(codePoint))
                {
                    hasUpper = true;
                    break;
                }
                i += Character.CharCount(codePoint);
            }
#endif
            if (!hasUpper) return value;

            int bufferLength = value.Length;
            // NOTE: We cannot use array pool. We must allocate here so when we return we are referencing the same chunk of memory.
            char[] tempArray = new char[bufferLength];
            int actualLength = value.ToLowerInvariant(tempArray);
            // If the string contains supplemental chars, we might end up with an overflow.
            // Re-allocate the array with 2x the length (which is the maximum if all are surrogates).
            if (actualLength == -1)
            {
                tempArray = new char[bufferLength * 2];
                actualLength = value.ToLowerInvariant(tempArray);
            }
            var result = new ReadOnlySpan<char>(tempArray, 0, actualLength);
            return result;
        }
#else

        /// <summary>
        /// Syntax:
        /// rule : keyword ':' condition
        /// keyword: &lt;identifier&gt;
        /// </summary>
        // ICU4N: Refactored ParseRule into TryParseRule for compatibility with .NET
        private static ParseRuleStatus TryParseRule(string description, out Rule result, out string source, out string context)
        {
            result = default;
            source = default;
            context = description;

            if (description.Length == 0)
            {
                result = DEFAULT_RULE;
                return ParseRuleStatus.OK;
            }

            int x = description.IndexOf(':');
            if (x == -1)
            {
                return ParseRuleStatus.MissingColonInRule;
            }

            // ICU4N: Checked 2nd arg
            return TryParseRule(description.Substring(0, x).Trim(), description.Substring(x + 1).Trim(), out result, out source, out context);
        }

        // ICU4N: Added overload for use by PluralRulesLoader so it doesn't have to use StringBuilder
        internal static bool TryParseRule(string keyword, string description, out Rule result)
        {
            return TryParseRule(keyword, description, out result, out string _, out string _) == ParseRuleStatus.OK;
        }

        /// <summary>
        /// Syntax:
        /// rule : keyword ':' condition
        /// keyword: &lt;identifier&gt;
        /// </summary>
        // ICU4N: Added overload for use by PluralRulesLoader so it doesn't have to use StringBuilder
        private static ParseRuleStatus TryParseRule(string keyword, string description, out Rule result, out string source, out string context)
        {
            result = default;
            source = default;
            context = description;

            if (description.Length == 0)
            {
                result = DEFAULT_RULE;
                return ParseRuleStatus.OK;
            }

            keyword = keyword.ToLowerInvariant();
            if (!IsValidKeyword(keyword))
            {
                source = keyword;
                return ParseRuleStatus.KeywordInvalid;
            }

            description = description.ToLowerInvariant();

            string[] constraintOrSamples = AT_SEPARATED.Split(description);
            bool sampleFailure = false;
#pragma warning disable 612, 618
            FixedDecimalSamples integerSamples = null, decimalSamples = null;
            ParseRuleStatus status;
            switch (constraintOrSamples.Length)
            {
                case 1: break;
                case 2:
                    if ((status = FixedDecimalSamples.TryParse(constraintOrSamples[1], out integerSamples, out source, out context)) != ParseRuleStatus.OK)
                        return status;
                    if (integerSamples.sampleType == PluralRulesSampleType.Decimal)
                    {
                        decimalSamples = integerSamples;
                        integerSamples = null;
                    }
                    break;
                case 3:
                    if ((status = FixedDecimalSamples.TryParse(constraintOrSamples[1], out integerSamples, out source, out context)) != ParseRuleStatus.OK)
                        return status;
                    if ((status = FixedDecimalSamples.TryParse(constraintOrSamples[2], out decimalSamples, out source, out context)) != ParseRuleStatus.OK)
                        return status;
                    if (integerSamples.sampleType != PluralRulesSampleType.Integer || decimalSamples.sampleType != PluralRulesSampleType.Decimal) // ICU4N TODO: This can never fail - should assert
#pragma warning restore 612, 618
                    {
                        return ParseRuleStatus.StringMustHaveIntOrDec;
                    }
                    break;
                default:
                    source = keyword;
                    return ParseRuleStatus.TooManySamples;
            }
            // ICU4N TODO: This is completely unused
            if (sampleFailure)
            {
                return ParseRuleStatus.IllformedSamplesAtChar;
            }

            // 'other' is special, and must have no rules; all other keywords must have rules.
            bool isOther = keyword.Equals("other", StringComparison.Ordinal);
            if (isOther != (constraintOrSamples[0].Length == 0))
            {
                context = description;
                return ParseRuleStatus.KeywordOtherMustNotHaveConstraints;
            }

            IConstraint constraint;
            if (isOther)
            {
                constraint = NO_CONSTRAINT;
            }
            else
            {
                if ((status = TryParseConstraint(constraintOrSamples[0], out constraint, out source, out context)) != ParseRuleStatus.OK)
                    return status;
            }
            result = new Rule(keyword, constraint, integerSamples, decimalSamples);
            return ParseRuleStatus.OK;
        }
#endif

#if FEATURE_SPAN
        /// <summary>
        /// Syntax:
        /// rules : rule
        ///         rule ';' rules
        /// </summary>
        private static ParseRuleStatus TryParseRuleChain(ReadOnlySpan<char> description, out RuleList result, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context)
        {
            source = default;
            context = description;
            result = new RuleList();
            // remove trailing ;
            if (description[description.Length - 1] == ';')
            {
                description = description.TrimEnd(';');
            }

            ParseRuleStatus status;

            foreach (var ruleToken in description.AsTokens(';', SplitTokenizerEnumerator.PatternWhiteSpace))
            {
                // ICU4N: ruleToken is already trimmed
                if ((status = TryParseRule(ruleToken.Text, out Rule rule, out source, out context)) != ParseRuleStatus.OK)
                    return status;

                result.AddRule(rule);
            }
            result = result.Finish();
            return ParseRuleStatus.OK;
        }
#else

        /// <summary>
        /// Syntax:
        /// rules : rule
        ///         rule ';' rules
        /// </summary>
        private static ParseRuleStatus TryParseRuleChain(string description, out RuleList result, out string source, out string context)
        {
            source = default;
            context = description;
            result = new RuleList();
            // remove trailing ;
            if (description.EndsWith(";", StringComparison.Ordinal))
            {
                description = description.Substring(0, description.Length - 1); // ICU4N: Checked 2nd arg
            }
            ParseRuleStatus status;
            string[] rules = SEMI_SEPARATED.Split(description);
            for (int i = 0; i < rules.Length; ++i)
            {
                if ((status = TryParseRule(rules[i].Trim(), out Rule rule, out source, out context)) != ParseRuleStatus.OK)
                    return status;

                result.AddRule(rule);
            }
            result = result.Finish();
            return ParseRuleStatus.OK;
        }
#endif

        /// <summary>
        /// A status that is used to determine whether a parse result succeeded or failed in a specific way.
        /// </summary>
        internal enum ParseRuleStatus
        {
            OK,

            MisplacedEndRangeBound, // "Can only have … at the end of samples: "
            IllformedNumberRange, // "Ill-formed number range: "
            SamplesMustStartWithIntOrDec, // "Samples must start with 'integer' or 'decimal'"
            RangeBoundMustBeFloat,

            MissingColonInRule, // "Missing ':' in rule description '{0}'."
            KeywordInvalid, // "Keyword '{0}' is not valid."
            StringMustHaveIntOrDec, // "Must have @integer then @decimal in '{0}'."
            TooManySamples, // "Too many samples in '{0}'."
            IllformedSamplesAtChar, // "Ill-formed samples—'@' characters."
            KeywordOtherMustNotHaveConstraints, // "The keyword 'other' must have no constraints, just samples."

            ConstraintInvalidOperand,
            ConstraintUnexpectedToken, // "Unexpected token '{0}' in '{1}'."
            ConstraintMissingToken,
            ConstraintModulusMustBeDigits,
            ConstraintValueMustBeDigits,
        }

#nullable enable
        [DoesNotReturn]
        internal static void ThrowParseException(ParseRuleStatus status, string source, string context)
        {
            throw CreateParseException(status, source, context);
        }

        internal static Exception CreateParseException(ParseRuleStatus status, string source, string context)
        {
            return new FormatException(GetExceptionMessage(status, source, context));
        }

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        internal static string GetExceptionMessage(ParseRuleStatus status, string source, string context) => status switch
        {
            ParseRuleStatus.MisplacedEndRangeBound => string.Format(SR.MisplacedEndRangeBound, context),
            ParseRuleStatus.IllformedNumberRange => string.Format(SR.IllformedNumberRange, source),
            ParseRuleStatus.SamplesMustStartWithIntOrDec => string.Format(SR.SamplesMustStartWithIntOrDec, context),
            ParseRuleStatus.RangeBoundMustBeFloat => string.Format(SR.RangeBoundMustBeFloat, source),

            ParseRuleStatus.MissingColonInRule => string.Format(SR.MissingColonInRule, context),
            ParseRuleStatus.KeywordInvalid => string.Format(SR.KeywordInvalid, source),
            ParseRuleStatus.StringMustHaveIntOrDec => string.Format(SR.StringMustHaveIntOrDec, context),
            ParseRuleStatus.TooManySamples => string.Format(SR.TooManySamples, source, context),
            ParseRuleStatus.IllformedSamplesAtChar => SR.IllformedSamplesAtChar,
            ParseRuleStatus.KeywordOtherMustNotHaveConstraints => SR.KeywordOtherMustNotHaveConstraints,

            ParseRuleStatus.ConstraintInvalidOperand => string.Format(SR.ConstraintInvalidOperand, source),
            ParseRuleStatus.ConstraintUnexpectedToken => string.Format(SR.ConstraintUnexpectedToken, source, context),
            ParseRuleStatus.ConstraintMissingToken => string.Format(SR.ConstraintMissingToken, context),
            ParseRuleStatus.ConstraintModulusMustBeDigits => string.Format(SR.ConstraintModulusMustBeDigits, source, context),
            ParseRuleStatus.ConstraintValueMustBeDigits => string.Format(SR.ConstraintValueMustBeDigits, source, context),
        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

#nullable restore
        internal static class SR
        {
            public const string MisplacedEndRangeBound = "Can only have … at the end of samples: '{0}'";
            public const string IllformedNumberRange = "Ill-formed number range: '{0}'";
            public const string SamplesMustStartWithIntOrDec = "Samples must start with 'integer' or 'decimal': '{0}'";
            public const string RangeBoundMustBeFloat = "Range bound must be an integral or floating point number in ASCII digits: '{0}'";

            public const string MissingColonInRule = "Missing ':' in rule description '{0}'.";
            public const string KeywordInvalid = "Keyword '{0}' is not valid.";
            public const string StringMustHaveIntOrDec = "Must have @integer then @decimal in '{0}'.";
            public const string TooManySamples = "Too many samples in '{0}: {1}'.";
            public const string IllformedSamplesAtChar = "Ill-formed samples—'@' characters.";
            public const string KeywordOtherMustNotHaveConstraints = "The keyword 'other' must have no constraints, just samples.";

            public const string ConstraintInvalidOperand = "Invalid operand: '{0}'.";
            public const string ConstraintUnexpectedToken = "Unexpected token '{0}' in '{1}'.";
            public const string ConstraintMissingToken = "Missing token at the end of '{0}'.";
            public const string ConstraintModulusMustBeDigits = "Modulus must contain digits: '{0}' in '{1}'.";
            public const string ConstraintValueMustBeDigits = "Constraint value must contain digits: '{0}' in '{1}'.";
        }

        /// <summary>
        ///  An implementation of <see cref="IConstraint"/> representing a modulus,
        ///  a range of values, and include/exclude. Provides lots of
        ///  convenience factory methods.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        private class RangeConstraint : IConstraint
        {
            //private static readonly long serialVersionUID = 1;

            private readonly int mod;
            private readonly bool inRange;
            private readonly bool integersOnly;
            private readonly double lowerBound;
            private readonly double upperBound;
            private readonly long[] range_list;
#pragma warning disable 612, 618
            private readonly Operand operand;

            internal RangeConstraint(int mod, bool inRange, Operand operand, bool integersOnly,
                    double lowBound, double highBound, long[] vals)
            {
#pragma warning restore 612, 618
                this.mod = mod;
                this.inRange = inRange;
                this.integersOnly = integersOnly;
                this.lowerBound = lowBound;
                this.upperBound = highBound;
                this.range_list = vals;
                this.operand = operand;
            }

#pragma warning disable 612, 618
            public virtual bool IsFulfilled(IFixedDecimal number)
            {
                double n = number.GetPluralOperand(operand);
                if ((integersOnly && (n - (long)n) != 0.0
                        || operand == Operand.j && number.GetPluralOperand(Operand.v) != 0))
                {
                    return !inRange;
                }
#pragma warning restore 612, 618
                if (mod != 0)
                {
                    n = n % mod;    // java % handles double numerator the way we want
                }
                bool test = n >= lowerBound && n <= upperBound;
                if (test && range_list != null)
                {
                    test = false;
                    for (int i = 0; !test && i < range_list.Length; i += 2)
                    {
                        test = n >= range_list[i] && n <= range_list[i + 1];
                    }
                }
                return inRange == test;
            }

#pragma warning disable 612, 618
            public virtual bool IsLimited(PluralRulesSampleType sampleType)
            {
                bool valueIsZero = lowerBound == upperBound && lowerBound == 0d;
                bool hasDecimals =
                (operand == Operand.v || operand == Operand.w || operand == Operand.f || operand == Operand.t)
                && inRange != valueIsZero; // either NOT f = zero or f = non-zero
                switch (sampleType)
                {
                    case PluralRulesSampleType.Integer:
                        return hasDecimals // will be empty
                                || (operand == Operand.n || operand == Operand.i || operand == Operand.j)
                                && mod == 0
                                && inRange;

                    case PluralRulesSampleType.Decimal:
                        return (!hasDecimals || operand == Operand.n || operand == Operand.j)
                                && (integersOnly || lowerBound == upperBound)
                                && mod == 0
                                && inRange;
                }
                return false;
            }
#pragma warning restore 612, 618

            public override string ToString()
            {
                StringBuilder result = new StringBuilder();
                result.Append(operand);
                if (mod != 0)
                {
                    result.Append(" % ").Append(mod);
                }
                bool isList = lowerBound != upperBound;
                result.Append(
                        !isList ? (inRange ? " = " : " != ")
                                : integersOnly ? (inRange ? " = " : " != ")
                                        : (inRange ? " within " : " not within ")
                        );
                if (range_list != null)
                {
                    for (int i = 0; i < range_list.Length; i += 2)
                    {
                        AddRange(result, range_list[i], range_list[i + 1], i != 0);
                    }
                }
                else
                {
                    AddRange(result, lowerBound, upperBound, false);
                }
                return result.ToString();
            }
        }

        private static void AddRange(StringBuilder result, double lb, double ub, bool addSeparator)
        {
            if (addSeparator)
            {
                result.Append(",");
            }
            if (lb == ub)
            {
                result.Append(Format(lb));
            }
            else
            {
                result.Append(Format(lb) + ".." + Format(ub));
            }
        }

        private static string Format(double lb)
        {
            long lbi = (long)lb;
            return lb == lbi ? lbi.ToString(CultureInfo.InvariantCulture) : lb.ToString(CultureInfo.InvariantCulture); //String.valueOf(lbi) : String.valueOf(lb);
        }

        /// <summary>
        /// Convenience base class for and/or constraints.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        internal abstract class BinaryConstraint : IConstraint
        {
            //private static readonly long serialVersionUID = 1;
            protected readonly IConstraint a;
            protected readonly IConstraint b;

            protected BinaryConstraint(IConstraint a, IConstraint b)
            {
                this.a = a;
                this.b = b;
            }

#pragma warning disable 612, 618
            public abstract bool IsFulfilled(IFixedDecimal n);

            public abstract bool IsLimited(PluralRulesSampleType sampleType);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// A constraint representing the logical and of two constraints.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        internal class AndConstraint : BinaryConstraint
        {
            //private static readonly long serialVersionUID = 7766999779862263523L;

            internal AndConstraint(IConstraint a, IConstraint b)
                : base(a, b)
            {
            }

#pragma warning disable 612, 618
            public override bool IsFulfilled(IFixedDecimal n)
            {
                return a.IsFulfilled(n)
                        && b.IsFulfilled(n);
            }

            public override bool IsLimited(PluralRulesSampleType sampleType)
            {
#pragma warning restore 612, 618
                // we ignore the case where both a and b are unlimited but no values
                // satisfy both-- we still consider this 'unlimited'
                return a.IsLimited(sampleType)
                        || b.IsLimited(sampleType);
            }

            public override string ToString()
            {
                return a.ToString() + " and " + b.ToString();
            }
        }

        /// <summary>
        /// A constraint representing the logical or of two constraints.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        private class OrConstraint : BinaryConstraint
        {
            //private static readonly long serialVersionUID = 1405488568664762222L;

            internal OrConstraint(IConstraint a, IConstraint b)
                            : base(a, b)
            {
            }

#pragma warning disable 612, 618
            public override bool IsFulfilled(IFixedDecimal n)
            {
                return a.IsFulfilled(n)
                        || b.IsFulfilled(n);
            }

            public override bool IsLimited(PluralRulesSampleType sampleType)
#pragma warning restore 612, 618
            {
                return a.IsLimited(sampleType)
                        && b.IsLimited(sampleType);
            }

            public override string ToString()
            {
                return a.ToString() + " or " + b.ToString();
            }
        }

        /// <summary>
        /// Implementation of <see cref="Rule"/> that uses a constraint.
        /// Provides 'and' and 'or' to combine constraints.  Immutable.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        internal class Rule
        {
            // TODO - Findbugs: Class com.ibm.icu.text.PluralRules$Rule defines non-transient
            // non-serializable instance field integerSamples. See ticket#10494.
            //private static readonly long serialVersionUID = 1;
            private readonly string keyword;
            private readonly IConstraint constraint;
#pragma warning disable 612, 618
            private readonly FixedDecimalSamples integerSamples;
            private readonly FixedDecimalSamples decimalSamples;

            public Rule(string keyword, IConstraint constraint, FixedDecimalSamples integerSamples, FixedDecimalSamples decimalSamples)
#pragma warning restore 612, 618
            {
                this.keyword = keyword;
                this.constraint = constraint;
                this.integerSamples = integerSamples;
                this.decimalSamples = decimalSamples;
            }

#pragma warning disable 612, 618
            // ICU4N specific - adding accessors for private variables
            public FixedDecimalSamples IntegerSamples => integerSamples;
            public FixedDecimalSamples DecimalSamples => decimalSamples;
#pragma warning restore 612, 618

            public virtual Rule And(IConstraint c)
            {
                return new Rule(keyword, new AndConstraint(constraint, c), integerSamples, decimalSamples);
            }

            public virtual Rule Or(IConstraint c)
            {
                return new Rule(keyword, new OrConstraint(constraint, c), integerSamples, decimalSamples);
            }

            public virtual string Keyword => keyword;

#pragma warning disable 612, 618
            public virtual bool AppliesTo(IFixedDecimal n)
            {
                return constraint.IsFulfilled(n);
            }

            public virtual bool IsLimited(PluralRulesSampleType sampleType)
            {
                return constraint.IsLimited(sampleType);
            }
#pragma warning restore 612, 618

            public override string ToString()
            {
                return keyword + ": " + constraint.ToString()
                        + (integerSamples == null ? "" : " " + integerSamples.ToString())
                        + (decimalSamples == null ? "" : " " + decimalSamples.ToString());
            }

            /// <internal/>
            [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
            public override int GetHashCode()
#pragma warning restore 809
            {
                return keyword.GetHashCode() ^ constraint.GetHashCode();
            }

            public virtual string GetConstraint()
            {
                return constraint.ToString();
            }
        }

#if FEATURE_SERIALIZABLE
        [Serializable]
#endif
        internal class RuleList
        {
            private bool hasExplicitBoundingInfo = false;
            //private static readonly long serialVersionUID = 1;
            // ICU4N NOTE: To maintain isertion order, it is important that we don't delete from this collection.
            private readonly Dictionary<string, Rule> rules = new Dictionary<string, Rule>();
            private Rule otherRule = null;
            private const string OtherKeyword = "other";
            public int Count => rules.Count;
            public RuleList AddRule(Rule nextRule)
            {
                // ICU4N: Added this to encapsulate logic in RuleList. It was previously done in PluralRules.ParseRuleChain().
                hasExplicitBoundingInfo |= nextRule.IntegerSamples != null || nextRule.DecimalSamples != null;
                if (OtherKeyword.Equals(nextRule.Keyword, StringComparison.Ordinal))
                {
                    // ICU4N: Save this rule - we will add it last in Finish();
                    if (otherRule is null)
                        otherRule = nextRule;
                    else
                        throw new ArgumentException("Duplicate keyword:" + nextRule.Keyword);
                }
                else
                {
                    rules.Add(nextRule.Keyword, nextRule); // Don't allow dupicate keywords
                }
                return this;
            }

            // ICU4N specific - adding accessor for private property
            public bool HasExplicitBoundingInfo
            {
                get => hasExplicitBoundingInfo;
                set => hasExplicitBoundingInfo = value;
            }

            /// <summary>
            /// This rule must be called after all of the <see cref="AddRule(Rule)"/> calls to ensure the "other" rule is added.
            /// </summary>
            /// <returns>The completed <see cref="RuleList"/>.</returns>
            public virtual RuleList Finish()
            {
                // make sure that 'other' is present, and at the end.)
                if (otherRule is null)
                {
                    // ICU4N: Hard-coded rule will always succeed unless TryParseRule has a bug. So, we don't need a try version of this method.

                    // make sure we have always have an 'other' a rule
#if FEATURE_SPAN
                    ParseRuleStatus status = TryParseRule("other:".AsSpan(), out otherRule, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context);
                    if (status != ParseRuleStatus.OK)
                        ThrowParseException(status, source.ToString(), context.ToString());
#else
                    ParseRuleStatus status = TryParseRule("other:", out otherRule, out string source, out string context);
                    if (status != ParseRuleStatus.OK)
                        ThrowParseException(status, source, context);
#endif
                }
                rules.Add("other", otherRule);
                return this;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            private Rule SelectRule(IFixedDecimal n)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return rules.Values.FirstOrDefault(rule => rule.AppliesTo(n));
            }

#pragma warning disable CS0618 // Type or member is obsolete
            public virtual string Select(IFixedDecimal n)
            {
                if (n.IsInfinity || n.IsNaN)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return KeywordOther;
                }
                Rule r = SelectRule(n);
                return r.Keyword;
            }

            public virtual ICollection<string> GetKeywords() // ICU4N TODO: API Return Keys directly and change to a property. There is no way to add to it, anyway.
            {
                return rules.Keys.ToList();
            }

#pragma warning disable CS0618 // Type or member is obsolete
            public virtual bool IsLimited(string keyword, PluralRulesSampleType sampleType)
            {
                if (hasExplicitBoundingInfo)
                {
                    FixedDecimalSamples mySamples = GetDecimalSamples(keyword, sampleType);
                    return mySamples == null || mySamples.bounded;
                }

                return ComputeLimited(keyword, sampleType);
            }

            public virtual bool ComputeLimited(string keyword, PluralRulesSampleType sampleType)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                // if all rules with this keyword are limited, it's limited,
                // and if there's no rule with this keyword, it's unlimited
                return rules.TryGetValue(keyword, out Rule value) && value.IsLimited(sampleType);
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                foreach (Rule rule in rules.Values)
                {
                    if (builder.Length != 0)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        builder.Append(CategorySeparator);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    builder.Append(rule);
                }
                return builder.ToString();
            }

            public virtual string GetRules(string keyword)
            {
                return rules.TryGetValue(keyword, out Rule rule) ? rule.GetConstraint() : null;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            public virtual bool Select(IFixedDecimal sample, string keyword)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return rules.TryGetValue(keyword, out Rule rule) && rule.AppliesTo(sample);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            public virtual FixedDecimalSamples GetDecimalSamples(string keyword, PluralRulesSampleType sampleType)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return rules.TryGetValue(keyword, out Rule rule) ? GetSample(sampleType) : null;

#pragma warning disable CS0618 // Type or member is obsolete
                FixedDecimalSamples GetSample(PluralRulesSampleType type) => type == PluralRulesSampleType.Integer
#pragma warning restore CS0618 // Type or member is obsolete
                     ? rule.IntegerSamples
                     : rule.DecimalSamples;
            }
        }

#pragma warning disable 612, 618
        private bool AddConditional(ICollection<IFixedDecimal> toAddTo, ICollection<IFixedDecimal> others, double trial)
        {
            bool added;
            IFixedDecimal toAdd = new FixedDecimal(trial);
#pragma warning restore 612, 618
            if (!toAddTo.Contains(toAdd) && !others.Contains(toAdd))
            {
                others.Add(toAdd);
                added = true;
            }
            else
            {
                added = false;
            }
            return added;
        }



        // -------------------------------------------------------------------------
        // Static class methods.
        // -------------------------------------------------------------------------

        /// <summary>
        /// Provides access to the predefined cardinal-number <see cref="PluralRules"/> for a given
        /// <paramref name="locale"/>.
        /// Same as <c>GetInstance(UCultureInfo, PluralType.Cardinal)</c>.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="locale"/>.
        /// If there's no predefined rules for this <paramref name="locale"/>, the rules
        /// for the closest parent in the <paramref name="locale"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.8</stable>
        public static PluralRules GetInstance(UCultureInfo locale)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale));

            return GetInstance(locale.Name);
        }

        /// <summary>
        /// Provides access to the predefined cardinal-number <see cref="PluralRules"/> for a given
        /// <see cref="CultureInfo"/>.
        /// Same as <c>GetInstance(CultureInfo, PluralType.Cardinal)"/></c>
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="locale"/>.
        /// If there's no predefined rules for this <paramref name="locale"/>, the rules
        /// for the closest parent in the <paramref name="locale"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 54</stable>
        public static PluralRules GetInstance(CultureInfo locale)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale));

            return GetInstance(locale.ToUCultureInfo().Name);
        }

        /// <summary>
        /// Provides access to the predefined cardinal-number <see cref="PluralRules"/> for a given
        /// <paramref name="localeName"/>.
        /// Same as <c>GetInstance(CultureInfo, PluralType.Cardinal)"/></c>
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="localeName">The locale name for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="localeName"/>.
        /// If there's no predefined rules for this <paramref name="localeName"/>, the rules
        /// for the closest parent in the <paramref name="localeName"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="localeName"/> is <c>null</c>.</exception>
        /// <draft>ICU 60.1</draft>
        public static PluralRules GetInstance(string localeName) // ICU4N: Added overload to decouple from UCultureInfo
        {
            if (localeName is null)
                throw new ArgumentNullException(nameof(localeName));

#pragma warning disable 612, 618
            return PluralRulesFactory.DefaultFactory.GetInstance(localeName, PluralType.Cardinal);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Provides access to the predefined <see cref="PluralRules"/> for a given
        /// <paramref name="locale"/> and the plural <paramref name="type"/>.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="locale"/>.
        /// If there's no predefined rules for this <paramref name="locale"/>, the rules
        /// for the closest parent in the <paramref name="locale"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 50</stable>
        public static PluralRules GetInstance(UCultureInfo locale, PluralType type)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale));

            return GetInstance(locale.Name, type);
        }

        /// <summary>
        /// Provides access to the predefined <see cref="PluralRules"/> for a given
        /// <see cref="CultureInfo"/> and the plural <paramref name="type"/>.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="locale">The locale for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="locale"/>.
        /// If there's no predefined rules for this <paramref name="locale"/>, the rules
        /// for the closest parent in the <paramref name="locale"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="locale"/> is <c>null</c>.</exception>
        /// <stable>ICU 54</stable>
        public static PluralRules GetInstance(CultureInfo locale, PluralType type)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale));

            return GetInstance(locale.ToUCultureInfo().Name, type);
        }

        /// <summary>
        /// Provides access to the predefined <see cref="PluralRules"/> for a given
        /// <paramref name="localeName"/> and the plural <paramref name="type"/>.
        /// <para/>
        /// ICU defines plural rules for many locales based on CLDR <i>Language Plural Rules</i>.
        /// For these predefined rules, see CLDR page at
        /// <a href="http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html">http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/language_plural_rules.html</a>.
        /// <para/>
        /// This method is named <c>forLocale</c> in ICU4J.
        /// </summary>
        /// <param name="localeName">The locale name for which a <see cref="PluralRules"/> object is
        /// returned.</param>
        /// <param name="type">The plural type (e.g., cardinal or ordinal).</param>
        /// <returns>
        /// The predefined <see cref="PluralRules"/> object for this <paramref name="localeName"/>.
        /// If there's no predefined rules for this <paramref name="localeName"/>, the rules
        /// for the closest parent in the <paramref name="localeName"/> hierarchy that has one will
        /// be returned.  The final fallback always returns the default
        /// rules.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="localeName"/> is <c>null</c>.</exception>
        /// <draft>ICU 60.1</draft>
        public static PluralRules GetInstance(string localeName, PluralType type) // ICU4N: Added overload to decouple from UCultureInfo
        {
            if (localeName is null)
                throw new ArgumentNullException(nameof(localeName));

#pragma warning disable 612, 618
            return PluralRulesFactory.DefaultFactory.GetInstance(localeName, type);
#pragma warning restore 612, 618
        }

#if FEATURE_SPAN
        /// <summary>
        /// Checks whether a <paramref name="token"/> is a valid keyword.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <returns>true if the token is a valid keyword.</returns>
        private static bool IsValidKeyword(ReadOnlySpan<char> token)
        {
            foreach (char ch in token)
            {
                if (ch < 'a' || ch > 'z')
                    return false;
            }
            return true;
        }
#else

        /// <summary>
        /// Checks whether a <paramref name="token"/> is a valid keyword.
        /// </summary>
        /// <param name="token">The token to be checked.</param>
        /// <returns>true if the token is a valid keyword.</returns>
        private static bool IsValidKeyword(string token)
        {
            return ALLOWED_ID.IsSupersetOf(token);
        }
#endif

        /// <summary>
        /// Creates a new <see cref="PluralRules"/> object. Immutable.
        /// </summary>
        /// <param name="rules"></param>
        internal PluralRules(RuleList rules)
        {
            this.rules = rules;
            this.keywords = rules.GetKeywords().AsReadOnly();
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override int GetHashCode()
#pragma warning restore 809
        {
            return rules.GetHashCode();
        }

        /// <summary>
        /// Given a <paramref name="number"/>, returns the keyword of the first rule that applies to
        /// the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number for which the rule has to be determined.</param>
        /// <returns>The keyword of the selected rule.</returns>
        /// <stable>ICU 4.0</stable>
        public virtual string Select(double number)
        {
#pragma warning disable 612, 618
            return rules.Select(new FixedDecimal(number));
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Given a <paramref name="number"/>, returns the keyword of the first rule that applies to
        /// the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number for which the rule has to be determined.</param>
        /// <param name="countVisibleFractionDigits"></param>
        /// <param name="fractionaldigits"></param>
        /// <returns>The keyword of the selected rule.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual string Select(double number, int countVisibleFractionDigits, long fractionaldigits) // ICU4N: Marked internal since it is obsolete anyway
        {
            return rules.Select(new FixedDecimal(number, countVisibleFractionDigits, fractionaldigits));
        }

        /// <summary>
        /// Given a <paramref name="number"/> information, returns the keyword of the first rule that applies to
        /// the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number information for which the rule has to be determined.</param>
        /// <returns>The keyword of the selected rule.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual string Select(IFixedDecimal number) // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            return rules.Select(number);
        }

        /// <summary>
        /// Given a number information, and <paramref name="keyword"/>, return whether the keyword would match the number.
        /// </summary>
        /// <param name="sample">The number information for which the rule has to be determined.</param>
        /// <param name="keyword">The keyword to filter on.</param>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual bool Matches(FixedDecimal sample, string keyword) // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            return rules.Select(sample, keyword);
        }

        /// <summary>
        /// Gets a set of all rule keywords used in this <see cref="PluralRules"/>
        /// object.  The rule "other" is always present by default.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual ICollection<string> Keywords => keywords;

        /// <summary>
        /// Returns the unique value that this <paramref name="keyword"/> matches, or <see cref="NoUniqueValue"/>
        /// if the <paramref name="keyword"/> matches multiple values or is not defined for this <see cref="PluralRules"/>.
        /// </summary>
        /// <param name="keyword">The keyword to check for a unique value.</param>
        /// <returns>The unique value for the keyword, or <see cref="NoUniqueValue"/>.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual double GetUniqueKeywordValue(string keyword)
        {
            ICollection<double> values = GetAllKeywordValues(keyword);
            if (values != null && values.Count == 1)
            {
                return values.First();
            }
            return NoUniqueValue;
        }

        /// <summary>
        /// Returns all the values that trigger this keyword, or null if the number of such
        /// values is unlimited.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The values that trigger this keyword, or null.  The returned collection
        /// is immutable. It will be empty if the keyword is not defined.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual ICollection<double> GetAllKeywordValues(string keyword)
        {
#pragma warning disable 612, 618
            return GetAllKeywordValues(keyword, PluralRulesSampleType.Integer);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns all the values that trigger this <paramref name="keyword"/>, or null if the number of such
        /// values is unlimited.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="type">the type of samples requested,
        /// <see cref="PluralRulesSampleType.Integer"/> or <see cref="PluralRulesSampleType.Decimal"/>.</param>
        /// <returns>The values that trigger this <paramref name="keyword"/>, or null.  The returned collection
        /// is immutable. It will be empty if the <paramref name="keyword"/> is not defined.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual ICollection<double> GetAllKeywordValues(string keyword, PluralRulesSampleType type) // ICU4N: Marked internal since it is obsolete anyway
        {
            if (!IsLimited(keyword, type))
            {
                return null;
            }
            ICollection<double> samples = GetSamples(keyword, type);
            return samples == null ? null : samples.AsReadOnly();
        }

        /// <summary>
        /// Returns a list of integer values for which <see cref="Select(double)"/> would return that <paramref name="keyword"/>,
        /// or null if the keyword is not defined.
        /// </summary>
        /// <remarks>
        /// The returned collection is unmodifiable.
        /// The returned list is not complete, and there might be additional values that
        /// would return the <paramref name="keyword"/>.
        /// </remarks>
        /// <param name="keyword">The keyword to test.</param>
        /// <returns>A list of values matching the <paramref name="keyword"/>.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual ICollection<double> GetSamples(string keyword)
        {
#pragma warning disable 612, 618
            return GetSamples(keyword, PluralRulesSampleType.Integer);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns a list of values for which <see cref="Select(double)"/> would return that <paramref name="keyword"/>,
        /// or null if the keyword is not defined or no samples are available.
        /// </summary>
        /// <remarks>
        /// The returned collection is unmodifiable.
        /// The returned list is not complete, and there might be additional values that
        /// would return the <paramref name="keyword"/>. The keyword might be defined, and yet have an empty set of samples,
        /// IF there are samples for the other <paramref name="sampleType"/>.
        /// </remarks>
        /// <param name="keyword">The keyword to test.</param>
        /// <param name="sampleType">The type of samples requested, 
        /// <see cref="PluralRulesSampleType.Integer"/> or <see cref="PluralRulesSampleType.Decimal"/>.</param>
        /// <returns>A list of values matching the <paramref name="keyword"/>.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual ICollection<double> GetSamples(string keyword, PluralRulesSampleType sampleType) // ICU4N: Marked internal since it is obsolete anyway
        {
            if (!keywords.Contains(keyword))
            {
                return null;
            }
            ISet<double> result = new SortedSet<double>(); //new TreeSet<double>();

            if (rules.HasExplicitBoundingInfo)
            {
                FixedDecimalSamples samples = rules.GetDecimalSamples(keyword, sampleType);
                return samples == null ? result.AsReadOnly()
                        : samples.AddSamples(result).AsReadOnly();
            }

            // hack in case the rule is created without explicit samples
            int maxCount = IsLimited(keyword, sampleType) ? int.MaxValue : 20;

            switch (sampleType)
            {
                case PluralRulesSampleType.Integer:
                    for (int i = 0; i < 200; ++i)
                    {
                        if (!AddSample(keyword, Integer.GetInstance(i), maxCount, result))
                        {
                            break;
                        }
                    }
                    AddSample(keyword, Integer.GetInstance(1000000), maxCount, result); // hack for Welsh
                    break;
                case PluralRulesSampleType.Decimal:
                    for (int i = 0; i < 2000; ++i)
                    {
                        if (!AddSample(keyword, new FixedDecimal(i / 10d, 1), maxCount, result))
                        {
                            break;
                        }
                    }
                    AddSample(keyword, new FixedDecimal(1000000d, 1), maxCount, result); // hack for Welsh
                    break;
            }
            return result.Count == 0 ? null : result.AsReadOnly();
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual bool AddSample(string keyword, Number sample, int maxCount, ICollection<double> result) // ICU4N: Marked internal since it is obsolete anyway // ICU4N: sample will always be a number
        {
            string selectedKeyword = sample is FixedDecimal ? Select((FixedDecimal)sample) : Select(sample.ToDouble());
            if (selectedKeyword.Equals(keyword))
            {
                result.Add(sample.ToDouble());
                if (--maxCount < 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a list of values for which <see cref="Select(double)"/> would return that <paramref name="keyword"/>,
        /// or null if the keyword is not defined or no samples are available.
        /// </summary>
        /// <remarks>
        /// The returned collection is unmodifiable.
        /// The returned list is not complete, and there might be additional values that
        /// would return the <paramref name="keyword"/>.
        /// </remarks>
        /// <param name="keyword">The keyword to test.</param>
        /// <param name="sampleType">The type of samples requested, 
        /// <see cref="PluralRulesSampleType.Integer"/> or <see cref="PluralRulesSampleType.Decimal"/>.</param>
        /// <returns>A list of values matching the <paramref name="keyword"/>.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual FixedDecimalSamples GetDecimalSamples(string keyword, PluralRulesSampleType sampleType) // ICU4N specific - changed from public to internal (obsolete anyway)
        {
            return rules.GetDecimalSamples(keyword, sampleType);
        }

        /// <summary>
        /// Returns the set of locales for which <see cref="PluralRules"/> are known.
        /// </summary>
        /// <returns>The set of locales for which PluralRules are known, as an array.</returns>
        /// <draft>ICU 4.2 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UCultureInfo[] GetUCultures() // ICU4N: Renamed from GetAvailableLocales // ICU4N TODO: API - Add UCutureTypes enum ?
        {
#pragma warning disable 612, 618
            return PluralRulesFactory.DefaultFactory.GetUCultures();
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns the 'functionally equivalent' locale with respect to
        /// plural rules.  Calling <see cref="PluralRules.GetInstance(CultureInfo)"/> with the functionally equivalent
        /// locale, and with the provided locale, returns rules that behave the same.
        /// </summary>
        /// <remarks>
        /// All locales with the same functionally equivalent locale have
        /// plural rules that behave the same.  This is not exaustive;
        /// there may be other locales whose plural rules behave the same
        /// that do not have the same equivalent locale.
        /// </remarks>
        /// <param name="locale">The locale to check.</param>
        /// <param name="isAvailable">If not null and of length &gt; 0, this will hold 'true' at
        /// index 0 if locale is directly defined (without fallback) as having plural rules.</param>
        /// <returns>The functionally-equivalent locale.</returns>
        /// <draft>ICU 4.2 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UCultureInfo GetFunctionalEquivalent(UCultureInfo locale, out bool isAvailable)
        {
#pragma warning disable 612, 618
            return PluralRulesFactory.DefaultFactory.GetFunctionalEquivalent(locale, out isAvailable);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns the 'functionally equivalent' locale with respect to
        /// plural rules.  Calling <see cref="PluralRules.GetInstance(CultureInfo)"/> with the functionally equivalent
        /// locale, and with the provided locale, returns rules that behave the same.
        /// </summary>
        /// <remarks>
        /// All locales with the same functionally equivalent locale have
        /// plural rules that behave the same.  This is not exaustive;
        /// there may be other locales whose plural rules behave the same
        /// that do not have the same equivalent locale.
        /// </remarks>
        /// <param name="locale">The locale to check.</param>
        /// <returns>The functionally-equivalent locale.</returns>
        /// <draft>ICU 4.2 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UCultureInfo GetFunctionalEquivalent(UCultureInfo locale)
        {
#pragma warning disable 612, 618
            return PluralRulesFactory.DefaultFactory.GetFunctionalEquivalent(locale);
#pragma warning restore 612, 618
        }

        /// <stable>ICU 3.8</stable>
        public override string ToString()
        {
            return rules.ToString();
        }

        /// <stable>ICU 3.8</stable>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override bool Equals(object rhs)
        {
            return rhs is PluralRules && Equals((PluralRules)rhs);
        }
#pragma warning restore 809

        /// <summary>
        /// Returns true if rhs is equal to this.
        /// </summary>
        /// <param name="rhs">The <see cref="PluralRules"/> to compare to.</param>
        /// <returns>true if this and rhs are equal.</returns>
        /// <stable>ICU 3.8</stable>
        // TODO Optimize this
        public virtual bool Equals(PluralRules rhs)
        {
            return rhs != null && ToString().Equals(rhs.ToString());
        }

        // ICU4N specific - de-nested KeywordStatus enum and renamed PluralRulesKeywordStatus

        /// <summary>
        /// Find the status for the <paramref name="keyword"/>, given a certain set of explicit values.
        /// </summary>
        /// <param name="keyword">The particular keyword (call <see cref="Keywords"/> to get the valid ones).</param>
        /// <param name="offset">The offset used, or 0.0d if not. Internally, the offset is subtracted from each explicit value before
        /// checking against the <paramref name="keyword"/> values.</param>
        /// <param name="explicits">A set of doubles that are used explicitly (eg [=0], "[=1]"). May be empty or null.</param>
        /// <param name="uniqueValue">If non null, set to the unique value.</param>
        /// <returns>The <see cref="PluralRulesKeywordStatus"/>.</returns>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual PluralRulesKeywordStatus GetKeywordStatus(string keyword, int offset, ICollection<double> explicits,
                out double? uniqueValue)
        {
#pragma warning disable 612, 618
            return GetKeywordStatus(keyword, offset, explicits, out uniqueValue, PluralRulesSampleType.Integer);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Find the status for the <paramref name="keyword"/>, given a certain set of explicit values.
        /// </summary>
        /// <param name="keyword">The particular keyword (call <see cref="Keywords"/> to get the valid ones)</param>
        /// <param name="offset">The offset used, or 0.0d if not. Internally, the offset is subtracted from each explicit value before
        /// checking against the keyword values.</param>
        /// <param name="explicits">A set of doubles that are used explicitly (eg [=0], "[=1]"). May be empty or null.</param>
        /// <param name="uniqueValue">Request <see cref="PluralRulesKeywordStatus"/> relative to 
        /// <see cref="PluralRulesSampleType.Integer"/> or <see cref="PluralRulesSampleType.Decimal"/> values.</param>
        /// <param name="sampleType">If non null, set to the unique value.</param>
        /// <returns>The <see cref="PluralRulesKeywordStatus"/>.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual PluralRulesKeywordStatus GetKeywordStatus(string keyword, int offset, ICollection<double> explicits,
                out double? uniqueValue, PluralRulesSampleType sampleType)  // ICU4N: Marked internal since it is obsolete anyway
        {
            // ICU4N specific - since we are using an out parameter, we don't need to check whether it is null first.
            uniqueValue = null;

            if (!keywords.Contains(keyword))
            {
                return PluralRulesKeywordStatus.Invalid;
            }

            if (!IsLimited(keyword, sampleType))
            {
                return PluralRulesKeywordStatus.Unbounded;
            }

            ICollection<double> values = GetSamples(keyword, sampleType);

            int originalSize = values.Count;

            if (explicits == null)
            {
                explicits = Collection.EmptySet<double>();
            }

            // Quick check on whether there are multiple elements

            if (originalSize > explicits.Count)
            {
                if (originalSize == 1)
                {
                    // ICU4N specific - since we are using an out parameter, we don't need to check whether it is null first.
                    uniqueValue = values.First(); //.iterator().next();
                    return PluralRulesKeywordStatus.Unique;
                }
                return PluralRulesKeywordStatus.Bounded;
            }

            // Compute if the quick test is insufficient.

            var subtractedSet = new JCG.HashSet<double>(values);
            foreach (double @explicit in explicits)
            {
                subtractedSet.Remove(@explicit - offset);
            }
            if (subtractedSet.Count == 0)
            {
                return PluralRulesKeywordStatus.Suppressed;
            }

            // ICU4N specific - since we are using an out parameter, we don't need to check whether it is null first.
            if (subtractedSet.Count == 1)
            {
                uniqueValue = subtractedSet.First(); //.iterator().next();
            }

            return originalSize == 1 ? PluralRulesKeywordStatus.Unique : PluralRulesKeywordStatus.Bounded;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public virtual string GetRules(string keyword)
        {
            return rules.GetRules(keyword);
        }

        private void WriteObject(Stream @out)
        {
            //throw new System.Runtime.Serialization.SerializationException();
            throw new Exception(); // ICU4N TODO: What exception will work here (.NET Standard 1.3 doesn't support serialization anyway)
        }

        private void ReadObject(Stream @in)
        {
            //throw new SerializationException();
            throw new Exception(); // ICU4N TODO: What exception will work here (.NET Standard 1.3 doesn't support serialization anyway)
        }

        private object WriteReplace()
        {
            return new PluralRulesSerialProxy(ToString());
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual int CompareTo(PluralRules other) // ICU4N: Marked internal since it is obsolete anyway
        {
            return ToString().CompareToOrdinal(other.ToString());
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual bool IsLimited(string keyword) // ICU4N: Marked internal since it is obsolete anyway
        {
            return rules.IsLimited(keyword, PluralRulesSampleType.Integer);
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual bool IsLimited(string keyword, PluralRulesSampleType sampleType) // ICU4N: Marked internal since it is obsolete anyway
        {
            return rules.IsLimited(keyword, sampleType);
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual bool ComputeLimited(string keyword, PluralRulesSampleType sampleType) // ICU4N: Marked internal since it is obsolete anyway
        {
            return rules.ComputeLimited(keyword, sampleType);
        }

        // From PluralSelectorAdapter in ICU4J. We implement IPluralSelector directly on PluralRules as per the comments.
        string IPluralSelector.Select(object context, double number)
        {
#pragma warning disable 612, 618
            IFixedDecimal dec = (IFixedDecimal)context;
            return rules.Select(dec);
#pragma warning restore 612, 618
        }
    }

    /// <summary>
    /// Type of plurals and <see cref="PluralRules"/>.
    /// </summary>
    /// <stable>ICU 50</stable>
    public enum PluralType
    {
        /// <summary>
        /// Plural rules for cardinal numbers: 1 file vs. 2 files.
        /// </summary>
        /// <stable>ICU 50</stable>
        Cardinal,

        /// <summary>
        /// Plural rules for ordinal numbers: 1st file, 2nd file, 3rd file, 4th file, etc.
        /// </summary>
        /// <stable>ICU 50</stable>
        Ordinal
    }

    /// <summary>
    /// Selection parameter for either integer-only or decimal-only.
    /// </summary>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    internal enum PluralRulesSampleType // ICU4N: Marked internal since it is obsolete anyway // ICU4N TODO: API - change name back to SampleType?
    {
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        Integer,

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        Decimal
    }

    /// <summary>
    /// Status of the keyword for the rules, given a set of explicit values.
    /// </summary>
    /// <draft>ICU 50</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public enum PluralRulesKeywordStatus // ICU4N TODO: API - change name back to KeywordStatus?
    {
        /// <summary>
        /// The keyword is not valid for the rules.
        /// </summary>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        Invalid,

        /// <summary>
        /// The keyword is valid, but unused (it is covered by the explicit values, OR has no values for the given <see cref="PluralRulesSampleType"/>).
        /// </summary>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        Suppressed,

        /// <summary>
        /// The keyword is valid, used, and has a single possible value (before considering explicit values).
        /// </summary>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        Unique,

        /// <summary>
        /// The keyword is valid, used, not unique, and has a finite set of values.
        /// </summary>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        Bounded,

        /// <summary>
        /// The keyword is valid but not bounded; there indefinitely many matching values.
        /// </summary>
        /// <draft>ICU 50</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        Unbounded
    }
}
