using System;
#if FEATURE_SPAN
namespace ICU4N.Text
{
    /// <summary>
    /// Extensions to the <see cref="ReadOnlySpan{T}"/> and <see cref="string"/> to allow tokenization without allocating substrings.
    /// </summary>
    internal static class SplitTokenizerExtensions
    {
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiter.Length, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, int delimiterLength)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiterLength, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, string trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiter.Length, trimChars.AsSpan(), TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, string trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiter.Length, trimChars.AsSpan(), trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, int delimiterLength, string trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiterLength, trimChars.AsSpan(), TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, int delimiterLength, string trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiterLength, trimChars.AsSpan(), trimBehavior);
        }

        // string, ReadOnlySpan<char>

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiter.Length, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiter.Length, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, int delimiterLength, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiterLength, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, string delimiter, int delimiterLength, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter.AsSpan(), delimiterLength, trimChars, trimBehavior);
        }
#endif


        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, int delimiterLength)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiter between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char delimiter)
        {
            return new SplitTokenizerEnumerator(text, delimiter, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The characater to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char delimiter, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiter">The characater to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, trimBehavior);
        }



        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiter.Length, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter, int delimiterLength)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiterLength, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiter.Length, trimChars, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, trimBehavior);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiter.Length, trimChars, trimBehavior);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiterLength, trimChars, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this string text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, trimBehavior);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, delimiterLength, trimChars, trimBehavior);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiter between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, char delimiter)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, char delimiter, ReadOnlySpan<char> trimChars)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, TrimBehavior.StartAndEnd);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, trimChars, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this string text, char delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, trimBehavior);
#else
            return new SplitTokenizerEnumerator(text.AsSpan(), delimiter, trimChars, trimBehavior);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, int delimiterLength)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, int delimiterLength, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, ReadOnlySpan<char>.Empty, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiter.Length, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The sequence to consider delimiters between tokens.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, delimiterLength, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiter between tokens.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, char delimiter)
        {
            return new SplitTokenizerEnumerator(text, delimiter, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiter between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, char delimiter, ReadOnlySpan<char> trimChars)
        {
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiter"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiter">The characater to consider a delimiter between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="SplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static SplitTokenizerEnumerator AsTokens(this char[] text, char delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new SplitTokenizerEnumerator(text, delimiter, trimChars, trimBehavior);
        }

        // ------------------------------------------------------------------
        // Multi-Delimiter Overloads - Similar to PluralRules.SimpleTokenizer
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiters"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char[] delimiters)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char[] delimiters, ReadOnlySpan<char> trimChars)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="ReadOnlySpan{T}"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this ReadOnlySpan<char> text, char[] delimiters, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, trimBehavior);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiters"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this string text, char[] delimiters)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#else
            return new MultiDelimiterSplitTokenizerEnumerator(text.AsSpan(), delimiters, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this string text, char[] delimiters, ReadOnlySpan<char> trimChars)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, TrimBehavior.StartAndEnd);
#else
            return new MultiDelimiterSplitTokenizerEnumerator(text.AsSpan(), delimiters, trimChars, TrimBehavior.StartAndEnd);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="string"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="string"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this string text, char[] delimiters, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
#if FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, trimBehavior);
#else
            return new MultiDelimiterSplitTokenizerEnumerator(text.AsSpan(), delimiters, trimChars, trimBehavior);
