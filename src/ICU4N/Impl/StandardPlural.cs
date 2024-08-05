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

        private static readonly string[] keywords =
#if FEATURE_ILIST_ASREADONLY
            Enum.GetNames<StandardPlural>()
#else
            ((string[])Enum.GetNames(typeof(StandardPlural)))
#endif
            .Select(n => n.ToLowerInvariant()).ToArray();

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

        // ICU4N: Factored out OrNullFromString and replaced with TryGetValue()

        // ICU4N: Refactored OrOtherFromString(ICharSequence keyword) to GetValueOrOther()

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural GetValueOrOther(ResourceKey keyword)
            => TryGetValue(keyword, out StandardPlural p) ? p : StandardPlural.Other;

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural GetValueOrOther(string keyword)
            => TryGetValue(keyword, out StandardPlural p) ? p : StandardPlural.Other;


        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural GetValueOrOther(ReadOnlySpan<char> keyword)
            => TryGetValue(keyword, out StandardPlural p) ? p : StandardPlural.Other;

        // ICU4N specific: Refactored FromString(ICharSequence keyword) to TryGetValue

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryGetValue(ResourceKey keyword, out StandardPlural result)
        {
            if (keyword is null)
                throw new ArgumentNullException(nameof(keyword));

            switch (keyword.Length)
            {
                case 3:
                    if (keyword.SequenceEqual("one"))
                    {
                        result = StandardPlural.One;
                        return true;
                    }
                    else if (keyword.SequenceEqual("two"))
                    {
                        result = StandardPlural.Two;
                        return true;
                    }
                    else if (keyword.SequenceEqual("few"))
                    {
                        result = StandardPlural.Few;
                        return true;
                    }
                    break;
                case 4:
                    if (keyword.SequenceEqual("many"))
                    {
                        result = StandardPlural.Many;
                        return true;
                    }
                    else if (keyword.SequenceEqual("zero"))
                    {
                        result = StandardPlural.Zero;
                        return true;
                    }
                    break;
                case 5:
                    if (keyword.SequenceEqual("other"))
                    {
                        result = StandardPlural.Other;
                        return true;
                    }
                    break;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryGetValue(string keyword, out StandardPlural result)
        {
            if (keyword is null)
                throw new ArgumentNullException(nameof(keyword));

            switch (keyword.Length)
            {
                case 3:
                    if ("one".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.One;
                        return true;
                    }
                    else if ("two".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Two;
                        return true;
                    }
                    else if ("few".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Few;
                        return true;
                    }
                    break;
                case 4:
                    if ("many".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Many;
                        return true;
                    }
                    else if ("zero".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Zero;
                        return true;
                    }
                    break;
                case 5:
                    if ("other".Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Other;
                        return true;
                    }
                    break;
            }
            result = default;
            return false;
        }


        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryGetValue(ReadOnlySpan<char> keyword, out StandardPlural result)
        {
            switch (keyword.Length)
            {
                case 3:
                    if ("one".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.One;
                        return true;
                    }
                    else if ("two".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Two;
                        return true;
                    }
                    else if ("few".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Few;
                        return true;
                    }
                    break;
                case 4:
                    if ("many".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Many;
                        return true;
                    }
                    else if ("zero".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Zero;
                        return true;
                    }
                    break;
                case 5:
                    if ("other".AsSpan().Equals(keyword, StringComparison.Ordinal))
                    {
                        result = StandardPlural.Other;
                        return true;
                    }
                    break;
            }
            result = default;
            return false;
        }

        // ICU4N specific: Factored out IndexOrNegativeFromString(ICharSequence keyword) because we can use TryGetValue() and pick a default in .NET

        // ICU4N specific: Factored out IndexOrOtherIndexFromString(ICharSequence keyword) because we can use GetValueOrOther() and cast to int in .NET

        // ICU4N specific: Factored out IndexFromString(ICharSequence keyword) because we can use TryGetValue() and cast to int in .NET
    }
}
