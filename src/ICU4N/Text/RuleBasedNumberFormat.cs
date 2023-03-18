using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N;
using J2N.Collections.Generic.Extensions;
using J2N.Globalization;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Resources;
using System.Text;
using System.Threading;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// The presentation of <see cref="RuleBasedNumberFormat"/>.
    /// </summary>
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        enum NumberPresentation
    {
        /// <summary>
        /// Indicates to create a spellout formatter that spells out a value
        /// in words in the desired language.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        SpellOut = 1,

        /// <summary>
        /// Indicates to create an ordinal formatter that attaches an ordinal
        /// suffix from the desired language to the end of the number (e.g. "123rd").
        /// </summary>
        /// <draft>ICU 60.1</draft>
        Ordinal = 2,

        /// <summary>
        /// Indicates to create a duration formatter that formats a duration in
        /// seconds as hours, minutes, and seconds.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        Duration = 3,

        /// <summary>
        /// Indicates to create a numbering system formatter to format a number in
        /// a rules-based numbering system such as <c>%hebrew</c> for Hebrew numbers or <c>%roman-upper</c>
        /// for upper-case Roman numerals.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        NumberingSystem = 4,
    }

    /// <summary>
    /// Extensions to <see cref="NumberPresentation"/>.
    /// </summary>
    internal static class NumberPresentationExtensions
    {
        /// <summary>
        /// Gets the rule name key to lookup this rule in the ICU resources.
        /// <para/>
        /// This is used internally to lookup the resource data.
        /// </summary>
        /// <param name="presentation">This <see cref="NumberPresentation"/> value.</param>
        /// <returns>The rule name key to lookup this rule in the ICU resources.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="presentation"/> value was not recognized.</exception>
        internal static string ToRuleNameKey(this NumberPresentation presentation) => presentation switch // ICU4N: This was originally in RuleBasedNumberFormat
        {
            NumberPresentation.SpellOut => "RBNFRules/SpelloutRules",
            NumberPresentation.Ordinal => "RBNFRules/OrdinalRules",
            NumberPresentation.Duration => "RBNFRules/DurationRules",
            NumberPresentation.NumberingSystem => "RBNFRules/NumberingSystemRules",
            _ => throw new ArgumentOutOfRangeException(nameof(presentation), $"Not expected presentation value: {presentation}"),
        };

        /// <summary>
        /// Gets the rule localizations key to lookup this rule in the ICU resources.
        /// <para/>
        /// This is used internally to lookup the resource data.
        /// </summary>
        /// <param name="presentation">This <see cref="NumberPresentation"/> value.</param>
        /// <returns>The rule localizations key to lookup this rule in the ICU resources.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="presentation"/> value was not recognized.</exception>
        internal static string ToRuleLocalizationsKey(this NumberPresentation presentation) => presentation switch // ICU4N: This was originally in RuleBasedNumberFormat
        {
            NumberPresentation.SpellOut => "SpelloutLocalizations",
            NumberPresentation.Ordinal => "OrdinalLocalizations",
            NumberPresentation.Duration => "DurationLocalizations",
            NumberPresentation.NumberingSystem => "NumberingSystemLocalizations",
            _ => throw new ArgumentOutOfRangeException(nameof(presentation), $"Not expected presentation value: {presentation}"),
        };
    }


    /// <summary>
    /// A class that formats numbers according to a set of rules. This number formatter is
    /// typically used for spelling out numeric values in words (e.g., 25,3476 as
    /// "twenty-five thousand three hundred seventy-six" or "vingt-cinq mille trois
    /// cents soixante-seize" or
    /// "funfundzwanzigtausenddreihundertsechsundsiebzig"), but can also be used for
    /// other complicated formatting tasks, such as formatting a number of seconds as hours,
    /// minutes and seconds (e.g., 3,730 as "1:02:10").
    /// <para/>
    /// The resources contain three predefined formatters for each locale: spellout, which
    /// spells out a value in words (123 is "one hundred twenty-three"); ordinal, which
    /// appends an ordinal suffix to the end of a numeral (123 is "123rd"); and
    /// duration, which shows a duration in seconds as hours, minutes, and seconds (123 is
    /// "2:03"). The client can also define more specialized <see cref="RuleBasedNumberFormat"/>s
    /// by supplying programmer-defined rule sets.
    /// <para/>
    /// The behavior of a <see cref="RuleBasedNumberFormat"/> is specified by a textual description
    /// that is either passed to the constructor as a <see cref="string"/> or loaded from a resource
    /// bundle. In its simplest form, the description consists of a semicolon-delimited list of <em>rules.</em>
    /// Each rule has a string of output text and a value or range of values it is applicable to.
    /// In a typical spellout rule set, the first twenty rules are the words for the numbers from
    /// 0 to 19:
    /// <code>
    /// zero; one; two; three; four; five; six; seven; eight; nine;
    /// ten; eleven; twelve; thirteen; fourteen; fifteen; sixteen; seventeen; eighteen; nineteen;
    /// </code>
    /// <para/>
    /// For larger numbers, we can use the preceding set of rules to format the ones place, and
    /// we only have to supply the words for the multiples of 10:
    /// <code>
    /// 20: twenty[-&gt;&gt;];
    /// 30: thirty{-&gt;&gt;];
    /// 40: forty[-&gt;&gt;];
    /// 50: fifty[-&gt;&gt;];
    /// 60: sixty[-&gt;&gt;];
    /// 70: seventy[-&gt;&gt;];
    /// 80: eighty[-&gt;&gt;];
    /// 90: ninety[-&gt;&gt;];
    /// </code>
    /// <para/>
    /// In these rules, the <em>base value</em> is spelled out explicitly and set off from the
    /// rule's output text with a colon. The rules are in a sorted list, and a rule is applicable
    /// to all numbers from its own base value to one less than the next rule's base value. The
    /// "&gt;&gt;" token is called a <em>substitution</em> and tells the formatter to
    /// isolate the number's ones digit, format it using this same set of rules, and place the
    /// result at the position of the "&gt;&gt;" token. Text in brackets is omitted if
    /// the number being formatted is an even multiple of 10 (the hyphen is a literal hyphen; 24
    /// is "twenty-four," not "twenty four").
    /// <para/>
    /// For even larger numbers, we can actually look up several parts of the number in the
    /// list:
    /// <code>100: &lt;&lt; hundred[ &gt;&gt;];</code>
    /// <para/>
    /// The "&lt;&lt;" represents a new kind of substitution. The &lt;&lt; isolates
    /// the hundreds digit (and any digits to its left), formats it using this same rule set, and
    /// places the result where the "&lt;&lt;" was. Notice also that the meaning of
    /// &gt;&gt; has changed: it now refers to both the tens and the ones digits. The meaning of
    /// both substitutions depends on the rule's base value. The base value determines the rule's <em>divisor,</em>
    /// which is the highest power of 10 that is less than or equal to the base value (the user
    /// can change this). To fill in the substitutions, the formatter divides the number being
    /// formatted by the divisor. The integral quotient is used to fill in the &lt;&lt;
    /// substitution, and the remainder is used to fill in the &gt;&gt; substitution. The meaning
    /// of the brackets changes similarly: text in brackets is omitted if the value being
    /// formatted is an even multiple of the rule's divisor. The rules are applied recursively, so
    /// if a substitution is filled in with text that includes another substitution, that
    /// substitution is also filled in.
    /// <para/>
    /// This rule covers values up to 999, at which point we add another rule:
    /// <code>1000: &lt;&lt; thousand[ &gt;&gt;];</code>
    /// <para/>
    /// Again, the meanings of the brackets and substitution tokens shift because the rule's
    /// base value is a higher power of 10, changing the rule's divisor. This rule can actually be
    /// used all the way up to 999,999. This allows us to finish out the rules as follows:
    /// <code>
    /// 1,000,000: &lt;&lt; million[ &gt;&gt;];
    /// 1,000,000,000: &lt;&lt; billion[ &gt;&gt;];
    /// 1,000,000,000,000: &lt;&lt; trillion[ &gt;&gt;];
    /// 1,000,000,000,000,000: OUT OF RANGE!;
    /// </code>
    /// <para/>
    /// Commas, periods, and spaces can be used in the base values to improve legibility and
    /// are ignored by the rule parser. The last rule in the list is customarily treated as an
    /// "overflow rule," applying to everything from its base value on up, and often (as
    /// in this example) being used to print out an error message or default representation.
    /// Notice also that the size of the major groupings in large numbers is controlled by the
    /// spacing of the rules: because in English we group numbers by thousand, the higher rules
    /// are separated from each other by a factor of 1,000.
    /// <para/>
    /// To see how these rules actually work in practice, consider the following example:
    /// Formatting 25,430 with this rule set would work like this:
    /// <list type="table">
    ///     <item>
    ///         <term><strong>&lt;&lt; thousand &gt;&gt;</strong></term>
    ///         <description>[the rule whose base value is 1,000 is applicable to 25,340]</description>
    ///     </item>
    ///     <item>
    ///         <term><strong>twenty-&gt;&gt;</strong> thousand &gt;&gt;</term>
    ///         <description>[25,340 over 1,000 is 25. The rule for 20 applies.]</description>
    ///     </item>
    ///     <item>
    ///         <term>twenty-<strong>five</strong> thousand &gt;&gt;</term>
    ///         <description>[25 mod 10 is 5. The rule for 5 is "five."</description>
    ///     </item>
    ///     <item>
    ///         <term>twenty-five thousand <strong>&lt;&lt; hundred &gt;&gt;</strong></term>
    ///         <description>[25,340 mod 1,000 is 340. The rule for 100 applies.]</description>
    ///     </item>
    ///     <item>
    ///         <term>twenty-five thousand <strong>three</strong> hundred &gt;&gt;</term>
    ///         <description>[340 over 100 is 3. The rule for 3 is &quot;three.&quot;]</description>
    ///     </item>
    ///     <item>
    ///         <term>twenty-five thousand three hundred <strong>forty</strong></term>
    ///         <description>[340 mod 100 is 40. The rule for 40 applies. Since 40 divides
    ///         evenly by 10, the hyphen and substitution in the brackets are omitted.]</description>
    ///     </item>
    /// </list>
    /// <para/>
    /// The above syntax suffices only to format positive integers. To format negative numbers,
    /// we add a special rule:
    /// <code>-x: minus &gt;&gt;;</code>
    /// <para/>
    /// This is called a <em>negative-number rule,</em> and is identified by "-x"
    /// where the base value would be. This rule is used to format all negative numbers. the
    /// &gt;&gt; token here means "find the number's absolute value, format it with these
    /// rules, and put the result here."
    /// <para/>
    /// We also add a special rule called a <em>fraction rule </em>for numbers with fractional
    /// parts:
    /// <code>x.x: &lt;&lt; point &gt;&gt;;</code>
    /// <para/>
    /// This rule is used for all positive non-integers (negative non-integers pass through the
    /// negative-number rule first and then through this rule). Here, the &lt;&lt; token refers to
    /// the number's integral part, and the &gt;&gt; to the number's fractional part. The
    /// fractional part is formatted as a series of single-digit numbers (e.g., 123.456 would be
    /// formatted as "one hundred twenty-three point four five six").
    /// <para/>
    /// To see how this rule syntax is applied to various languages, examine the resource data.
    /// </summary>
    /// <remarks>
    /// There is actually much more flexibility built into the rule language than the
    /// description above shows. A formatter may own multiple rule sets, which can be selected by
    /// the caller, and which can use each other to fill in their substitutions. Substitutions can
    /// also be filled in with digits, using a <see cref="ICU4N.Text.DecimalFormat"/> object. There is syntax that can be
    /// used to alter a rule's divisor in various ways. And there is provision for much more
    /// flexible fraction handling. A complete description of the rule syntax follows:
    /// <para/>
    /// The description of a <see cref="RuleBasedNumberFormat"/>'s behavior consists of one or more <em>rule
    /// sets.</em> Each rule set consists of a name, a colon, and a list of <em>rules.</em> A rule
    /// set name must begin with a % sign. Rule sets with names that begin with a single % sign
    /// are <em>public:</em> the caller can specify that they be used to format and parse numbers.
    /// Rule sets with names that begin with %% are <em>private:</em> they exist only for the use
    /// of other rule sets. If a formatter only has one rule set, the name may be omitted.
    /// <para/>
    /// The user can also specify a special "rule set" named <tt>%%lenient-parse</tt>.
    /// The body of <tt>%%lenient-parse</tt> isn't a set of number-formatting rules, but a <tt>RuleBasedCollator</tt>
    /// description which is used to define equivalences for lenient parsing. For more information
    /// on the syntax, see <see cref="RuleBasedCollator"/>. For more information on lenient parsing,
    /// see <see cref="LenientParseEnabled"/>. <em>Note:</em> symbols that have syntactic meaning
    /// in collation rules, such as '&amp;', have no particular meaning when appearing outside
    /// of the <tt>lenient-parse</tt> rule set.
    /// <para/>
    /// The body of a rule set consists of an ordered, semicolon-delimited list of <em>rules.</em>
    /// Internally, every rule has a base value, a divisor, rule text, and zero, one, or two <em>substitutions.</em>
    /// These parameters are controlled by the description syntax, which consists of a <em>rule
    /// descriptor,</em> a colon, and a <em>rule body.</em>
    /// <para/>
    /// A rule descriptor can take one of the following forms (text in <em>italics</em> is the
    /// name of a token):
    /// <list type="table">
    ///     <item>
    ///         <term><em>bv</em>:</term>
    ///         <description><em>bv</em> specifies the rule's base value. <em>bv</em> is a decimal
    ///         number expressed using ASCII digits. <em>bv</em> may contain spaces, period, and commas,
    ///         which are ignored. The rule's divisor is the highest power of 10 less than or equal to
    ///         the base value.</description>
    ///     </item>
    ///     <item>
    ///         <term><em>bv</em>/<em>rad</em>:</term>
    ///         <description><em>bv</em> specifies the rule's base value. The rule's divisor is the
    ///         highest power of <em>rad</em> less than or equal to the base value.</description>
    ///     </item>
    ///     <item>
    ///         <term><em>bv</em>&gt;:</term>
    ///         <description><em>bv</em> specifies the rule's base value. To calculate the divisor,
    ///         let the radix be 10, and the exponent be the highest exponent of the radix that yields a
    ///         result less than or equal to the base value. Every &gt; character after the base value
    ///         decreases the exponent by 1. If the exponent is positive or 0, the divisor is the radix
    ///         raised to the power of the exponent; otherwise, the divisor is 1.</description>
    ///     </item>
    ///     <item>
    ///         <term><em>bv</em>/<em>rad</em>&gt;:</term>
    ///         <description><em>bv</em> specifies the rule's base value. To calculate the divisor,
    ///         let the radix be <em>rad</em>, and the exponent be the highest exponent of the radix that
    ///         yields a result less than or equal to the base value. Every &gt; character after the radix
    ///         decreases the exponent by 1. If the exponent is positive or 0, the divisor is the radix
    ///         raised to the power of the exponent; otherwise, the divisor is 1.</description>
    ///     </item>
    ///     <item>
    ///         <term>-x:</term>
    ///         <description>The rule is a negative-number rule.</description>
    ///     </item>
    ///     <item>
    ///         <term>x.x:</term>
    ///         <description>The rule is an <em>improper fraction rule</em>. If the full stop in
    ///         the middle of the rule name is replaced with the decimal point
    ///         that is used in the language or DecimalFormatSymbols, then that rule will
    ///         have precedence when formatting and parsing this rule. For example, some
    ///         languages use the comma, and can thus be written as x,x instead. For example,
    ///         you can use "x.x: &lt;&lt; point &gt;&gt;;x,x: &lt;&lt; comma &gt;&gt;;" to
    ///         handle the decimal point that matches the language's natural spelling of
    ///         the punctuation of either the full stop or comma.</description>
    ///     </item>
    ///     <item>
    ///         <term>0.x:</term>
    ///         <description>The rule is a <em>proper fraction rule</em>. If the full stop in
    ///         the middle of the rule name is replaced with the decimal point
    ///         that is used in the language or DecimalFormatSymbols, then that rule will
    ///         have precedence when formatting and parsing this rule. For example, some
    ///         languages use the comma, and can thus be written as 0,x instead. For example,
    ///         you can use "0.x: point &gt;&gt;;0,x: comma &gt;&gt;;" to
    ///         handle the decimal point that matches the language's natural spelling of
    ///         the punctuation of either the full stop or comma</description>
    ///     </item>
    ///     <item>
    ///         <term>x.0:</term>
    ///         <description>The rule is a <em>master rule</em>. If the full stop in
    ///         the middle of the rule name is replaced with the decimal point
    ///         that is used in the language or DecimalFormatSymbols, then that rule will
    ///         have precedence when formatting and parsing this rule. For example, some
    ///         languages use the comma, and can thus be written as x,0 instead. For example,
    ///         you can use "x.0: &lt;&lt; point;x,0: &lt;&lt; comma;" to
    ///         handle the decimal point that matches the language's natural spelling of
    ///         the punctuation of either the full stop or comma</description>
    ///     </item>
    ///     <item>
    ///         <term>Inf:</term>
    ///         <description>The rule for infinity.</description>
    ///     </item>
    ///     <item>
    ///         <term>NaN:</term>
    ///         <description>The rule for an IEEE 754 NaN (not a number).</description>
    ///     </item>
    ///     <item>
    ///         <term><em>nothing</em></term>
    ///         <description>If the rule's rule descriptor is left out, the base value is one plus the
    ///         preceding rule's base value (or zero if this is the first rule in the list) in a normal
    ///         rule set. In a fraction rule set, the base value is the same as the preceding rule's
    ///         base value.</description>
    ///     </item>
    /// </list>
    /// <para/>
    /// A rule set may be either a regular rule set or a <em>fraction rule set,</em> depending
    /// on whether it is used to format a number's integral part (or the whole number) or a
    /// number's fractional part. Using a rule set to format a rule's fractional part makes it a
    /// fraction rule set.
    /// <para/>
    /// Which rule is used to format a number is defined according to one of the following
    /// algorithms: If the rule set is a regular rule set, do the following:
    /// <list type="bullet">
    ///     <item><description>If the rule set includes a master rule (and the number was passed in as a <tt>double</tt>),
    ///     use the master rule. (If the number being formatted was passed in as a <tt>long</tt>,
    ///     the master rule is ignored.)</description></item>
    ///     <item><description>If the number is negative, use the negative-number rule.</description></item>
    ///     <item><description>If the number has a fractional part and is greater than 1, use the improper fraction
    ///     rule.</description></item>
    ///     <item><description>If the number has a fractional part and is between 0 and 1, use the proper fraction
    ///     rule.</description></item>
    ///     <item><description>Binary-search the rule list for the rule with the highest base value less than or equal
    ///     to the number. If that rule has two substitutions, its base value is not an even multiple
    ///     of its divisor, and the number <em>is</em> an even multiple of the rule's divisor, use the
    ///     rule that precedes it in the rule list. Otherwise, use the rule itself.</description></item>
    /// </list>
    /// <para/>
    /// If the rule set is a fraction rule set, do the following:
    /// <list type="bullet">
    ///     <item><description>Ignore negative-number and fraction rules.</description></item>
    ///     <item><description>For each rule in the list, multiply the number being formatted (which will always be
    ///     between 0 and 1) by the rule's base value. Keep track of the distance between the result
    ///     the nearest integer.</description></item>
    ///     <item><description>Use the rule that produced the result closest to zero in the above calculation. In the
    ///     event of a tie or a direct hit, use the first matching rule encountered. (The idea here is
    ///     to try each rule's base value as a possible denominator of a fraction. Whichever
    ///     denominator produces the fraction closest in value to the number being formatted wins.) If
    ///     the rule following the matching rule has the same base value, use it if the numerator of
    ///     the fraction is anything other than 1; if the numerator is 1, use the original matching
    ///     rule. (This is to allow singular and plural forms of the rule text without a lot of extra
    ///     hassle.)</description></item>
    /// </list>
    /// <para/>
    /// A rule's body consists of a string of characters terminated by a semicolon. The rule
    /// may include zero, one, or two <em>substitution tokens,</em> and a range of text in
    /// brackets. The brackets denote optional text (and may also include one or both
    /// substitutions). The exact meanings of the substitution tokens, and under what conditions
    /// optional text is omitted, depend on the syntax of the substitution token and the context.
    /// The rest of the text in a rule body is literal text that is output when the rule matches
    /// the number being formatted.
    /// <para/>
    /// A substitution token begins and ends with a <em>token character.</em> The token
    /// character and the context together specify a mathematical operation to be performed on the
    /// number being formatted. An optional <em>substitution descriptor </em>specifies how the
    /// value resulting from that operation is used to fill in the substitution. The position of
    /// the substitution token in the rule body specifies the location of the resultant text in
    /// the original rule text.
    /// <para/>
    /// The meanings of the substitution token characters are as follows:
    /// <list type="table">
    ///     <item>
    ///         <term>&gt;&gt;</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in normal rule</term>
    ///                     <description>Divide the number by the rule's divisor and format the remainder</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in negative-number rule</term>
    ///                     <description>Find the absolute value of the number and format the result</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in fraction or master rule</term>
    ///                     <description>Isolate the number's fractional part and format it.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in rule in fraction rule set</term>
    ///                     <description>Not allowed.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>&gt;&gt;&gt;</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in normal rule</term>
    ///                     <description>Divide the number by the rule's divisor and format the remainder,
    ///                     but bypass the normal rule-selection process and just use the
    ///                     rule that precedes this one in this rule list.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in all other rules</term>
    ///                     <description>Not allowed.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>&lt;&lt;</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in normal rule</term>
    ///                     <description>Divide the number by the rule's divisor and format the quotient</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in negative-number rule</term>
    ///                     <description>Not allowed.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in fraction or master rule</term>
    ///                     <description>Isolate the number's integral part and format it.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in rule in fraction rule set</term>
    ///                     <description>Multiply the number by the rule's base value and format the result.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>==</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in all rule sets</term>
    ///                     <description>Format the number unchanged</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>[]</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in normal rule</term>
    ///                     <description>Omit the optional text if the number is an even multiple of the rule's divisor</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in negative-number rule</term>
    ///                     <description>Not allowed.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in improper-fraction rule</term>
    ///                     <description>Omit the optional text if the number is between 0 and 1 (same as specifying both an
    ///                     x.x rule and a 0.x rule)</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in master rule</term>
    ///                     <description>Omit the optional text if the number is an integer (same as specifying both an x.x
    ///                     rule and an x.0 rule)</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in proper-fraction rule</term>
    ///                     <description>Not allowed.</description>
    ///                 </item>
    ///                 <item>
    ///                     <term>in rule in fraction rule set</term>
    ///                     <description>Omit the optional text if multiplying the number by the rule's base value yields 1.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>$(cardinal,<i>plural syntax</i>)$</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in all rule sets</term>
    ///                     <description>This provides the ability to choose a word based on the number divided by the radix to the power of the
    ///                     exponent of the base value for the specified locale, which is normally equivalent to the &lt;&lt; value.
    ///                     This uses the cardinal plural rules from <see cref="PluralFormat"/>. All strings used in the plural format are treated
    ///                     as the same base value for parsing.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>$(ordinal,<i>plural syntax</i>)$</term>
    ///         <description>
    ///             <list type="bullet">
    ///                 <item>
    ///                     <term>in all rule sets</term>
    ///                     <description>This provides the ability to choose a word based on the number divided by the radix to the power of the
    ///                     exponent of the base value for the specified locale, which is normally equivalent to the &lt;&lt; value.
    ///                     This uses the ordinal plural rules from <see cref="PluralFormat"/>. All strings used in the plural format are treated
    ///                     as the same base value for parsing.</description>
    ///                 </item>
    ///             </list>
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// The substitution descriptor (i.e., the text between the token characters) may take one
    /// of three forms:
    /// <list type="table">
    ///     <item>
    ///         <term>A rule set name</term>
    ///         <description>Perform the mathematical operation on the number, and format the result using the
    ///         named rule set.</description>
    ///     </item>
    ///     <item>
    ///         <term>A <see cref="ICU4N.Text.DecimalFormat"/> pattern</term>
    ///         <description>Perform the mathematical operation on the number, and format the result using a
    ///         <see cref="ICU4N.Text.DecimalFormat"/> with the specified pattern. The pattern must begin with 0 or #.</description>
    ///     </item>
    ///     <item>
    ///         <term>Nothing</term>
    ///         <description>Perform the mathematical operation on the number, and format the result using the rule
    ///         set containing the current rule, except:
    ///             <list type="bullet">
    ///                 <item><description>You can't have an empty substitution descriptor with a == substitution.</description></item>
    ///                 <item><description>If you omit the substitution descriptor in a &gt;&gt; substitution in a fraction rule,
    ///                 format the result one digit at a time using the rule set containing the current rule.</description></item>
    ///                 <item><description>If you omit the substitution descriptor in a &lt;&lt; substitution in a rule in a
    ///                 fraction rule set, format the result using the default rule set for this formatter.</description></item>
    ///             </list>
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        partial class RuleBasedNumberFormat : NumberFormat
    {
        //-----------------------------------------------------------------------
        // constants
        //-----------------------------------------------------------------------

        //// Generated by serialver from JDK 1.4.1_01
        ////static final long serialVersionUID = -7664252765575395068L;

        // ICU4N: Moved constants to NumberPresentation enum

        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The formatter's rule sets.
        /// </summary>
        [NonSerialized]
        private NFRuleSet[] ruleSets = null;

        /// <summary>
        /// The formatter's rule names mapped to rule sets.
        /// </summary>
        [NonSerialized]
        private IDictionary<string, NFRuleSet> ruleSetsMap = null;

        /// <summary>
        /// A pointer to the formatter's default rule set. This is always included
        /// in <see cref="ruleSets"/>.
        /// </summary>
        [NonSerialized]
        private NFRuleSet defaultRuleSet = null;

        /// <summary>
        /// The formatter's locale.  This is used to create <see cref="ICU4N.Text.DecimalFormatSymbols"/> and
        /// <see cref="Collator"/> objects.
        /// </summary>
        /// <serial/>
        private UCultureInfo locale = null; // ICU4N TODO: UCultureInfo is not serializable.

        /// <summary>
        /// The formatter's rounding mode.
        /// </summary>
        /// <serial/>
        private Numerics.BigMath.RoundingMode roundingMode = Numerics.BigMath.RoundingMode.Unnecessary;

        /// <summary>
        /// <see cref="Collator"/> to be used in lenient parsing. This variable is lazy-evaluated:
        /// the collator is actually created the first time the client does a parse
        /// with lenient-parse mode turned on.
        /// </summary>
        [NonSerialized]
#pragma warning disable CS0618 // Type or member is obsolete
        private IRbnfLenientScannerProvider scannerProvider = null;
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// flag to mark whether we've previously looked for a scanner and failed
        /// </summary>
        [NonSerialized]
        private bool lookedForScanner;

        /// <summary>
        /// The <see cref="ICU4N.Text.DecimalFormatSymbols"/> object that any
        /// <see cref="ICU4N.Text.DecimalFormat"/> objects this
        /// formatter uses should use. This variable is lazy-evaluated: it isn't
        /// filled in if the rule set never uses a DecimalFormat pattern.
        /// </summary>
        [NonSerialized]
        private DecimalFormatSymbols decimalFormatSymbols = null;

        /// <summary>
        /// The <see cref="NumberFormat"/> used when lenient parsing numbers. This needs to reflect
        /// the locale. This is lazy-evaluated, like <see cref="decimalFormatSymbols"/>. It is
        /// here so it can be shared by different <see cref="NFSubstitution"/>s.
        /// </summary>
        [NonSerialized]
        private DecimalFormat decimalFormat = null;

        /// <summary>
        /// The rule used when dealing with infinity. This is lazy-evaluated, and derived from <see cref="decimalFormat"/>.
        /// It is here so it can be shared by different <see cref="NFRuleSet"/>s.
        /// </summary>
        [NonSerialized]
        private NFRule defaultInfinityRule = null;

        /// <summary>
        /// The rule used when dealing with IEEE 754 NaN. This is lazy-evaluated, and derived from <see cref="decimalFormat"/>.
        /// It is here so it can be shared by different <see cref="NFRuleSet"/>s.
        /// </summary>
        [NonSerialized]
        private NFRule defaultNaNRule = null;

        /// <summary>
        /// Flag specifying whether lenient parse mode is on or off.  Off by default.
        /// </summary>
        /// <serial/>
        private bool lenientParse = false;

        /// <summary>
        /// If the description specifies lenient-parse rules, they're stored here until
        /// the collator is created.
        /// </summary>
        [NonSerialized]
        private string lenientParseRules;

        /// <summary>
        /// If the description specifies post-process rules, they're stored here until
        /// post-processing is required.
        /// </summary>
        [NonSerialized]
        private string postProcessRules;

        /// <summary>
        /// Post processor lazily constructed from the <see cref="postProcessRules"/>.
        /// </summary>
        [NonSerialized]
        private IRbnfPostProcessor postProcessor;

        /// <summary>
        /// Localizations for rule set names.
        /// </summary>
        /// <serial/>
        private IDictionary<string, string[]> ruleSetDisplayNames;

        /// <summary>
        /// The public rule set names;
        /// </summary>
        /// <serial/>
        private string[] publicRuleSetNames;

        /// <summary>
        /// Data for handling context-based capitalization
        /// </summary>
        private bool capitalizationInfoIsSet = false;
        private bool capitalizationForListOrMenu = false;
        private bool capitalizationForStandAlone = false;
        [NonSerialized]
        private BreakIterator capitalizationBrkIter = null;


        private static readonly bool DEBUG = ICUDebug.Enabled("rbnf");

        //-----------------------------------------------------------------------
        // constructors
        //-----------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> that behaves according to the description
        /// passed in. The formatter uses the <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="description">A description of the formatter's desired behavior.
        /// See the class documentation for a complete explanation of the description
        /// syntax.
        /// </param>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public RuleBasedNumberFormat(string description) // ICU4N TODO: API Fix "current culture"
        {
            locale = UCultureInfo.CurrentCulture; // ICU4N TODO: In .NET, the default is to use invariant culture
            Init(description, null);
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> that behaves according to the description
        /// passed in. The formatter uses the <see cref="UCultureInfo.CurrentCulture"/>.
        /// <para/>
        /// The localizations data provides information about the public
        /// rule sets and their localized display names for different
        /// locales. The first element in the list is an array of the names
        /// of the public rule sets. The first element in this array is
        /// the initial default ruleset. The remaining elements in the
        /// list are arrays of localizations of the names of the public
        /// rule sets. Each of these is one longer than the initial array,
        /// with the first <see cref="string"/> being the <see cref="UCultureInfo"/> ID, and the remaining
        /// <see cref="string"/>s being the localizations of the rule set names, in the
        /// same order as the initial array.
        /// </summary>
        /// <param name="description">A description of the formatter's desired behavior.
        /// See the class documentation for a complete explanation of the description
        /// syntax.</param>
        /// <param name="localizations">A list of localizations for the rule set
        /// names in the description.</param>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 3.2</stable>
        internal RuleBasedNumberFormat(string description, string[][] localizations) // ICU4N TODO: API Fix "current culture" and localizations input and make public
        {
            locale = UCultureInfo.CurrentCulture; // ICU4N TODO: In .NET, the default is to use invariant culture
            Init(description, localizations);
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> that behaves according to the description
        /// passed in. The formatter uses the specified <see cref="locale"/> to determine the
        /// characters to use when formatting in numerals, and to define equivalences
        /// for lenient parsing.
        /// </summary>
        /// <param name="description">A description of the formatter's desired behavior.
        /// See the class documentation for a complete explanation of the description
        /// syntax.</param>
        /// <param name="locale">A locale, which governs which characters are used for
        /// formatting values in numerals, and which characters are equivalent in
        /// lenient parsing.</param>
        /// <stable>ICU 2.0</stable>
        public RuleBasedNumberFormat(string description, CultureInfo locale)
            : this(description, locale.ToUCultureInfo())
        {
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> that behaves according to the description
        /// passed in. The formatter uses the specified locale to determine the
        /// characters to use when formatting in numerals, and to define equivalences
        /// for lenient parsing.
        /// </summary>
        /// <param name="description">A description of the formatter's desired behavior.
        /// See the class documentation for a complete explanation of the description
        /// syntax.</param>
        /// <param name="locale">A locale, which governs which characters are used for
        /// formatting values in numerals, and which characters are equivalent in
        /// lenient parsing.</param>
        /// <stable>ICU 3.2</stable>
        public RuleBasedNumberFormat(string description, UCultureInfo locale)
        {
            this.locale = locale;
            Init(description, null);
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> that behaves according to the description
        /// passed in. The formatter uses the specified locale to determine the
        /// characters to use when formatting in numerals, and to define equivalences
        /// for lenient parsing.
        /// <para/>
        /// The localizations data provides information about the public
        /// rule sets and their localized display names for different
        /// locales. The first element in the list is an array of the names
        /// of the public rule sets. The first element in this array is
        /// the initial default ruleset. The remaining elements in the
        /// list are arrays of localizations of the names of the public
        /// rule sets. Each of these is one longer than the initial array,
        /// with the first <see cref="string"/> being the <see cref="UCultureInfo"/> ID, and the remaining
        /// <see cref="string"/>s being the localizations of the rule set names, in the
        /// same order as the initial array.
        /// </summary>
        /// <param name="description">A description of the formatter's desired behavior.
        /// See the class documentation for a complete explanation of the description
        /// syntax.</param>
        /// <param name="localizations">A list of localizations for the rule set names in the description.</param>
        /// <param name="locale">A locale, which governs which characters are used for
        /// formatting values in numerals, and which characters are equivalent in
        /// lenient parsing.</param>
        /// <stable>ICU 3.2</stable>
        internal RuleBasedNumberFormat(string description, string[][] localizations, UCultureInfo locale) // ICU4N TODO: API Clean up localizations input and make public
        {
            this.locale = locale;
            Init(description, localizations);
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> from a predefined description. The <paramref name="format"/>
        /// chooses among four possible predefined formats: <see cref="NumberPresentation.SpellOut"/>, <see cref="NumberPresentation.Ordinal"/>,
        /// <see cref="NumberPresentation.Duration"/>, and <see cref="NumberPresentation.NumberingSystem"/>.
        /// </summary>
        /// <param name="locale">The locale for the formatter.</param>
        /// <param name="format">A <see cref="NumberPresentation"/> specifying which kind of formatter to create for that
        /// locale.</param>
        /// <stable>ICU 2.0</stable>
        public RuleBasedNumberFormat(CultureInfo locale, NumberPresentation format)
            : this(locale.ToUCultureInfo(), format)
        {
        }

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> from a predefined description. The <paramref name="format"/>
        /// chooses among four possible predefined formats: <see cref="NumberPresentation.SpellOut"/>, <see cref="NumberPresentation.Ordinal"/>,
        /// <see cref="NumberPresentation.Duration"/>, and <see cref="NumberPresentation.NumberingSystem"/>.
        /// </summary>
        /// <param name="locale">The locale for the formatter.</param>
        /// <param name="format">A <see cref="NumberPresentation"/> specifying which kind of formatter to create for that
        /// locale.</param>
        /// <stable>ICU 3.2</stable>
        public RuleBasedNumberFormat(UCultureInfo locale, NumberPresentation format)
        {
            this.locale = locale;

            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.
                GetBundleInstance(ICUData.IcuRuleBasedNumberFormatBaseName, locale);

            // TODO: determine correct actual/valid locale.  Note ambiguity
            // here -- do actual/valid refer to pattern, DecimalFormatSymbols,
            // or Collator?
            UCultureInfo uloc = bundle.UCulture;
            SetCulture(uloc, uloc);

            StringBuilder description = new StringBuilder();
            string[][] localizations = null;

            try
            {
                ICUResourceBundle rules = bundle.GetWithFallback("RBNFRules/" + rulenames[(int)format - 1]);
                UResourceBundleEnumerator it = rules.GetEnumerator();
                while (it.MoveNext())
                {
                    description.Append(it.Current.GetString());
                }
            }
            catch (MissingManifestResourceException)
            {
            }

            // We use findTopLevel() instead of get() because
            // it's faster when we know that it's usually going to fail.
            UResourceBundle locNamesBundle = bundle.FindTopLevel(locnames[(int)format - 1]);
            if (locNamesBundle != null)
            {
                localizations = new string[locNamesBundle.Length][];
                for (int i = 0; i < localizations.Length; ++i)
                {
                    localizations[i] = locNamesBundle.Get(i).GetStringArray();
                }
            }
            // else there are no localized names. It's not that important.

            Init(description.ToString(), localizations);
        }

        private static readonly string[] rulenames = {
            "SpelloutRules", "OrdinalRules", "DurationRules", "NumberingSystemRules",
        };
        private static readonly string[] locnames = {
            "SpelloutLocalizations", "OrdinalLocalizations", "DurationLocalizations", "NumberingSystemLocalizations",
        };

        /// <summary>
        /// Creates a <see cref="RuleBasedNumberFormat"/> from a predefined description. Uses the
        /// <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <param name="format">A <see cref="NumberPresentation"/> specifying which kind of formatter to create.</param>
        /// <seealso cref="UCultureInfo.CurrentCulture"/>
        /// <stable>ICU 2.0</stable>
        public RuleBasedNumberFormat(NumberPresentation format) // ICU4N TODO: API Fix this so it sticks to the current culture (use null as a placeholder?)
            : this(UCultureInfo.CurrentCulture, format)
        {
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Duplicates this formatter.
        /// </summary>
        /// <returns>A <see cref="RuleBasedNumberFormat"/> that is equal to this one.</returns>
        /// <stable>ICU 2.0</stable>
        public override object Clone()
        {
            return base.Clone();
        }

        /// <summary>
        /// Tests two <see cref="RuleBasedNumberFormat"/>s for equality.
        /// </summary>
        /// <param name="that">The formatter to compare against this one.</param>
        /// <returns><c>true</c> if the two formatters have identical behavior; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 2.0</stable>
        public override bool Equals(object that)
        {
            // if the other object isn't a RuleBasedNumberFormat, that's
            // all we need to know
            // Test for capitalization info equality is adequately handled
            // by the NumberFormat test for capitalizationSetting equality;
            // the info here is just derived from that.

            // cast the other object's pointer to a pointer to a
            // RuleBasedNumberFormat
            if (!(that is RuleBasedNumberFormat that2))
            {
                return false;
            }
            else
            {
                // compare their locales and lenient-parse modes
                if (!locale.Equals(that2.locale) || lenientParse != that2.lenientParse)
                {
                    return false;
                }

                // if that succeeds, then compare their rule set lists
                if (ruleSets.Length != that2.ruleSets.Length)
                {
                    return false;
                }
                for (int i = 0; i < ruleSets.Length; i++)
                {
                    if (!ruleSets[i].Equals(that2.ruleSets[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode() // ICU4N specific - we cannot make GetHashCode [Obsolete] in .NET and must have an implementation.
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Generates a textual description of this formatter.
        /// </summary>
        /// <returns>A <see cref="string"/> containing a rule set that will produce a <see cref="RuleBasedNumberFormat"/>
        /// with identical behavior to this one. This won't necessarily be identical
        /// to the rule set description that was originally passed in, but will produce
        /// the same result.</returns>
        /// <stable>ICU 2.0</stable>
        public override string ToString()
        {

            // accumulate the descriptions of all the rule sets in a
            // StringBuffer, then cast it to a String and return it
            StringBuilder result = new StringBuilder();
            foreach (NFRuleSet ruleSet in ruleSets)
            {
                result.Append(ruleSet.ToString());
            }
            return result.ToString();
        }

        // ICU4N TODO: Serialization
        ///////**
        ////// * Writes this object to a stream.
        ////// * @param out The stream to write to.
        ////// */
        ////private void writeObject(java.io.ObjectOutputStream out)
        ////{
        ////// we just write the textual description to the stream, so we
        ////// have an implementation-independent streaming format
        ////out.writeUTF(this.toString());
        ////out.writeObject(this.locale);
        ////out.writeInt(this.roundingMode);
        ////}

        /////**
        //// * Reads this object in from a stream.
        //// * @param in The stream to read from.
        //// */
        ////private void readObject(java.io.ObjectInputStream in)
        ////{

        ////    // read the description in from the stream
        ////    String description = in.readUTF();
        ////    ULocale loc;

        ////    try {
        ////        loc = (ULocale) in.readObject();
        ////    } catch (Exception e) {
        ////        loc = ULocale.getDefault(Category.FORMAT);
        ////    }
        ////    try
        ////    {
        ////        roundingMode = in.readInt();
        ////    }
        ////    catch (Exception ignored)
        ////    {
        ////    }

        ////    // build a brand-new RuleBasedNumberFormat from the description,
        ////    // then steal its substructure.  This object's substructure and
        ////    // the temporary RuleBasedNumberFormat drop on the floor and
        ////    // get swept up by the garbage collector
        ////    RuleBasedNumberFormat temp = new RuleBasedNumberFormat(description, loc);
        ////    ruleSets = temp.ruleSets;
        ////    ruleSetsMap = temp.ruleSetsMap;
        ////    defaultRuleSet = temp.defaultRuleSet;
        ////    publicRuleSetNames = temp.publicRuleSetNames;
        ////    decimalFormatSymbols = temp.decimalFormatSymbols;
        ////    decimalFormat = temp.decimalFormat;
        ////    locale = temp.locale;
        ////    defaultInfinityRule = temp.defaultInfinityRule;
        ////    defaultNaNRule = temp.defaultNaNRule;
        ////}


        //-----------------------------------------------------------------------
        // public API functions
        //-----------------------------------------------------------------------

        /// <summary>
        /// Returns a list of the names of all of this formatter's public rule sets.
        /// </summary>
        /// <returns>A list of the names of all of this formatter's public rule sets.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual string[] GetRuleSetNames()
        {
            return (string[])publicRuleSetNames.Clone();
        }

        /// <summary>
        /// Return a list of locales for which there are locale-specific display names
        /// for the rule sets in this formatter. If there are no localized display names, return <c>null</c>.
        /// </summary>
        /// <returns>An array of the <see cref="UCultureInfo"/>s for which there is rule set display name information.</returns>
        /// <stable>ICU 3.2</stable>
        public virtual UCultureInfo[] GetRuleSetDisplayNameLocales() // ICU4N TODO: API - Ideally, this would never return null and would be suffixed Cultures
        {
            if (ruleSetDisplayNames != null)
            {
                ICollection<string> s = ruleSetDisplayNames.Keys;
                string[] locales = s.ToArray();
                Array.Sort(locales, CaseInsensitiveComparer.Default  /*StringComparer.OrdinalIgnoreCase*/);
                UCultureInfo[] result = new UCultureInfo[locales.Length];
                for (int i = 0; i < locales.Length; ++i)
                {
                    result[i] = new UCultureInfo(locales[i]);
                }
                return result;
            }
            return null;
        }

        private string[] GetNameListForLocale(UCultureInfo loc)
        {
            if (loc != null && ruleSetDisplayNames != null)
            {
                //String[] localeNames = { loc.getBaseName(), ULocale.getDefault(Category.DISPLAY).getBaseName() };
                string[] localeNames = { loc.Name, UCultureInfo.CurrentUICulture.Name };
                foreach (string lname in localeNames)
                {
                    string lname2 = lname;
                    while (lname2.Length > 0)
                    {
                        //String[] names = ruleSetDisplayNames.get(lname);
                        if (ruleSetDisplayNames.TryGetValue(lname2, out string[] names) || names != null)
                        {
                            return names;
                        }
                        //lname = UCultureInfo.getFallback(lname);
                        lname2 = UCultureInfo.GetParent(lname2);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Return the rule set display names for the provided <paramref name="locale"/>. These are in the same order
        /// as those returned by <see cref="GetRuleSetNames()"/>. The <paramref name="locale"/> is matched against the locales for
        /// which there is display name data, using normal fallback rules. If no locale matches,
        /// the default display names are returned. (These are the internal rule set names minus
        /// the leading '%'.)
        /// </summary>
        /// <param name="locale">The <see cref="UCultureInfo"/> to retrieve the rule set display names for.</param>
        /// <returns>An array of the locales that have display name information.</returns>
        /// <seealso cref="GetRuleSetNames()"/>
        /// <stable>ICU 3.2</stable>
        public virtual string[] GetRuleSetDisplayNames(UCultureInfo locale)
        {
            string[] names = GetNameListForLocale(locale);
            if (names != null)
            {
                return (string[])names.Clone();
            }
            names = GetRuleSetNames();
            for (int i = 0; i < names.Length; ++i)
            {
                names[i] = names[i].Substring(1);
            }
            return names;
        }

        /// <summary>
        /// Return the rule set display names for the <see cref="UCultureInfo.CurrentUICulture"/>.
        /// </summary>
        /// <returns>An array of the display names.</returns>
        /// <seealso cref="GetRuleSetDisplayNames(UCultureInfo)"/>
        /// <see cref="UCultureInfo.CurrentUICulture"/>
        /// <stable>ICU 3.2</stable>
        public virtual string[] GetRuleSetDisplayNames()
        {
            return GetRuleSetDisplayNames(UCultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Return the rule set display name for the provided <paramref name="ruleSetName"/> and <paramref name="locale"/>.
        /// The <paramref name="locale"/> is matched against the locales for which there is display name data, using
        /// normal fallback rules. If no locale matches, the default display name is returned.
        /// </summary>
        /// <param name="ruleSetName">The name of the rule set.</param>
        /// <param name="locale">The <see cref="UCultureInfo"/> to retrieve the rule set display name for.</param>
        /// <returns>The display name for the rule set.</returns>
        /// <exception cref="ArgumentException">The provided <paramref name="ruleSetName"/> is not a valid rule set name for this format.</exception>
        /// <seealso cref="GetRuleSetDisplayName(string)"/>
        /// <stable>ICU 3.2</stable>
        public virtual string GetRuleSetDisplayName(string ruleSetName, UCultureInfo locale)
        {
            string[] rsnames = publicRuleSetNames;
            for (int ix = 0; ix < rsnames.Length; ++ix)
            {
                if (rsnames[ix].Equals(ruleSetName, StringComparison.Ordinal))
                {
                    string[] names = GetNameListForLocale(locale);
                    if (names != null)
                    {
                        return names[ix];
                    }
                    return rsnames[ix].Substring(1);
                }
            }
            throw new ArgumentException("unrecognized rule set name: " + ruleSetName);
        }

        /// <summary>
        /// Return the rule set display name for the provided rule set in the <see cref="UCultureInfo.CurrentUICulture"/>.
        /// </summary>
        /// <param name="ruleSetName">The name of the rule set.</param>
        /// <returns>The display name for the rule set.</returns>
        /// <seealso cref="GetRuleSetDisplayName(string, UCultureInfo)"/>
        /// <seealso cref="UCultureInfo.CurrentUICulture"/>
        /// <stable>ICU 3.2</stable>
        public virtual string GetRuleSetDisplayName(string ruleSetName)
        {
            return GetRuleSetDisplayName(ruleSetName, UCultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Formats the specified <paramref name="number"/> according to the specified <paramref name="ruleSet"/>.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="ruleSet">The name of the rule set to format the number with.
        /// This must be the name of a valid public rule set for this formatter.</param>
        /// <returns>A textual representation of the number.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="ruleSet"/> is not public.
        /// <para/>
        /// -or-
        /// <para/>
        /// The <paramref name="ruleSet"/> is not valid for this formatter.
        /// </exception>
        /// <stable>ICU 2.0</stable>
        public virtual string Format(double number, string ruleSet)
        {
            if (ruleSet.StartsWith("%%", StringComparison.Ordinal))
            {
                throw new ArgumentException("Can't use internal rule set");
            }
            return AdjustForContext(Format(number, FindRuleSet(ruleSet)));
        }

        /// <summary>
        /// Formats the specified <paramref name="number"/> according to the specified <paramref name="ruleSet"/>.
        /// This method preserves all the precision in the <see cref="long"/> -- it doesn't convert it to a <see cref="double"/>.
        /// <para/>
        /// <strong>Note:</strong>If the specified rule set specifies a master ["x.0"] rule, this method
        /// ignores it. Use the <see cref="Format(double, string)"/> overload if you need it.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="ruleSet">The name of the rule set to format the number with.
        /// This must be the name of a valid public rule set for this formatter.</param>
        /// <returns>A textual representation of the number.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="ruleSet"/> is not public.
        /// <para/>
        /// -or-
        /// <para/>
        /// The <paramref name="ruleSet"/> is not valid for this formatter.
        /// </exception>
        /// <stable>ICU 2.0</stable>
        public virtual string Format(long number, string ruleSet)
        {
            if (ruleSet.StartsWith("%%", StringComparison.Ordinal))
            {
                throw new ArgumentException("Can't use internal rule set");
            }
            return AdjustForContext(Format(number, FindRuleSet(ruleSet)));
        }

        /// <summary>
        /// Formats the specified number using the formatter's default rule set.
        /// <para/>
        /// <strong>Note:</strong> The default rule set is the last public rule set defined in the description.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> that the result should be appended to.</param>
        /// <param name="ignore">This function doesn't examine or update the field position.</param>
        /// <returns><paramref name="toAppendTo"/></returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(double number,
                                   StringBuffer toAppendTo,
                                   FieldPosition ignore)
        {
            // this is one of the inherited format() methods.  Since it doesn't
            // have a way to select the rule set to use, it just uses the
            // default one
            // Note, the BigInteger/BigDecimal methods below currently go through this.
            if (toAppendTo.Length == 0)
            {
                toAppendTo.Append(AdjustForContext(Format(number, defaultRuleSet)));
            }
            else
            {
                // appending to other text, don't capitalize
                toAppendTo.Append(Format(number, defaultRuleSet));
            }
            return toAppendTo;
        }

        /// <summary>
        /// Formats the specified number using the formatter's default rule set.
        /// This method preserves all the precision in the <see cref="long"/> -- it doesn't convert it to a <see cref="double"/>.
        /// <para/>
        /// <strong>Note:</strong> The default rule set is the last public rule set defined in the description.
        /// <para/>
        /// <strong>Note:</strong> If the specified rule set specifies a master ["x.0"] rule, this method
        /// ignores it. Use the <see cref="Format(double, StringBuffer, FieldPosition)"/> overload if you need it.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> that the result should be appended to.</param>
        /// <param name="ignore">This function doesn't examine or update the field position.</param>
        /// <returns><paramref name="toAppendTo"/></returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION
        public
#else
        internal
#endif
            override StringBuffer Format(long number,
                                   StringBuffer toAppendTo,
                                   FieldPosition ignore)
        {
            // this is one of the inherited format() methods.  Since it doesn't
            // have a way to select the rule set to use, it just uses the
            // default one
            if (toAppendTo.Length == 0)
            {
                toAppendTo.Append(AdjustForContext(Format(number, defaultRuleSet)));
            }
            else
            {
                // appending to other text, don't capitalize
                toAppendTo.Append(Format(number, defaultRuleSet));
            }
            return toAppendTo;
        }

        /// <summary>
        /// Formats the specified number using the formatter's default rule set.
        /// <para/>
        /// <strong>Note:</strong> The default rule set is the last public rule set defined in the description.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> that the result should be appended to.</param>
        /// <param name="pos">on input: an optional alignment field; on output: the offsets
        /// of the alignment field in the formatted text.</param>
        /// <returns><paramref name="toAppendTo"/></returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(Numerics.BigMath.BigInteger number,
                                   StringBuffer toAppendTo,
                                   FieldPosition pos)
        {
            return Format(new Numerics.BigDecimal(number), toAppendTo, pos);
        }

        /// <summary>
        /// Formats the specified number using the formatter's default rule set.
        /// <para/>
        /// <strong>Note:</strong> The default rule set is the last public rule set defined in the description.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> that the result should be appended to.</param>
        /// <param name="pos">on input: an optional alignment field; on output: the offsets
        /// of the alignment field in the formatted text.</param>
        /// <returns><paramref name="toAppendTo"/></returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(Numerics.BigMath.BigDecimal number,
                                        StringBuffer toAppendTo,
                                        FieldPosition pos)
        {
            return Format(new Numerics.BigDecimal(number), toAppendTo, pos);
        }

        private static readonly Numerics.BigDecimal MaxValue = Numerics.BigDecimal.GetInstance(long.MaxValue);
        private static readonly Numerics.BigDecimal MinValue = Numerics.BigDecimal.GetInstance(long.MinValue);

        /// <summary>
        /// Formats the specified number using the formatter's default rule set.
        /// <para/>
        /// <strong>Note:</strong> The default rule set is the last public rule set defined in the description.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <param name="toAppendTo">A <see cref="StringBuffer"/> that the result should be appended to.</param>
        /// <param name="pos">on input: an optional alignment field; on output: the offsets
        /// of the alignment field in the formatted text.</param>
        /// <returns><paramref name="toAppendTo"/></returns>
        /// <stable>ICU 2.0</stable>
#if FEATURE_FIELDPOSITION && FEATURE_BIGMATH
        public
#else
        internal
#endif
            override StringBuffer Format(Numerics.BigDecimal number,
                                   StringBuffer toAppendTo,
                                   FieldPosition pos)
        {
            if (MinValue.CompareTo(number) > 0 || MaxValue.CompareTo(number) < 0)
            {
                // We're outside of our normal range that this framework can handle.
                // The DecimalFormat will provide more accurate results.
                return DecimalFormat.Format(number, toAppendTo, pos);
            }
            if (number.Scale == 0)
            {
                return Format(number.ToInt64(), toAppendTo, pos);
            }
            return Format(number.ToDouble(), toAppendTo, pos);
        }

        /// <summary>
        /// Parses the specified string, beginning at the specified position, according
        /// to this formatter's rules. This will match the string against all of the
        /// formatter's public rule sets and return the value corresponding to the longest
        /// parseable substring. This function's behavior is affected by the lenient
        /// parse mode.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <param name="parsePosition">On entry, contains the position of the first character
        /// in "text" to examine. On exit, has been updated to contain the position
        /// of the first character in "text" that wasn't consumed by the parse.</param>
        /// <returns>The number that corresponds to the parsed text. This will be an
        /// instance of either <see cref="Long"/> or <see cref="Double"/>, depending on whether the result has a
        /// fractional part.</returns>
        /// <seealso cref="LenientParseEnabled"/>
        /// <stable>ICU 2.0</stable>
        public override Number Parse(string text, ParsePosition parsePosition)
        {
            // parsePosition tells us where to start parsing.  We copy the
            // text in the string from here to the end inro a new string,
            // and create a new ParsePosition and result variable to use
            // for the duration of the parse operation
            string workingText = text.Substring(parsePosition.Index);
            ParsePosition workingPos = new ParsePosition(0);
            Number tempResult = null;

            // keep track of the largest number of characters consumed in
            // the various trials, and the result that corresponds to it
            Number result = NFRule.Zero;
            ParsePosition highWaterMark = new ParsePosition(workingPos.Index);

            // iterate over the public rule sets (beginning with the default one)
            // and try parsing the text with each of them.  Keep track of which
            // one consumes the most characters: that's the one that determines
            // the result we return
            for (int i = ruleSets.Length - 1; i >= 0; i--)
            {
                // skip private or unparseable rule sets
                if (!ruleSets[i].IsPublic || !ruleSets[i].IsParseable)
                {
                    continue;
                }

                // try parsing the string with the rule set.  If it gets past the
                // high-water mark, update the high-water mark and the result
                tempResult = ruleSets[i].Parse(workingText, workingPos, double.MaxValue);
                if (workingPos.Index > highWaterMark.Index)
                {
                    result = tempResult;
                    highWaterMark.Index = workingPos.Index;
                }
                // commented out because this API on ParsePosition doesn't exist in 1.1.x
                //            if (workingPos.ErrorIndex > highWaterMark.ErrorIndex) {
                //                highWaterMark.ErrorIndex = workingPos.ErrorIndex;
                //            }

                // if we manage to use up all the characters in the string,
                // we don't have to try any more rule sets
                if (highWaterMark.Index == workingText.Length)
                {
                    break;
                }

                // otherwise, reset our internal parse position to the
                // beginning and try again with the next rule set
                workingPos.Index = 0;
            }

            // add the high water mark to our original parse position and
            // return the result
            parsePosition.Index = parsePosition.Index + highWaterMark.Index;
            // commented out because this API on ParsePosition doesn't exist in 1.1.x
            //        if (highWaterMark.Index == 0) {
            //            parsePosition.ErrorIndex = parsePosition.Index + highWaterMark.ErrorIndex;
            //        }
            return result;
        }

        /// <summary>
        /// Gets or sets whether lenient parse mode should be enabled or disabled.
        /// <para/>
        /// When in lenient parse mode, the formatter uses an <see cref="IRbnfLenientScanner"/>
        /// for parsing the text. Lenient parsing is only in effect if a scanner
        /// is set. If a provider is not set, and this is used for parsing,
        /// a default scanner <see cref="ICU4N.Impl.Text.RbnfScannerProvider"/> will be set if it
        /// is loaded in the app domain. Otherwise this will have no effect.
        /// </summary>
        /// <seealso cref="IRbnfLenientScanner"/>
        /// <seealso cref="LenientScannerProvider"/>
        /// <stable>ICU 2.0</stable>
        public virtual bool LenientParseEnabled
        {
            get => lenientParse;
            set => lenientParse = value;
        }

        /// <summary>
        /// Gets or sets the provider for the lenient scanner. If this has not been set and/or the <c>ICU4N.Impl.Text.RbnfScannerProvider</c>
        /// has not been loaded in the current app domain, <see cref="LenientParseEnabled"/> has no effect.
        /// </summary>
        /// <seealso cref="LenientParseEnabled"/>
        /// <stable>ICU 4.4</stable>
        // This is necessary to decouple collation from format code. (or at least that is how it was done in ICU4J - we can probably just get rid of the reflection code).
#pragma warning disable CS0618 // Type or member is obsolete
        internal virtual IRbnfLenientScannerProvider LenientScannerProvider // ICU4N specific - marked internal instead of public
#pragma warning restore CS0618 // Type or member is obsolete
        {
            get
            {
                // there's a potential race condition if two threads try to set/get the scanner at
                // the same time, but you get what you get, and you shouldn't be using this from
                // multiple threads anyway.
                if (scannerProvider == null && lenientParse && !lookedForScanner)
                {
                    try
                    {
                        lookedForScanner = true;
                        Type cls = Type.GetType("ICU4N.Impl.Text.RbnfScannerProvider"); // This class is in the collation package
                        if (cls is null)
                            return null;
#pragma warning disable CS0618 // Type or member is obsolete
                        scannerProvider = (IRbnfLenientScannerProvider)Activator.CreateInstance(cls);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    catch (Exception)
                    {
                        // any failure, we just ignore and return null
                    }
                }

                return scannerProvider;
            }
            set => scannerProvider = value;
        }

        /**
         * Override the default rule set to use.  If ruleSetName is null, reset
         * to the initial default rule set.
         * @param ruleSetName the name of the rule set, or null to reset the initial default.
         * @throws IllegalArgumentException if ruleSetName is not the name of a public ruleset.
         * @stable ICU 2.0
         */
        internal virtual void SetDefaultRuleSet(string ruleSetName) // ICU4N TODO: API - Convert this to GetDefaultRuleSet() and make private so we can use a defaultRuleSetName field to set the value
        {
            if (ruleSetName == null)
            {
                if (publicRuleSetNames.Length > 0)
                {
                    defaultRuleSet = FindRuleSet(publicRuleSetNames[0]);
                }
                else
                {
                    defaultRuleSet = null;
                    int n = ruleSets.Length;
                    while (--n >= 0)
                    {
                        string currentName = ruleSets[n].Name;
                        if (currentName.Equals("%spellout-numbering", StringComparison.Ordinal) ||
                            currentName.Equals("%digits-ordinal", StringComparison.Ordinal) ||
                            currentName.Equals("%duration", StringComparison.Ordinal))
                        {

                            defaultRuleSet = ruleSets[n];
                            return;
                        }
                    }

                    n = ruleSets.Length;
                    while (--n >= 0)
                    {
                        if (ruleSets[n].IsPublic)
                        {
                            defaultRuleSet = ruleSets[n];
                            break;
                        }
                    }
                }
            }
            else if (ruleSetName.StartsWith("%%", StringComparison.Ordinal))
            {
                throw new ArgumentException("cannot use private rule set: " + ruleSetName);
            }
            else
            {
                defaultRuleSet = FindRuleSet(ruleSetName);
            }
        }

        /// <summary>
        /// Gets or sets the name of the current default rule set. If the default rule set is not public or not set, returns <see cref="string.Empty"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The setter <paramref name="value"/> is not public.
        /// <para/>
        /// -or-
        /// <para/>
        /// The setter <paramref name="value"/> is not valid for this formatter.
        /// </exception>
        /// <stable>ICU 3.0</stable>
        public virtual string DefaultRuleSetName
        {
            get
            {
                if (defaultRuleSet != null && defaultRuleSet.IsPublic)
                {
                    return defaultRuleSet.Name;
                }
                return string.Empty;
            }
            set => SetDefaultRuleSet(value);
        }

        /// <summary>
        /// Sets the decimal format symbols used by this formatter. The formatter uses a copy of the
        /// provided symbols.
        /// </summary>
        /// <param name="newSymbols">The desired <see cref="ICU4N.Text.DecimalFormatSymbols"/>.</param>
        /// <stable>ICU 49</stable>
        public virtual void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols)
        {
            if (newSymbols != null)
            {
                decimalFormatSymbols = (DecimalFormatSymbols)newSymbols.Clone();
                if (decimalFormat != null)
                {
                    decimalFormat.SetDecimalFormatSymbols(decimalFormatSymbols);
                }
                if (defaultInfinityRule != null)
                {
                    defaultInfinityRule = null;
                    var _ = DefaultInfinityRule; // Reset with the new DecimalFormatSymbols
                }
                if (defaultNaNRule != null)
                {
                    defaultNaNRule = null;
                    var _ = DefaultNaNRule; // Reset with the new DecimalFormatSymbols
                }

                // Apply the new decimalFormatSymbols by reparsing the rulesets
                foreach (NFRuleSet ruleSet in ruleSets)
                {
                    ruleSet.SetDecimalFormatSymbols(decimalFormatSymbols);
                }
            }
        }

        /// <summary>
        /// <icu/> Set a particular <see cref="DisplayContext"/> value in the formatter,
        /// such as <see cref="DisplayContext.CapitalizationForStandalone"/>.
        /// </summary>
        /// <param name="context">The <see cref="DisplayContext"/> value to set.</param>
        /// <stable>ICU 53</stable>
        // Here we override the NumberFormat implementation in order to
        // lazily initialize relevant items
        public override void SetContext(DisplayContext context) // ICU4N TODO: API - Refactor DisplayContext into a class with 4 properties (corresponding to 4 different enums)
        {
            base.SetContext(context);
            if (!capitalizationInfoIsSet &&
                  (context == DisplayContext.CapitalizationForUIListOrMenu || context == DisplayContext.CapitalizationForStandalone))
            {
                InitCapitalizationContextInfo(locale);
                capitalizationInfoIsSet = true;
            }
            if (capitalizationBrkIter == null && (context == DisplayContext.CapitalizationForBeginningOfSentence ||
                  (context == DisplayContext.CapitalizationForUIListOrMenu && capitalizationForListOrMenu) ||
                  (context == DisplayContext.CapitalizationForStandalone && capitalizationForStandAlone)))
            {
                capitalizationBrkIter = BreakIterator.GetSentenceInstance(locale);
            }
        }

        /// <summary>
        /// Gets or sets the rounding mode.
        /// </summary>
        /// <exception cref="ArgumentException">The setter <paramref name="value"/> is not
        /// recognized.</exception>
        /// <seealso cref="Numerics.BigMath.BigDecimal"/>
        /// <stable>ICU 56</stable>
#if FEATURE_BIGMATH
        public
#else
        internal
#endif
            override Numerics.BigMath.RoundingMode RoundingMode
        {
            get => roundingMode;
            set
            {
                // ICU4N TODO: In Java, this is supposed to be the ICU BigDecimal RoundingMode (it was an int), but
                // the DecimalFormat class uses java.math.RoundingMode instead. Need to fix this so either both
                // can fit here or we have a way to make a conversion.
                if (value < Numerics.BigMath.RoundingMode.Up || value > Numerics.BigMath.RoundingMode.Unnecessary)
                {
                    throw new ArgumentException("Invalid rounding mode: " + value);
                }

                this.roundingMode = value;
            }
        }

        //-----------------------------------------------------------------------
        // package-internal API
        //-----------------------------------------------------------------------

        /// <summary>
        /// Gets a reference to the formatter's default rule set. The default
        /// rule set is the last public rule set in the description, or the one
        /// most recently set by <see cref="DefaultRuleSetName"/>.
        /// </summary>
        internal NFRuleSet DefaultRuleSet => defaultRuleSet;

        /// <summary>
        /// Gets the scanner to use for lenient parsing.  The scanner is
        /// provided by the <see cref="LenientScannerProvider"/>. Returns <c>null</c>
        /// if <see cref="LenientParseEnabled"/> is <c>false</c> or there is no registered provider.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        internal IRbnfLenientScanner LenientScanner
#pragma warning restore CS0618 // Type or member is obsolete
        {
            get
            {
                if (lenientParse)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    return LenientScannerProvider?.Get(locale, lenientParseRules);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="ICU4N.Text.DecimalFormatSymbols"/> object that should be used by all <see cref="ICU4N.Text.DecimalFormat"/>
        /// instances owned by this formatter. This object is lazily created: this function
        /// creates it the first time it's called.
        /// </summary>
        internal DecimalFormatSymbols DecimalFormatSymbols
            // lazy-evaluate the DecimalFormatSymbols object.  This object
            // is shared by all DecimalFormat instances belonging to this
            // formatter.
            => LazyInitializer.EnsureInitialized(ref decimalFormatSymbols, () => new DecimalFormatSymbols(locale));


        internal DecimalFormat DecimalFormat
        {
            get => LazyInitializer.EnsureInitialized(ref decimalFormat, () =>
            {
                // Don't use NumberFormat.GetInstance, which can cause a recursive call
                string pattern = GetPattern(locale, NumberFormatStyle.NumberStyle);
                return new DecimalFormat(pattern, DecimalFormatSymbols);
            });
        }

        internal PluralFormat CreatePluralFormat(PluralType pluralType, string pattern)
        {
            return new PluralFormat(locale, pluralType, pattern, DecimalFormat);
        }

        /// <summary>
        /// Returns the default rule for infinity. This object is lazily created: this function
        /// creates it the first time it's called.
        /// </summary>
        internal NFRule DefaultInfinityRule
            => LazyInitializer.EnsureInitialized(ref defaultInfinityRule, () => new NFRule(this, "Inf: " + DecimalFormatSymbols.Infinity));


            /// <summary>
            /// Returns the default rule for NaN. This object is lazily created: this function
            /// creates it the first time it's called.
            /// </summary>
        internal NFRule DefaultNaNRule
            => LazyInitializer.EnsureInitialized(ref defaultNaNRule, () => new NFRule(this, "NaN: " + DecimalFormatSymbols.NaN));

        //-----------------------------------------------------------------------
        // construction implementation
        //-----------------------------------------------------------------------

        /**
         * This extracts the special information from the rule sets before the
         * main parsing starts.  Extra whitespace must have already been removed
         * from the description.  If found, the special information is removed from the
         * description and returned, otherwise the description is unchanged and null
         * is returned.  Note: the trailing semicolon at the end of the special
         * rules is stripped.
         * @param description the rbnf description with extra whitespace removed
         * @param specialName the name of the special rule text to extract
         * @return the special rule text, or null if the rule was not found
         */
        private string ExtractSpecial(StringBuilder description, string specialName)
        {
            string result = null;
            int lp = description.IndexOf(specialName, StringComparison.Ordinal);
            if (lp != -1)
            {
                // we've got to make sure we're not in the middle of a rule
                // (where specialName would actually get treated as
                // rule text)
                if (lp == 0 || description[lp - 1] == ';')
                {
                    // locate the beginning and end of the actual special
                    // rules (there may be whitespace between the name and
                    // the first token in the description)
                    int lpEnd = description.IndexOf(";%", lp, StringComparison.Ordinal);

                    if (lpEnd == -1)
                    {
                        lpEnd = description.Length - 1; // later we add 1 back to get the '%'
                    }
                    int lpStart = lp + specialName.Length;
                    while (lpStart < lpEnd &&
                           PatternProps.IsWhiteSpace(description[lpStart]))
                    {
                        ++lpStart;
                    }

                    // copy out the special rules
                    result = description.ToString(lpStart, lpEnd - lpStart); // ICU4N: Corrected 2nd parameter

                    // remove the special rule from the description
                    description.Delete(lp, (lpEnd + 1) - lp); // delete the semicolon but not the '%' // ICU4N: Corrected 2nd parameter
                }
            }
            return result;
        }

        /**
         * This function parses the description and uses it to build all of
         * internal data structures that the formatter uses to do formatting
         * @param description The description of the formatter's desired behavior.
         * This is either passed in by the caller or loaded out of a resource
         * by one of the constructors, and is in the description format specified
         * in the class docs.
         */
        private void Init(string description, string[][] localizations)
        {
            InitLocalizations(localizations);

            // start by stripping the trailing whitespace from all the rules
            // (this is all the whitespace follwing each semicolon in the
            // description).  This allows us to look for rule-set boundaries
            // by searching for ";%" without having to worry about whitespace
            // between the ; and the %
            StringBuilder descBuf = StripWhitespace(description);

            // check to see if there's a set of lenient-parse rules.  If there
            // is, pull them out into our temporary holding place for them,
            // and delete them from the description before the real description-
            // parsing code sees them

            lenientParseRules = ExtractSpecial(descBuf, "%%lenient-parse:");
            postProcessRules = ExtractSpecial(descBuf, "%%post-process:");

            // pre-flight parsing the description and count the number of
            // rule sets (";%" marks the end of one rule set and the beginning
            // of the next)
            int numRuleSets = 1;
            int p = 0;
            while ((p = descBuf.IndexOf(";%", p, StringComparison.Ordinal)) != -1)
            {
                ++numRuleSets;
                p += 2; // Skip the length of ";%"
            }

            // our rule list is an array of the appropriate size
            ruleSets = new NFRuleSet[numRuleSets];
            ruleSetsMap = new Dictionary<string, NFRuleSet>(numRuleSets * 2 + 1);
            defaultRuleSet = null;

            // Used to count the number of public rule sets
            // Public rule sets have names that begin with % instead of %%.
            int publicRuleSetCount = 0;

            // divide up the descriptions into individual rule-set descriptions
            // and store them in a temporary array.  At each step, we also
            // new up a rule set, but all this does is initialize its name
            // and remove it from its description.  We can't actually parse
            // the rest of the descriptions and finish initializing everything
            // because we have to know the names and locations of all the rule
            // sets before we can actually set everything up
            string[] ruleSetDescriptions = new string[numRuleSets];

            int curRuleSet = 0;
            int start = 0;

            while (curRuleSet < ruleSets.Length)
            {
                p = descBuf.IndexOf(";%", start, StringComparison.Ordinal);
                if (p < 0)
                {
                    p = descBuf.Length - 1;
                }
                ruleSetDescriptions[curRuleSet] = descBuf.ToString(start, (p + 1) - start); // ICU4N: Corrected 2nd parameter
                NFRuleSet ruleSet = new NFRuleSet(this, ruleSetDescriptions, curRuleSet);
                ruleSets[curRuleSet] = ruleSet;
                string currentName = ruleSet.Name;
                ruleSetsMap[currentName] = ruleSet;
                if (!currentName.StartsWith("%%", StringComparison.Ordinal))
                {
                    ++publicRuleSetCount;
                    if (defaultRuleSet == null
                            && currentName.Equals("%spellout-numbering", StringComparison.Ordinal)
                            || currentName.Equals("%digits-ordinal", StringComparison.Ordinal)
                            || currentName.Equals("%duration", StringComparison.Ordinal))
                    {
                        defaultRuleSet = ruleSet;
                    }
                }
                ++curRuleSet;
                start = p + 1;
            }

            // now we can take note of the formatter's default rule set, which
            // is the last public rule set in the description (it's the last
            // rather than the first so that a user can create a new formatter
            // from an existing formatter and change its default behavior just
            // by appending more rule sets to the end)

            // {dlf} Initialization of a fraction rule set requires the default rule
            // set to be known.  For purposes of initialization, this is always the
            // last public rule set, no matter what the localization data says.

            // Set the default ruleset to the last public ruleset, unless one of the predefined
            // ruleset names %spellout-numbering, %digits-ordinal, or %duration is found

            if (defaultRuleSet == null)
            {
                for (int i = ruleSets.Length - 1; i >= 0; --i)
                {
                    if (!ruleSets[i].Name.StartsWith("%%", StringComparison.Ordinal))
                    {
                        defaultRuleSet = ruleSets[i];
                        break;
                    }
                }
            }
            if (defaultRuleSet == null)
            {
                defaultRuleSet = ruleSets[ruleSets.Length - 1];
            }

            // finally, we can go back through the temporary descriptions
            // list and finish setting up the substructure
            for (int i = 0; i < ruleSets.Length; i++)
            {
                ruleSets[i].ParseRules(ruleSetDescriptions[i]);
            }

            // Now that the rules are initialized, the 'real' default rule
            // set can be adjusted by the localization data.

            // prepare an array of the proper size and copy the names into it
            string[] publicRuleSetTemp = new string[publicRuleSetCount];
            publicRuleSetCount = 0;
            for (int i = ruleSets.Length - 1; i >= 0; i--)
            {
                if (!ruleSets[i].Name.StartsWith("%%", StringComparison.Ordinal))
                {
                    publicRuleSetTemp[publicRuleSetCount++] = ruleSets[i].Name;
                }
            }

            if (publicRuleSetNames != null)
            {
                // confirm the names, if any aren't in the rules, that's an error
                // it is ok if the rules contain public rule sets that are not in this list
                //loop:
                for (int i = 0; i < publicRuleSetNames.Length; ++i)
                {
                    string name = publicRuleSetNames[i];
                    for (int j = 0; j < publicRuleSetTemp.Length; ++j)
                    {
                        if (name.Equals(publicRuleSetTemp[j], StringComparison.Ordinal))
                        {
                            //continue loop;
                            goto loop_continue;
                        }
                    }
                    throw new ArgumentException("did not find public rule set: " + name);

                loop_continue: { /* intentionally blank */ }
                }

                defaultRuleSet = FindRuleSet(publicRuleSetNames[0]); // might be different
            }
            else
            {
                publicRuleSetNames = publicRuleSetTemp;
            }
        }

        /**
         * Take the localizations array and create a Map from the locale strings to
         * the localization arrays.
         */
        private void InitLocalizations(string[][] localizations)
        {
            if (localizations != null)
            {
                publicRuleSetNames = (string[])localizations[0].Clone();

                IDictionary<string, string[]> m = new Dictionary<string, string[]>();
                for (int i = 1; i < localizations.Length; ++i)
                {
                    string[] data = localizations[i];
                    // ICU4N: Convert any culture names to use underscore instead of dash
                    string loc = new LocaleIDParser(data[0]).GetBaseName();
                    string[] names = new string[data.Length - 1];
                    if (names.Length != publicRuleSetNames.Length)
                    {
                        throw new ArgumentException("public name length: " + publicRuleSetNames.Length +
                                                           " != localized names[" + i + "] length: " + names.Length);
                    }
                    Array.Copy(data, 1, names, 0, names.Length);
                    m[loc] = names;
                }

                if (m.Count != 0)
                {
                    ruleSetDisplayNames = m;
                }
            }
        }

        /**
         * Set capitalizationForListOrMenu, capitalizationForStandAlone
         */
        private void InitCapitalizationContextInfo(UCultureInfo theLocale)
        {
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, theLocale);
            try
            {
                ICUResourceBundle rdb = rb.GetWithFallback("contextTransforms/number-spellout");
                int[] intVector = rdb.GetInt32Vector();
                if (intVector.Length >= 2)
                {
                    capitalizationForListOrMenu = (intVector[0] != 0);
                    capitalizationForStandAlone = (intVector[1] != 0);
                }
            }
            catch (MissingManifestResourceException)
            {
                // use default
            }
        }

        /**
         * This function is used by init() to strip whitespace between rules (i.e.,
         * after semicolons).
         * @param description The formatter description
         * @return The description with all the whitespace that follows semicolons
         * taken out.
         */
        private StringBuilder StripWhitespace(string description)
        {
            // since we don't have a method that deletes characters (why?!!)
            // create a new StringBuffer to copy the text into
            StringBuilder result = new StringBuilder();
            int descriptionLength = description.Length;

            // iterate through the characters...
            int start = 0;
            while (start < descriptionLength)
            {
                // seek to the first non-whitespace character...
                while (start < descriptionLength
                       && PatternProps.IsWhiteSpace(description[start]))
                {
                    ++start;
                }

                //if the first non-whitespace character is semicolon, skip it and continue
                if (start < descriptionLength && description[start] == ';')
                {
                    start += 1;
                    continue;
                }

                // locate the next semicolon in the text and copy the text from
                // our current position up to that semicolon into the result
                int p = description.IndexOf(';', start);
                if (p == -1)
                {
                    // or if we don't find a semicolon, just copy the rest of
                    // the string into the result
                    result.Append(description.Substring(start));
                    break;
                }
                else if (p < descriptionLength)
                {
                    result.Append(description.Substring(start, (p + 1) - start)); // ICU4N: Corrected 2nd parameter
                    start = p + 1;
                }
                else
                {
                    // when we get here, we've seeked off the end of the string, and
                    // we terminate the loop (we continue until *start* is -1 rather
                    // than until *p* is -1, because otherwise we'd miss the last
                    // rule in the description)
                    break;
                }
            }
            return result;
        }

        //-----------------------------------------------------------------------
        // formatting implementation
        //-----------------------------------------------------------------------

        /**
         * Bottleneck through which all the public format() methods
         * that take a double pass. By the time we get here, we know
         * which rule set we're using to do the formatting.
         * @param number The number to format
         * @param ruleSet The rule set to use to format the number
         * @return The text that resulted from formatting the number
         */
        private string Format(double number, NFRuleSet ruleSet)
        {
            // all API format() routines that take a double vector through
            // here.  Create an empty string buffer where the result will
            // be built, and pass it to the rule set (along with an insertion
            // position of 0 and the number being formatted) to the rule set
            // for formatting
            StringBuilder result = new StringBuilder();

            if (RoundingMode != Numerics.BigMath.RoundingMode.Unnecessary && !double.IsNaN(number) && !double.IsInfinity(number))
            {
                // We convert to a string because BigDecimal insists on excessive precision.
                number = Numerics.BigDecimal.Parse(Double.ToString(number, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture).SetScale(MaximumFractionDigits, roundingMode.ToICURoundingMode()).ToDouble();
            //RoundingModeExtensions
            }
            ruleSet.Format(number, result, 0, 0);
            PostProcess(result, ruleSet);
            return result.ToString();
        }

        /**
         * Bottleneck through which all the public format() methods
         * that take a long pass. By the time we get here, we know
         * which rule set we're using to do the formatting.
         * @param number The number to format
         * @param ruleSet The rule set to use to format the number
         * @return The text that resulted from formatting the number
         */
        private string Format(long number, NFRuleSet ruleSet)
        {
            // all API format() routines that take a double vector through
            // here.  We have these two identical functions-- one taking a
            // double and one taking a long-- the couple digits of precision
            // that long has but double doesn't (both types are 8 bytes long,
            // but double has to borrow some of the mantissa bits to hold
            // the exponent).
            // Create an empty string buffer where the result will
            // be built, and pass it to the rule set (along with an insertion
            // position of 0 and the number being formatted) to the rule set
            // for formatting
            StringBuilder result = new StringBuilder();
            if (number == long.MinValue)
            {
                // We can't handle this value right now. Provide an accurate default value.
                result.Append(DecimalFormat.Format(long.MinValue));
            }
            else
            {
                ruleSet.Format(number, result, 0, 0);
            }
            PostProcess(result, ruleSet);
            return result.ToString();
        }

        /**
         * Post-process the rules if we have a post-processor.
         */
        private void PostProcess(StringBuilder result, NFRuleSet ruleSet)
        {
            if (postProcessRules != null)
            {
                if (postProcessor == null)
                {
                    int ix = postProcessRules.IndexOf(";", StringComparison.Ordinal);
                    if (ix == -1)
                    {
                        ix = postProcessRules.Length;
                    }
                    string ppClassName = postProcessRules.Substring(0, ix).Trim(); // ICU4N: Checked 2nd parameter

                    // ICU4N: Hack to replace the namespace for the RBNFChinesePostProcessor if it is specified this way.
                    if (ppClassName.Equals("com.ibm.icu.text.RBNFChinesePostProcessor", StringComparison.Ordinal))
                        ppClassName = "ICU4N.Text.RbnfChinesePostProcessor";
                    try
                    {
                        //Class <?> cls = Class.forName(ppClassName);
                        //postProcessor = (RBNFPostProcessor)cls.newInstance();

                        Type cls = Type.GetType(ppClassName); // ICU4N TODO: Create abstract factory to create instance of the class, which is set at app startup
                        postProcessor = (IRbnfPostProcessor)Activator.CreateInstance(cls);
                        postProcessor.Init(this, postProcessRules);
                    }
                    catch (Exception e)
                    {
                        // if debug, print it out
                        if (DEBUG) Console.Out.WriteLine("could not locate " + ppClassName + ", error " +
                                           e.GetType().Name + ", " + e.Message);
                        postProcessor = null;
                        postProcessRules = null; // don't try again
                        return;
                    }
                }

                postProcessor.Process(result, ruleSet);
            }
        }

        /**
         * Adjust capitalization of formatted result for display context
         */
        private string AdjustForContext(string result)
        {
            DisplayContext capitalization = GetContext(DisplayContextType.Capitalization);
            if (capitalization != DisplayContext.CapitalizationNone && result != null && result.Length > 0
                && UChar.IsLower(result.CodePointAt(0)))
            {
                if (capitalization == DisplayContext.CapitalizationForBeginningOfSentence ||
                      (capitalization == DisplayContext.CapitalizationForUIListOrMenu && capitalizationForListOrMenu) ||
                      (capitalization == DisplayContext.CapitalizationForStandalone && capitalizationForStandAlone))
                {
                    if (capitalizationBrkIter == null)
                    {
                        // should only happen when deserializing, etc.
                        capitalizationBrkIter = BreakIterator.GetSentenceInstance(locale);
                    }
                    return UChar.ToTitleCase(locale, result, capitalizationBrkIter,
                                    UChar.TitleCaseNoLowerCase | UChar.TitleCaseNoBreakAdjustment);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the named rule set. Throws an <see cref="ArgumentException"/>
        /// if this formatter doesn't have a rule set with that name.
        /// </summary>
        /// <param name="name">The name of the desired rule set.</param>
        /// <returns>The rule set with that name.</returns>
        /// <exception cref="ArgumentException">No rule exists with the provided <paramref name="name"/>.</exception>
        internal NFRuleSet FindRuleSet(string name)
        {
            if (!ruleSetsMap.TryGetValue(name, out NFRuleSet result) || result == null)
            {
                throw new ArgumentException("No rule set named " + name);
            }
            return result;
        }
    }
}
