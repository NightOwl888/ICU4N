using ICU4N.Text;
using System;

namespace ICU4N.Numerics
{
    /// <summary>
    /// Identical to <see cref="ConstantMultiFieldModifier"/>, but supports currency spacing.
    /// </summary>
    internal class CurrencySpacingEnabledModifier : ConstantMultiFieldModifier
    {
        // These are the default currency spacing UnicodeSets in CLDR.
        // Pre-compute them for performance.
        // The unit test testCurrencySpacingPatternStability() will start failing if these change in CLDR.
        private static readonly UnicodeSet UNISET_DIGIT = new UnicodeSet("[:digit:]").Freeze();
        private static readonly UnicodeSet UNISET_NOTS = new UnicodeSet("[:^S:]").Freeze();

        // Constants for better readability. Types are for compiler checking.
        internal static readonly byte PREFIX = 0;
        internal static readonly byte SUFFIX = 1;
        internal static readonly short IN_CURRENCY = 0;
        internal static readonly short IN_NUMBER = 1;

        private readonly UnicodeSet afterPrefixUnicodeSet;
        private readonly string afterPrefixInsert;
        private readonly UnicodeSet beforeSuffixUnicodeSet;
        private readonly string beforeSuffixInsert;

        /// <summary> Safe code path </summary>
        public CurrencySpacingEnabledModifier(NumberStringBuilder prefix, NumberStringBuilder suffix, bool strong,
                DecimalFormatSymbols symbols)
                : base(prefix, suffix, strong)
        {
            // Check for currency spacing. Do not build the UnicodeSets unless there is
            // a currency code point at a boundary.
            if (prefix.Length > 0 && prefix.Fields[prefix.Length - 1] == NumberFormatField.Currency)
            {
                int prefixCp = prefix.GetLastCodePoint();
                UnicodeSet prefixUnicodeSet = GetUnicodeSet(symbols, IN_CURRENCY, PREFIX);
                if (prefixUnicodeSet.Contains(prefixCp))
                {
                    afterPrefixUnicodeSet = GetUnicodeSet(symbols, IN_NUMBER, PREFIX);
                    afterPrefixUnicodeSet.Freeze(); // no-op if set is already frozen
                    afterPrefixInsert = GetInsertString(symbols, PREFIX);
                }
                else
                {
                    afterPrefixUnicodeSet = null;
                    afterPrefixInsert = null;
                }
            }
            else
            {
                afterPrefixUnicodeSet = null;
                afterPrefixInsert = null;
            }
            if (suffix.Length > 0 && suffix.Fields[0] == NumberFormatField.Currency)
            {
                int suffixCp = suffix.GetLastCodePoint();
                UnicodeSet suffixUnicodeSet = GetUnicodeSet(symbols, IN_CURRENCY, SUFFIX);
                if (suffixUnicodeSet.Contains(suffixCp))
                {
                    beforeSuffixUnicodeSet = GetUnicodeSet(symbols, IN_NUMBER, SUFFIX);
                    beforeSuffixUnicodeSet.Freeze(); // no-op if set is already frozen
                    beforeSuffixInsert = GetInsertString(symbols, SUFFIX);
                }
                else
                {
                    beforeSuffixUnicodeSet = null;
                    beforeSuffixInsert = null;
                }
            }
            else
            {
                beforeSuffixUnicodeSet = null;
                beforeSuffixInsert = null;
            }
        }

        /// <summary> Safe code path </summary>
        public override int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
        {
            // Currency spacing logic
            int length = 0;
            if (rightIndex - leftIndex > 0 && afterPrefixUnicodeSet != null
                    && afterPrefixUnicodeSet.Contains(output.CodePointAt(leftIndex)))
            {
                // TODO: Should we use the CURRENCY field here?
                length += output.Insert(leftIndex, afterPrefixInsert, null);
            }
            if (rightIndex - leftIndex > 0 && beforeSuffixUnicodeSet != null
                    && beforeSuffixUnicodeSet.Contains(output.CodePointBefore(rightIndex)))
            {
                // TODO: Should we use the CURRENCY field here?
                length += output.Insert(rightIndex + length, beforeSuffixInsert, null);
            }

            // Call super for the remaining logic
            length += base.Apply(output, leftIndex, rightIndex + length);
            return length;
        }

        /// <summary> Unsafe code path </summary>
        public static int ApplyCurrencySpacing(NumberStringBuilder output, int prefixStart, int prefixLen, int suffixStart,
                int suffixLen, DecimalFormatSymbols symbols)
        {
            int length = 0;
            bool hasPrefix = (prefixLen > 0);
            bool hasSuffix = (suffixLen > 0);
            bool hasNumber = (suffixStart - prefixStart - prefixLen > 0); // could be empty string
            if (hasPrefix && hasNumber)
            {
                length += ApplyCurrencySpacingAffix(output, prefixStart + prefixLen, PREFIX, symbols);
            }
            if (hasSuffix && hasNumber)
            {
                length += ApplyCurrencySpacingAffix(output, suffixStart + length, SUFFIX, symbols);
            }
            return length;
        }

        /// <summary> Unsafe code path </summary>
        private static int ApplyCurrencySpacingAffix(NumberStringBuilder output, int index, byte affix,
                DecimalFormatSymbols symbols)
        {
            // NOTE: For prefix, output.fieldAt(index-1) gets the last field type in the prefix.
            // This works even if the last code point in the prefix is 2 code units because the
            // field value gets populated to both indices in the field array.
            NumberFormatField affixField = (affix == PREFIX) ? output.Fields[index - 1] : output.Fields[index];
            if (affixField != NumberFormatField.Currency)
            {
                return 0;
            }
            int affixCp = (affix == PREFIX) ? output.CodePointBefore(index) : output.CodePointAt(index);
            UnicodeSet affixUniset = GetUnicodeSet(symbols, IN_CURRENCY, affix);
            if (!affixUniset.Contains(affixCp))
            {
                return 0;
            }
            int numberCp = (affix == PREFIX) ? output.CodePointAt(index) : output.CodePointBefore(index);
            UnicodeSet numberUniset = GetUnicodeSet(symbols, IN_NUMBER, affix);
            if (!numberUniset.Contains(numberCp))
            {
                return 0;
            }
            string spacingString = GetInsertString(symbols, affix);

            // NOTE: This next line *inserts* the spacing string, triggering an arraycopy.
            // It would be more efficient if this could be done before affixes were attached,
            // so that it could be prepended/appended instead of inserted.
            // However, the build code path is more efficient, and this is the most natural
            // place to put currency spacing in the non-build code path.
            // TODO: Should we use the CURRENCY field here?
            return output.Insert(index, spacingString, null);
        }

        private static UnicodeSet GetUnicodeSet(DecimalFormatSymbols symbols, short position, byte affix)
        {
            string pattern = symbols
                    .GetPatternForCurrencySpacing(position == IN_CURRENCY ? CurrencySpacingPattern.CurrencyMatch
                            : CurrencySpacingPattern.SurroundingMatch, affix == SUFFIX);
            if (pattern.Equals("[:digit:]", StringComparison.Ordinal))
            {
                return UNISET_DIGIT;
            }
            else if (pattern.Equals("[:^S:]", StringComparison.Ordinal))
            {
                return UNISET_NOTS;
            }
            else
            {
                return new UnicodeSet(pattern);
            }
        }

        private static string GetInsertString(DecimalFormatSymbols symbols, byte affix)
        {
            return symbols.GetPatternForCurrencySpacing(CurrencySpacingPattern.InsertBetween, affix == SUFFIX);
        }
    }
}
