using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Numerics.Padder;

namespace ICU4N.Numerics
{
    /// <summary>
    /// Assorted utilities relating to decimal formatting pattern strings.
    /// </summary>
    internal class PatternStringUtils // ICU4N TODO: API - this was public in ICU4J
    {
        /**
     * Creates a pattern string from a property bag.
     *
     * <p>
     * Since pattern strings support only a subset of the functionality available in a property bag, a new property bag
     * created from the string returned by this function may not be the same as the original property bag.
     *
     * @param properties
     *            The property bag to serialize.
     * @return A pattern string approximately serializing the property bag.
     */
        public static string PropertiesToPatternString(DecimalFormatProperties properties)
        {
            StringBuilder sb = new StringBuilder();

            // Convenience references
            // The Math.Min() calls prevent DoS
            int dosMax = 100;
            int groupingSize = Math.Min(properties.SecondaryGroupingSize, dosMax);
            int firstGroupingSize = Math.Min(properties.GroupingSize, dosMax);
            int paddingWidth = Math.Min(properties.FormatWidth, dosMax);
            PadPosition? paddingLocation = properties.PadPosition;
            string paddingString = properties.PadString;
            int minInt = Math.Max(Math.Min(properties.MinimumIntegerDigits, dosMax), 0);
            int maxInt = Math.Min(properties.MaximumIntegerDigits, dosMax);
            int minFrac = Math.Max(Math.Min(properties.MinimumFractionDigits, dosMax), 0);
            int maxFrac = Math.Min(properties.MaximumFractionDigits, dosMax);
            int minSig = Math.Min(properties.MinimumSignificantDigits, dosMax);
            int maxSig = Math.Min(properties.MaximumSignificantDigits, dosMax);
            bool alwaysShowDecimal = properties.DecimalSeparatorAlwaysShown;
            int exponentDigits = Math.Min(properties.MinimumExponentDigits, dosMax);
            bool exponentShowPlusSign = properties.ExponentSignAlwaysShown;
            string pp = properties.PositivePrefix;
            string ppp = properties.PositivePrefixPattern;
            string ps = properties.PositiveSuffix;
            string psp = properties.PositiveSuffixPattern;
            string np = properties.NegativePrefix;
            string npp = properties.NegativePrefixPattern;
            string ns = properties.NegativeSuffix;
            string nsp = properties.NegativeSuffixPattern;

            // Prefixes
            if (ppp != null)
            {
                sb.Append(ppp);
            }
            AffixUtils.Escape(pp, sb);
            int afterPrefixPos = sb.Length;

            // Figure out the grouping sizes.
            int grouping1, grouping2, grouping;
            if (groupingSize != Math.Min(dosMax, -1) && firstGroupingSize != Math.Min(dosMax, -1)
                    && groupingSize != firstGroupingSize)
            {
                grouping = groupingSize;
                grouping1 = groupingSize;
                grouping2 = firstGroupingSize;
            }
            else if (groupingSize != Math.Min(dosMax, -1))
            {
                grouping = groupingSize;
                grouping1 = 0;
                grouping2 = groupingSize;
            }
            else if (firstGroupingSize != Math.Min(dosMax, -1))
            {
                grouping = groupingSize;
                grouping1 = 0;
                grouping2 = firstGroupingSize;
            }
            else
            {
                grouping = 0;
                grouping1 = 0;
                grouping2 = 0;
            }
            int groupingLength = grouping1 + grouping2 + 1;

            // Figure out the digits we need to put in the pattern.
            BigDecimal roundingInterval = properties.RoundingIncrement;
            StringBuilder digitsString = new StringBuilder();
            int digitsStringScale = 0;
            if (maxSig != Math.Min(dosMax, -1))
            {
                // Significant Digits.
                while (digitsString.Length < minSig)
                {
                    digitsString.Append('@');
                }
                while (digitsString.Length < maxSig)
                {
                    digitsString.Append('#');
                }
            }
            else if (roundingInterval != null)
            {
                // Rounding Interval.
                digitsStringScale = -roundingInterval.Scale;
                // TODO: Check for DoS here?
                string str = roundingInterval.ScaleByPowerOfTen(roundingInterval.Scale).ToPlainString();
                if (str[0] == '-')
                {
                    // TODO: Unsupported operation exception or fail silently?
                    digitsString.Append(str, 1, str.Length);
                }
                else
                {
                    digitsString.Append(str);
                }
            }
            while (digitsString.Length + digitsStringScale < minInt)
            {
                digitsString.Insert(0, '0');
            }
            while (-digitsStringScale < minFrac)
            {
                digitsString.Append('0');
                digitsStringScale--;
            }

            // Write the digits to the string builder
            int m0 = Math.Max(groupingLength, digitsString.Length + digitsStringScale);
            m0 = (maxInt != dosMax) ? Math.Max(maxInt, m0) - 1 : m0 - 1;
            int mN = (maxFrac != dosMax) ? Math.Min(-maxFrac, digitsStringScale) : digitsStringScale;
            for (int magnitude = m0; magnitude >= mN; magnitude--)
            {
                int di = digitsString.Length + digitsStringScale - magnitude - 1;
                if (di < 0 || di >= digitsString.Length)
                {
                    sb.Append('#');
                }
                else
                {
                    sb.Append(digitsString[di]);
                }
                if (magnitude > grouping2 && grouping > 0 && (magnitude - grouping2) % grouping == 0)
                {
                    sb.Append(',');
                }
                else if (magnitude > 0 && magnitude == grouping2)
                {
                    sb.Append(',');
                }
                else if (magnitude == 0 && (alwaysShowDecimal || mN < 0))
                {
                    sb.Append('.');
                }
            }

            // Exponential notation
            if (exponentDigits != Math.Min(dosMax, -1))
            {
                sb.Append('E');
                if (exponentShowPlusSign)
                {
                    sb.Append('+');
                }
                for (int i = 0; i < exponentDigits; i++)
                {
                    sb.Append('0');
                }
            }

            // Suffixes
            int beforeSuffixPos = sb.Length;
            if (psp != null)
            {
                sb.Append(psp);
            }
            AffixUtils.Escape(ps, sb);

            // Resolve Padding
            if (paddingWidth != -1)
            {
                while (paddingWidth - sb.Length > 0)
                {
                    sb.Insert(afterPrefixPos, '#');
                    beforeSuffixPos++;
                }
                int addedLength;
                switch (paddingLocation)
                {
                    case PadPosition.BeforePrefix:
                        addedLength = PatternStringUtils.EscapePaddingString(paddingString, sb, 0);
                        sb.Insert(0, '*');
                        afterPrefixPos += addedLength + 1;
                        beforeSuffixPos += addedLength + 1;
                        break;
                    case PadPosition.AfterPrefix:
                        addedLength = PatternStringUtils.EscapePaddingString(paddingString, sb, afterPrefixPos);
                        sb.Insert(afterPrefixPos, '*');
                        afterPrefixPos += addedLength + 1;
                        beforeSuffixPos += addedLength + 1;
                        break;
                    case PadPosition.BeforeSuffix:
                        PatternStringUtils.EscapePaddingString(paddingString, sb, beforeSuffixPos);
                        sb.Insert(beforeSuffixPos, '*');
                        break;
                    case PadPosition.AfterSuffix:
                        sb.Append('*');
                        PatternStringUtils.EscapePaddingString(paddingString, sb, sb.Length);
                        break;
                }
            }

            // Negative affixes
            // Ignore if the negative prefix pattern is "-" and the negative suffix is empty
            if (np != null || ns != null || (npp == null && nsp != null)
                    || (npp != null && (npp.Length != 1 || npp[0] != '-' || nsp.Length != 0)))
            {
                sb.Append(';');
                if (npp != null)
                    sb.Append(npp);
                AffixUtils.Escape(np, sb);
                // Copy the positive digit format into the negative.
                // This is optional; the pattern is the same as if '#' were appended here instead.
                sb.Append(sb, afterPrefixPos, beforeSuffixPos); // ICU4N TODO: Find a way to append a partial StringBuilder without allocating
                if (nsp != null)
                    sb.Append(nsp);
                AffixUtils.Escape(ns, sb);
            }

            return sb.ToString();
        }

