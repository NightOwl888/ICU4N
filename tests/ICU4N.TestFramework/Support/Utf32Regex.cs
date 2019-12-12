using ICU4N.Support.Text;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ICU4N.Support
{
    /// <summary>
    /// Patches the <see cref="Regex"/> class so it will automatically convert and interpret
    /// UTF32 characters expressed like <c>\U00010000</c> or UTF32 ranges expressed
    /// like <c>\U00010000-\U00010001</c>.
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class Utf32Regex : Regex // ICU4N TODO: This might be a useful tool to have in ICU4N.dll
    {
        private const char MinLowSurrogate = '\uDC00';
        private const char MaxLowSurrogate = '\uDFFF';

        private const char MinHighSurrogate = '\uD800';
        private const char MaxHighSurrogate = '\uDBFF';

        // Match any character class such as [A-z]
        private static readonly Regex characterClass = new Regex(
            "(?<!\\\\)(\\[.*?(?<!\\\\)\\])",
            RegexOptions.Compiled);

        // Match a UTF32 range such as \U000E01F0-\U000E0FFF
        // or an individual character such as \U000E0FFF
        private static readonly Regex utf32Range = new Regex(
            "(?<begin>\\\\U(?:00)?[0-9A-Fa-f]{6})-(?<end>\\\\U(?:00)?[0-9A-Fa-f]{6})|(?<begin>\\\\U(?:00)?[0-9A-Fa-f]{6})",
            RegexOptions.Compiled);

        public Utf32Regex()
            : base()
        {
        }

        public Utf32Regex(string pattern)
            : base(ConvertUTF32Characters(pattern))
        {
        }

        public Utf32Regex(string pattern, RegexOptions options)
            : base(ConvertUTF32Characters(pattern), options)
        {
        }

        public Utf32Regex(string pattern, RegexOptions options, TimeSpan matchTimeout)
            : base(ConvertUTF32Characters(pattern), options, matchTimeout)
        {
        }

#if FEATURE_SERIALIZABLE
        protected Utf32Regex(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
#endif

        private static string ConvertUTF32Characters(string regexString)
        {
            StringBuilder result = new StringBuilder();
            // Convert any UTF32 character ranges \U00000000-\U00FFFFFF to their
            // equivalent UTF16 characters
            ConvertUTF32CharacterClassesToUTF16Characters(regexString, result);
            // Now find all of the individual characters that were not in ranges and
            // fix those as well.
            ConvertUTF32CharactersToUTF16(result);

            return result.ToString();
        }

        private static void ConvertUTF32CharacterClassesToUTF16Characters(string regexString, StringBuilder result)
        {
            Match match = characterClass.Match(regexString); // Reset
            int lastEnd = 0;
            if (match.Success)
            {
                do
                {
                    string characterClass = match.Groups[1].Value;
                    string convertedCharacterClass = ConvertUTF32CharacterRangesToUTF16Characters(characterClass);

                    result.Append(regexString.Substring(lastEnd, match.Index - lastEnd)); // Remove the match
                    result.Append(convertedCharacterClass); // Append replacement 

                    lastEnd = match.Index + match.Length;
                } while ((match = match.NextMatch()).Success);
            }
            result.Append(regexString.Substring(lastEnd)); // Append tail
        }

        private static string ConvertUTF32CharacterRangesToUTF16Characters(string characterClass)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder chars = new StringBuilder();

            Match match = utf32Range.Match(characterClass); // Reset
            int lastEnd = 0;
            if (match.Success)
            {
                do
                {
                    string utf16Chars;
                    string rangeBegin = match.Groups["begin"].Value.Substring(2);

                    if (!string.IsNullOrEmpty(match.Groups["end"].Value))
                    {
                        string rangeEnd = match.Groups["end"].Value.Substring(2);
                        utf16Chars = UTF32RangeToUTF16Chars(rangeBegin, rangeEnd);
                    }
                    else
                    {
                        utf16Chars = UTF32ToUTF16Chars(rangeBegin);
                    }

                    result.Append(characterClass.Substring(lastEnd, match.Index - lastEnd)); // Remove the match
                    chars.Append(utf16Chars); // Append replacement 

                    lastEnd = match.Index + match.Length;
                } while ((match = match.NextMatch()).Success);
            }
            result.Append(characterClass.Substring(lastEnd)); // Append tail of character class

            // Special case - if we have removed all of the contents of the
            // character class, we need to remove the square brackets and the
            // alternation character |
            int emptyCharClass = result.IndexOf("[]");
            if (emptyCharClass >= 0)
            {
                result.Remove(emptyCharClass, 2);
                // Append replacement ranges (exclude beginning |)
                result.Append(chars.ToString(1, chars.Length - 1));
            }
            else
            {
                // Append replacement ranges
                result.Append(chars.ToString());
            }

            if (chars.Length > 0)
            {
                // Wrap both the character class and any UTF16 character alteration into
                // a non-capturing group.
                return "(?:" + result.ToString() + ")";
            }
            return result.ToString();
        }

        private static void ConvertUTF32CharactersToUTF16(StringBuilder result)
        {
            while (true)
            {
                int where = result.IndexOf("\\U00");
                if (where < 0)
                {
                    break;
                }
                string cp = UTF32ToUTF16Chars(result.ToString(where + 2, 8));
                result.Replace(where, 10, cp); // ICU4N: Corrected 2nd parameter
            }
        }

        private static string UTF32RangeToUTF16Chars(string hexBegin, string hexEnd)
        {
            var result = new StringBuilder();
            int beginCodePoint = int.Parse(hexBegin, NumberStyles.HexNumber);
            int endCodePoint = int.Parse(hexEnd, NumberStyles.HexNumber);

            var beginChars = char.ConvertFromUtf32(beginCodePoint);
            var endChars = char.ConvertFromUtf32(endCodePoint);
            int beginDiff = endChars[0] - beginChars[0];

            if (beginDiff == 0)
            {
                // If the begin character is the same, we can just use the syntax \uD807[\uDDEF-\uDFFF]
                result.Append('|');
                AppendUTF16Character(result, beginChars[0]);
                result.Append('[');
                AppendUTF16Character(result, beginChars[1]);
                result.Append('-');
                AppendUTF16Character(result, endChars[1]);
                result.Append(']');
            }
            else
            {
                // If the begin character is not the same, create 3 ranges
                // 1. The remainder of the first
                // 2. A range of all of the middle characters
                // 3. The beginning of the last

                result.Append('|');
                AppendUTF16Character(result, beginChars[0]);
                result.Append('[');
                AppendUTF16Character(result, beginChars[1]);
                result.Append('-');
                AppendUTF16Character(result, MaxLowSurrogate);
                result.Append(']');

                // We only need a middle range if the ranges are not adjacent
                if (beginDiff > 1)
                {
                    result.Append('|');
                    // We only need a character class if there are more than 1
                    // characters in the middle range
                    if (beginDiff > 2)
                    {
                        result.Append('[');
                    }
                    AppendUTF16Character(result, (char)(Math.Min(beginChars[0] + 1, MaxHighSurrogate)));
                    if (beginDiff > 2)
                    {
                        result.Append('-');
                        AppendUTF16Character(result, (char)(Math.Max(endChars[0] - 1, MinHighSurrogate)));
                        result.Append(']');
                    }
                    result.Append('[');
                    AppendUTF16Character(result, MinLowSurrogate);
                    result.Append('-');
                    AppendUTF16Character(result, MaxLowSurrogate);
                    result.Append(']');
                }

                result.Append('|');
                AppendUTF16Character(result, endChars[0]);
                result.Append('[');
                AppendUTF16Character(result, MinLowSurrogate);
                result.Append('-');
                AppendUTF16Character(result, endChars[1]);
                result.Append(']');
            }
            return result.ToString();
        }

        private static string UTF32ToUTF16Chars(string hex)
        {
            int codePoint = int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return UTF32ToUTF16Chars(codePoint);
        }

        private static string UTF32ToUTF16Chars(int codePoint)
        {
            StringBuilder result = new StringBuilder();
            AppendUTF16CodePoint(result, codePoint);
            return result.ToString();
        }

        private static void AppendUTF16CodePoint(StringBuilder text, int cp)
        {
            var chars = char.ConvertFromUtf32(cp);
            AppendUTF16Character(text, chars[0]);
            if (chars.Length == 2)
            {
                AppendUTF16Character(text, chars[1]);
            }
        }

        private static void AppendUTF16Character(StringBuilder text, char c)
        {
            text.Append(@"\u");
            text.Append(Convert.ToString(c, 16).ToUpperInvariant());
        }
    }
}
