using ICU4N.Globalization;
using System;
#nullable enable

// ICU4N: Corresponds with the format operations of icu4j/main/classes/core/src/com/ibm/icu/text/RuleBasedNumberFormat.java

namespace ICU4N
{
    /// <summary>
    /// Formats a numeric type into a sequence of <see cref="char"/>s based on a set of rules defined in an instance
    /// of <see cref="NumberFormatRules"/>.
    /// </summary>
    /// <remarks>
    /// The static methods of the <see cref="FormatNumberRuleBased"/> class are to support the
    /// Rule-Based Number Formatting specification described in
    /// <a href="https://unicode-org.github.io/cldr-staging/ldml/v36/tr35-numbers.html#Rule-Based_Number_Formatting">
    /// https://unicode-org.github.io/cldr-staging/ldml/v36/tr35-numbers.html#Rule-Based_Number_Formatting</a>.
    /// <para/>
    /// This number formatter is typically used for spelling out numeric values in words (e.g., 25,3476 as
    /// "twenty-five thousand three hundred seventy-six" or "vingt-cinq mille trois cents soixante-seize"
    /// or "funfundzwanzigtausenddreihundertsechsundsiebzig"), but can also be used for other complicated
    /// formatting tasks, such as formatting a number of seconds as hours, minutes and seconds
    /// (e.g., 3,730 as "1:02:10").
    /// <para/>
    /// The resources contain four predefined formatters for each locale: spellout, which
    /// spells out a value in words (123 is "one hundred twenty-three"); ordinal, which
    /// appends an ordinal suffix to the end of a numeral (123 is "123rd");
    /// duration, which shows a duration in seconds as hours, minutes, and seconds (123 is
    /// "2:03"), and numbering system, which contain algorithmic numbering systems such as
    /// <c>%hebrew</c> for Hebrew numbers or <c>%roman-upper</c>
    /// for upper-case Roman numerals.
    /// </remarks>
    public static partial class FormatNumberRuleBased
    {
        // ICU4N TODO: API Make public after we have a real replacement for DecimalFormat and can thus support user-defined rules.
        // We should also add an overload that accepts a (string description) parameter instead of (NumberFormatRules rules) for
        // one-off formatting operations with custom rules. However these overloads must not be extension methods to avoid conflicts
        // with existing ToString() overloads.

        internal static string ToString(this long value, NumberFormatRules rules, string? ruleSetName = default, IFormatProvider? provider = default)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));

            return IcuNumber.FormatInt64RuleBased(value, rules, ruleSetName, UNumberFormatInfo.GetInstance(provider));
        }
        internal static string ToString(this long value, NumberFormatRules rules, IFormatProvider? provider)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));

            return IcuNumber.FormatInt64RuleBased(value, rules, ruleSetName: null, UNumberFormatInfo.GetInstance(provider));
        }
        internal static bool TryFormat(this long value, Span<char> destination, out int charsWritten, NumberFormatRules rules, string? ruleSetName = default, IFormatProvider? provider = default)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));

            return IcuNumber.TryFormatInt64RuleBased(value, destination, out charsWritten, rules, ruleSetName, UNumberFormatInfo.GetInstance(provider));
        }

        internal static bool TryFormat(this long value, Span<char> destination, out int charsWritten, NumberFormatRules rules, IFormatProvider? provider)
        {
            if (rules is null)
                throw new ArgumentNullException(nameof(rules));

            return IcuNumber.TryFormatInt64RuleBased(value, destination, out charsWritten, rules, ruleSetName: null, UNumberFormatInfo.GetInstance(provider));
        }

    }
}