        /** @return The number of chars inserted. */
        //private static int EscapePaddingString(ICharSequence input, StringBuilder output, int startIndex)
        private static int EscapePaddingString(string input, StringBuilder output, int startIndex)
        {
            if (input is null || input.Length == 0)
                input = Padder.FallbackPaddingString;
            int startLength = output.Length;
            if (input.Length == 1)
            {
                if (input.Equals("'"))
                {
                    output.Insert(startIndex, "''");
                }
                else
                {
                    output.Insert(startIndex, input);
                }
            }
            else
            {
                output.Insert(startIndex, '\'');
                int offset = 1;
                for (int i = 0; i < input.Length; i++)
                {
                    // it's okay to deal in chars here because the quote mark is the only interesting thing.
                    char ch = input[i];
                    if (ch == '\'')
                    {
                        output.Insert(startIndex + offset, "''");
                        offset += 2;
                    }
                    else
                    {
                        output.Insert(startIndex + offset, ch);
                        offset += 1;
                    }
                }
                output.Insert(startIndex + offset, '\'');
            }
            return output.Length - startLength;
        }

        /**
         * Converts a pattern between standard notation and localized notation. Localized notation means that instead of
         * using generic placeholders in the pattern, you use the corresponding locale-specific characters instead. For
         * example, in locale <em>fr-FR</em>, the period in the pattern "0.000" means "decimal" in standard notation (as it
         * does in every other locale), but it means "grouping" in localized notation.
         *
         * <p>
         * A greedy string-substitution strategy is used to substitute locale symbols. If two symbols are ambiguous or have
         * the same prefix, the result is not well-defined.
         *
         * <p>
         * Locale symbols are not allowed to contain the ASCII quote character.
         *
         * <p>
         * This method is provided for backwards compatibility and should not be used in any new code.
         *
         * @param input
         *            The pattern to convert.
         * @param symbols
         *            The symbols corresponding to the localized pattern.
         * @param toLocalized
         *            true to convert from standard to localized notation; false to convert from localized to standard
         *            notation.
         * @return The pattern expressed in the other notation.
         */
        public static string ConvertLocalized(string input, DecimalFormatSymbols symbols, bool toLocalized)
        {
            if (input == null)
                return null;

            // Construct a table of strings to be converted between localized and standard.
            string[][] table = Arrays.NewRectangularArray<string>(21, 2); //new string[21][2];
            int standIdx = toLocalized ? 0 : 1;
            int localIdx = toLocalized ? 1 : 0;
            table[0][standIdx] = "%";
            table[0][localIdx] = symbols.PercentString;
            table[1][standIdx] = "‰";
            table[1][localIdx] = symbols.PerMillString;
            table[2][standIdx] = ".";
            table[2][localIdx] = symbols.DecimalSeparatorString;
            table[3][standIdx] = ",";
            table[3][localIdx] = symbols.GroupingSeparatorString;
            table[4][standIdx] = "-";
            table[4][localIdx] = symbols.MinusSignString;
            table[5][standIdx] = "+";
            table[5][localIdx] = symbols.PlusSignString;
            table[6][standIdx] = ";";
            table[6][localIdx] = char.ToString(symbols.PatternSeparator); //Character.ToString(symbols.PatternSeparator);
            table[7][standIdx] = "@";
            table[7][localIdx] = char.ToString(symbols.SignificantDigit); //Character.toString(symbols.getSignificantDigit());
            table[8][standIdx] = "E";
            table[8][localIdx] = symbols.ExponentSeparator;
            table[9][standIdx] = "*";
            table[9][localIdx] = char.ToString(symbols.PadEscape); //Character.toString(symbols.getPadEscape());
            table[10][standIdx] = "#";
            table[10][localIdx] = char.ToString(symbols.Digit); //Character.toString(symbols.getDigit());
            for (int i = 0; i < 10; i++)
            {
                table[11 + i][standIdx] = char.ToString((char)('0' + i)); //Character.toString((char)('0' + i));
                table[11 + i][localIdx] = symbols.DigitStringsLocal[i];
            }

            // Special case: quotes are NOT allowed to be in any localIdx strings.
            // Substitute them with '’' instead.
            for (int i = 0; i < table.Length; i++)
            {
                table[i][localIdx] = table[i][localIdx].Replace('\'', '’');
            }

            // Iterate through the string and convert.
            // State table:
            // 0 => base state
            // 1 => first char inside a quoted sequence in input and output string
            // 2 => inside a quoted sequence in input and output string
            // 3 => first char after a close quote in input string;
            // close quote still needs to be written to output string
            // 4 => base state in input string; inside quoted sequence in output string
            // 5 => first char inside a quoted sequence in input string;
            // inside quoted sequence in output string
            StringBuilder result = new StringBuilder();
            int state = 0;
        //outer:
            for (int offset = 0; offset < input.Length; offset++)
            {
                char ch = input[offset];

                // Handle a quote character (state shift)
                if (ch == '\'')
                {
                    if (state == 0)
                    {
                        result.Append('\'');
                        state = 1;
                        continue;
                    }
                    else if (state == 1)
                    {
                        result.Append('\'');
                        state = 0;
                        continue;
                    }
                    else if (state == 2)
                    {
                        state = 3;
                        continue;
                    }
                    else if (state == 3)
                    {
                        result.Append('\'');
                        result.Append('\'');
                        state = 1;
                        continue;
                    }
                    else if (state == 4)
                    {
                        state = 5;
                        continue;
                    }
                    else
                    {
                        Debug.Assert(state == 5);
                        result.Append('\'');
                        result.Append('\'');
                        state = 4;
                        continue;
                    }
                }

                if (state == 0 || state == 3 || state == 4)
                {
                    foreach (string[] pair in table)
                    {
                        // Perform a greedy match on this symbol string
                        if (input.RegionMatches(offset, pair[0], 0, pair[0].Length, StringComparison.Ordinal))
                        {
                            // Skip ahead past this region for the next iteration
                            offset += pair[0].Length - 1;
                            if (state == 3 || state == 4)
                            {
                                result.Append('\'');
                                state = 0;
                            }
                            result.Append(pair[1]);
                            //continue outer;
                            goto outer_continue;
                        }
                    }
                    // No replacement found. Check if a special quote is necessary
                    foreach (string[] pair in table)
                    {
                        if (input.RegionMatches(offset, pair[1], 0, pair[1].Length, StringComparison.Ordinal))
                        {
                            if (state == 0)
                            {
                                result.Append('\'');
                                state = 4;
                            }
                            result.Append(ch);
                            //continue outer;
                            goto outer_continue;
                        }
                    }
                    // Still nothing. Copy the char verbatim. (Add a close quote if necessary)
                    if (state == 3 || state == 4)
                    {
                        result.Append('\'');
                        state = 0;
                    }
                    result.Append(ch);
                }
                else
                {
                    Debug.Assert(state == 1 || state == 2 || state == 5);
                    result.Append(ch);
                    state = 2;
                }
            outer_continue: { /* Intentionally blank */ }
            }
            // Resolve final quotes
            if (state == 3 || state == 4)
            {
                result.Append('\'');
                state = 0;
            }
            if (state != 0)
            {
                throw new ArgumentException("Malformed localized pattern: unterminated quote"); // ICU4N TODO: FormatException?
            }
            return result.ToString();
        }
    }
}
