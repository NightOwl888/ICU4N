using System;

namespace ICU4N.Globalization
{
    /// <summary>
    /// The presentation style of rule-based number formatting.
    /// </summary>
    // ICU4N NOTE: For the sake of compatibility with .NET, we changed the values of each of these elements
    // so we have a reasonable default (SpellOut==0) rather than an invalid key for a newly defined instance.
    // Note that you would need to add 1 to make them the same value as ICU4J (if that is a thing).
    public enum NumberPresentation
    {
        /// <summary>
        /// Indicates to create a spellout formatter that spells out a value
        /// in words in the desired language.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        SpellOut = 0,

        /// <summary>
        /// Indicates to create an ordinal formatter that attaches an ordinal
        /// suffix from the desired language to the end of the number (e.g. "123rd").
        /// </summary>
        /// <draft>ICU 60.1</draft>
        Ordinal = 1,

        /// <summary>
        /// Indicates to create a duration formatter that formats a duration in
        /// seconds as hours, minutes, and seconds.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        Duration = 2,

        /// <summary>
        /// Indicates to create a numbering system formatter to format a number in
        /// a rules-based numbering system such as <c>%hebrew</c> for Hebrew numbers or <c>%roman-upper</c>
        /// for upper-case Roman numerals.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        NumberingSystem = 3,
    }

    /// <summary>
    /// Extensions to <see cref="NumberPresentation"/>.
    /// </summary>
    internal static class NumberPresentationExtensions
    {
        /// <summary>
        /// Returns a boolean telling whether a given integral value, or its name as a string, exists in
        /// a specified enumeration.
        /// </summary>
        /// <param name="presentation">The value or name of a constant.</param>
        /// <returns><c>true</c> if a given integral value, or its name as a string, exists in a specified
        /// enumeration; <c>false</c> otherwise.</returns>
        internal static bool IsDefined(this NumberPresentation presentation)
            => presentation >= NumberPresentation.SpellOut && presentation <= NumberPresentation.NumberingSystem;

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
}