#endif
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiters"/>. This is
        /// intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this char[] text, char[] delimiters)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, ReadOnlySpan<char>.Empty, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this char[] text, char[] delimiters, ReadOnlySpan<char> trimChars)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, TrimBehavior.StartAndEnd);
        }

        /// <summary>
        /// Creates an enumerator that splits this <see cref="T:char[]"/> based on the <paramref name="delimiters"/>.
        /// Trims the <paramref name="trimChars"/> from each token. This is intended for use within a foreach loop.
        /// </summary>
        /// <param name="text">This <see cref="T:char[]"/>.</param>
        /// <param name="delimiters">The character(s) to consider delimiters between tokens.</param>
        /// <param name="trimChars">The span which contains the set of characters to remove.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <returns>A <see cref="MultiDelimiterSplitTokenizerEnumerator"/> that can be used to enumerate the tokens.</returns>
        public static MultiDelimiterSplitTokenizerEnumerator AsTokens(this char[] text, char[] delimiters, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            return new MultiDelimiterSplitTokenizerEnumerator(text, delimiters, trimChars, trimBehavior);
        }
    }

    /// <summary>
    /// Bitwise flags to use to control the trim behavior of <see cref="SplitTokenizerEnumerator"/> and
    /// <see cref="MultiDelimiterSplitTokenizerEnumerator"/>.
    /// </summary>
    [Flags]
    internal enum TrimBehavior
    {
        /// <summary>
        /// Trim the beginning of the token.
        /// </summary>
        Start = 1,

        /// <summary>
        /// Trim the end of the token.
        /// </summary>
        End = 2,

        /// <summary>
        /// Trim both the beginning and the end of the token.
        /// </summary>
        StartAndEnd = Start | End,
    }


    /// <summary>
    /// Effectively does a non-allocating split operation, splitting on the delimiter and then trimming each token
    /// using trimChars.
    /// </summary>
    internal ref struct SplitTokenizerEnumerator
    {
        /// <summary>
        /// These are the characters used by the Java Pattern class, which is a subset of the characters that are used in the .NET Regex class for "\s".
        /// <para/>
        /// Note this set is slightly different than <see cref="ICU4N.Impl.PatternProps.WhiteSpace"/>,
        /// which is the set matched from <see cref="ICU4N.Impl.PatternProps.IsWhiteSpace(int)"/>.
        /// </summary>
        public static readonly char[] PatternWhiteSpace = new char[] { (char)0x09, (char)0x0a, (char)0x0b, (char)0x0c, (char)0x0d, (char)0x20 };

        private ReadOnlySpan<char> text;
#pragma warning disable IDE0044, S2933 // Add readonly modifier
        private ReadOnlySpan<char> multiCharDelimiter;
        private char delimiter;
        private bool isSingleCharDelimiter;
        private int delimiterLength;
        private TrimBehavior trimBehavior;
        private ReadOnlySpan<char> trimChars;
#pragma warning restore IDE0044, S2933 // Add readonly modifier


        /// <summary>
        /// Creates an enumerator that tokenizes <paramref name="text"/>, splitting on the <paramref name="delimiter"/> and
        /// then trimming each segment using <paramref name="trimChars"/>. This implements the Enumerator contract in C# so it can
        /// be used inside of a foreach statement.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="delimiter">The sequence of characters to split on.</param>
        /// <param name="delimiterLength">The actual length of the delimiter. This is the value used when determining the strings to return.
        /// This can be used to adjust the beginning of the text in the output, for example, matching on the string ';%' the actual length is 2,
        /// but including the '%' character as part of the (next) output string, this can be specified as 1 here.</param>
        /// <param name="trimChars">The characters to trim from each token.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delimiterLength"/> is less than zero.</exception>
        public SplitTokenizerEnumerator(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, int delimiterLength, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            if (delimiterLength < 0)
                throw new ArgumentOutOfRangeException(nameof(delimiterLength), SR.ArgumentOutOfRange_NeedNonNegNum);
            this.text = text;
            this.multiCharDelimiter = delimiter;
            this.delimiter = default;
            this.isSingleCharDelimiter = false;
            this.delimiterLength = delimiterLength;
            this.trimBehavior = trimBehavior;
            this.trimChars = trimChars;
            Current = default;
        }

        /// <summary>
        /// Creates an enumerator that tokenizes <paramref name="text"/>, splitting on the <paramref name="delimiter"/> and
        /// then trimming each segment using <paramref name="trimChars"/>. This implements the Enumerator contract in C# so it can
        /// be used inside of a foreach statement.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="delimiter">The character to split on.</param>
        /// <param name="trimChars">The characters to trim from the beginning and end of each token.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        public SplitTokenizerEnumerator(ReadOnlySpan<char> text, char delimiter, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            this.text = text;
            this.multiCharDelimiter = default;
            this.delimiter = delimiter;
            this.isSingleCharDelimiter = true;
            this.delimiterLength = 1;
            this.trimBehavior = trimBehavior;
            this.trimChars = trimChars;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public SplitTokenizerEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = text;
            if (span.Length == 0) // Reached the end of the string
                return false;

            var index = isSingleCharDelimiter ? span.IndexOf(delimiter) : span.IndexOf(multiCharDelimiter, StringComparison.Ordinal);
            if (index == -1) // The string is composed of only 1 segment
            {
                text = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new SplitEntry(Trim(span), ReadOnlySpan<char>.Empty);
                return true;
            }

            // We do the split and then trim off the unwanted characters before returning the entry.
            Current = new SplitEntry(Trim(span.Slice(0, index)), span.Slice(index, delimiterLength));
            text = span.Slice(index + delimiterLength); // Skip the matched chars.
            return true;
        }

        /// <summary>
        /// Trims the string as specified in the constructor.
        /// </summary>
        private ReadOnlySpan<char> Trim(ReadOnlySpan<char> text)
        {
            if (trimChars.Length == 0)
                return text;
            
            switch (trimBehavior)
            {
                case TrimBehavior.StartAndEnd:
                    return text.Trim(trimChars);
                case TrimBehavior.Start:
                    return text.TrimStart(trimChars);
                case TrimBehavior.End:
                    return text.TrimEnd(trimChars);
                default:
                    return text.Trim(trimChars);
            }
        }

        public SplitEntry Current { get; private set; }


        internal static class SR
        {
            public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
        }
    }


    /// <summary>
    /// Effectively does a non-allocating split operation, splitting on any of the delimiters and then trimming each token
    /// using trimChars.
    /// </summary>
    internal ref struct MultiDelimiterSplitTokenizerEnumerator
    {
        private ReadOnlySpan<char> text;
#pragma warning disable IDE0044, S2933 // Add readonly modifier
        private ReadOnlySpan<char> delimiters;
        private TrimBehavior trimBehavior;
        private ReadOnlySpan<char> trimChars;
#pragma warning restore IDE0044, S2933 // Add readonly modifier

        /// <summary>
        /// Creates an enumerator that tokenizes <paramref name="text"/>, splitting on any of the <paramref name="delimiters"/> and
        /// then trimming each segment using <paramref name="trimChars"/>. This implements the Enumerator contract in C# so it can
        /// be used inside of a foreach statement.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="delimiters">The character(s) to split on.</param>
        /// <param name="trimChars">The characters to trim from the beginning and end of each token.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        public MultiDelimiterSplitTokenizerEnumerator(ReadOnlySpan<char> text, char[] delimiters, ReadOnlySpan<char> trimChars, TrimBehavior trimBehavior)
        {
            this.text = text;
            this.delimiters = delimiters;
            this.trimBehavior = trimBehavior;
            this.trimChars = trimChars;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public MultiDelimiterSplitTokenizerEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = text;
            if (span.Length == 0) // Reached the end of the string
                return false;

            var index = span.IndexOfAny(delimiters);
            if (index == -1) // The string is composed of only 1 segment
            {
                text = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new SplitEntry(Trim(span), ReadOnlySpan<char>.Empty);
                return true;
            }

            // We do the split and then trim off the unwanted characters before returning the entry.
            Current = new SplitEntry(Trim(span.Slice(0, index)), span.Slice(index, 1));
            text = span.Slice(index + 1); // Skip the matched char.
            return true;
        }

        /// <summary>
        /// Trims the string as specified in the constructor.
        /// </summary>
        private ReadOnlySpan<char> Trim(ReadOnlySpan<char> text)
        {
            if (trimChars.Length == 0)
                return text;

            switch (trimBehavior)
            {
                case TrimBehavior.StartAndEnd:
                    return text.Trim(trimChars);
                case TrimBehavior.Start:
                    return text.TrimStart(trimChars);
                case TrimBehavior.End:
                    return text.TrimEnd(trimChars);
                default:
                    return text.Trim(trimChars);
            }
        }

        public SplitEntry Current { get; private set; }
    }
}
#endif