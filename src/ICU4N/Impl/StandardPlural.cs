using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

//
// Ported from icu4j/main/classes/core/src/com/ibm/icu/impl/StandardPlural.java
//
namespace ICU4N.Impl
{
    /// <summary>
    /// Standard CLDR plural form/category constants.
    /// See http://www.unicode.org/reports/tr35/tr35-numbers.html#Language_Plural_Rules
    /// </summary>
    public enum StandardPlural
    {
        /// <summary>
        /// Zero
        /// </summary>
        Zero,
        /// <summary>
        /// One
        /// </summary>
        One,
        /// <summary>
        /// Two
        /// </summary>
        Two,
        /// <summary>
        /// Few
        /// </summary>
        Few,
        /// <summary>
        /// Many
        /// </summary>
        Many,
        /// <summary>
        /// Other
        /// </summary>
        Other
    }

    /// <summary>
    /// Utilities for working with the <see cref="StandardPlural"/> enum.
    /// </summary>
    public static partial class StandardPluralUtil
    {
        private static readonly IList<StandardPlural> values =
#if FEATURE_ILIST_ASREADONLY
            System.Collections.Generic.CollectionExtensions.AsReadOnly(Enum.GetValues<StandardPlural>());
#else
            ((StandardPlural[])Enum.GetValues(typeof(StandardPlural))).AsReadOnly();
#endif

        /// <summary>
        /// Gets an unmodifiable List of all standard plural form constants.
        /// <see cref="IList{T}"/> version of <see cref="StandardPlural"/>.
        /// </summary>
        public static IList<StandardPlural> Values => values;

        /// <summary>
        /// Gets the number of standard plural forms/categories.
        /// </summary>
        public static int Count => values.Count;

        /// <summary>
        /// The lowercase CLDR keyword string for the plural form.
        /// </summary>
        /// <param name="standardPlural">This <see cref="StandardPlural"/>.</param>
        /// <returns>The lowercase CLDR keyword string for the plural form.</returns>
        public static string GetKeyword(this StandardPlural standardPlural)
        {
            switch (standardPlural)
            {
                case StandardPlural.Zero: return "zero";
                case StandardPlural.One: return "one";
                case StandardPlural.Two: return "two";
                case StandardPlural.Few: return "few";
                case StandardPlural.Many: return "many";
                case StandardPlural.Other: return "other";
                default: throw new ArgumentOutOfRangeException(nameof(standardPlural));
            }
        }

        // ICU4N specific: OrNullFromString(ICharSequence keyword) moved to StandardPluralExtension.tt

        // ICU4N specific: OrOtherFromString(ICharSequence keyword) moved to StandardPluralExtension.tt

        // ICU4N specific: FromString(ICharSequence keyword) moved to StandardPluralExtension.tt and
        // made into TryFromString

        // ICU4N specific: IndexOrNegativeFromString(ICharSequence keyword) moved to StandardPluralExtension.tt

        // ICU4N specific: IndexOrOtherIndexFromString(ICharSequence keyword) moved to StandardPluralExtension.tt

        // ICU4N specific: IndexFromString(ICharSequence keyword) moved to StandardPluralExtension.tt
        // and made into TryIndexFromString
    }
}
