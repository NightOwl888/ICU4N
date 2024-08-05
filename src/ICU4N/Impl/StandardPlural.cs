using J2N.Collections.Generic.Extensions;
using J2N.Text;
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

        private static readonly string[] keywords = new string[] {
            "zero", "one", "two", "few", "many", "other"
        };

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
            int index = (int)standardPlural;
            if (index < 0 || index >= keywords.Length)
                throw new ArgumentOutOfRangeException(nameof(standardPlural));

            return keywords[index];
        }

        // ICU4N specific: OrNullFromString(ICharSequence keyword) moved to StandardPlural.generated.tt

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <c>null</c>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or null.</returns>
        public static StandardPlural? OrNullFromString(ResourceKey keyword)
        {
            switch (keyword.Length)
            {
                case 3:
                    if (keyword.SequenceEqual("one"))
                    {
                        return StandardPlural.One;
                    }
                    else if (keyword.SequenceEqual("two"))
                    {
                        return StandardPlural.Two;
                    }
                    else if (keyword.SequenceEqual("few"))
                    {
                        return StandardPlural.Few;
                    }
                    break;
                case 4:
                    if (keyword.SequenceEqual("many"))
                    {
                        return StandardPlural.Many;
                    }
                    else if (keyword.SequenceEqual("zero"))
                    {
                        return StandardPlural.Zero;
                    }
                    break;
                case 5:
                    if (keyword.SequenceEqual("other"))
                    {
                        return StandardPlural.Other;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        // ICU4N specific: OrOtherFromString(ICharSequence keyword) moved to StandardPlural.generated.tt

        // ICU4N specific: FromString(ICharSequence keyword) moved to StandardPlural.generated.tt and
        // made into TryFromString

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryFromString(ResourceKey keyword, out StandardPlural result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = p.Value;
                return true;
            }
            else
            {
                result = default(StandardPlural);
                return false;
            }
        }

        // ICU4N specific: IndexOrNegativeFromString(ICharSequence keyword) moved to StandardPlural.generated.tt

        // ICU4N specific: IndexOrOtherIndexFromString(ICharSequence keyword) moved to StandardPlural.generated.tt

        // ICU4N specific: IndexFromString(ICharSequence keyword) moved to StandardPlural.generated.tt
        // and made into TryIndexFromString
    }
}
