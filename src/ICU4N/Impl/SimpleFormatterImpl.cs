using ICU4N.Text;
using J2N.Text;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ICU4N.Impl
{
    // ICU4N TODO: API - Make this part of SimpleFormatter instead of having a separate class?
    // Upsides:
    // 1) Single type to interact with.
    // 2) Gets rid of the "Impl" in the class name.
    // Downsides:
    // 1) Difficult to tell the difference between the simmple and advanced APIs.

    /// <summary>
    /// Formats simple patterns like "{1} was born in {0}".
    /// Internal version of <see cref="SimpleFormatter"/>
    /// with only static methods, to avoid wrapper objects.
    /// </summary>
    /// <remarks>
    /// This class "compiles" pattern strings into a binary format
    /// and implements formatting etc. based on that.
    /// <para/>
    /// Format:
    /// Index 0: One more than the highest argument number.
    /// Followed by zero or more arguments or literal-text segments.
    /// <para/>
    /// An argument is stored as its number, less than <see cref="ARG_NUM_LIMIT"/>.
    /// A literal-text segment is stored as its length (at least 1) offset by <see cref="ARG_NUM_LIMIT"/>,
    /// followed by that many chars.
    /// </remarks>
    public static partial class SimpleFormatterImpl
    {
        private const int CharStackBufferSize = 32;

        /// <summary>
        /// Argument numbers must be smaller than this limit.
        /// Text segment lengths are offset by this much.
        /// This is currently the only unused char value in compiled patterns,
        /// except it is the maximum value of the first unit (max arg +1).
        /// </summary>
        private const int ARG_NUM_LIMIT = 0x100;
        private const char LEN1_CHAR = (char)(ARG_NUM_LIMIT + 1);
        private const char LEN2_CHAR = (char)(ARG_NUM_LIMIT + 2);
        private const char LEN3_CHAR = (char)(ARG_NUM_LIMIT + 3);

        /// <summary>
        /// Initial and maximum char/UChar value set for a text segment.
        /// Segment length char values are from ARG_NUM_LIMIT+1 to this value here.
        /// Normally 0xffff, but can be as small as ARG_NUM_LIMIT+1 for testing.
        /// </summary>
        private const char SEGMENT_LENGTH_ARGUMENT_CHAR = (char)0xffff;
        /// <summary>
        /// Maximum length of a text segment. Longer segments are split into shorter ones.
        /// </summary>
        private const int MAX_SEGMENT_LENGTH = SEGMENT_LENGTH_ARGUMENT_CHAR - ARG_NUM_LIMIT;

        /// <summary>
        /// "Intern" some common patterns.
        /// </summary>
        private static readonly string[][] COMMON_PATTERNS = {
            new string[] { "{0} {1}", "\u0002\u0000" + LEN1_CHAR + " \u0001" },
            new string[] { "{0} ({1})", "\u0002\u0000" + LEN2_CHAR + " (\u0001" + LEN1_CHAR + ')' },
            new string[] { "{0}, {1}", "\u0002\u0000" + LEN2_CHAR + ", \u0001" },
            new string[] { "{0} – {1}", "\u0002\u0000" + LEN3_CHAR + " – \u0001" },  // en dash
        };

        private static class SR
        {
            public const string Argument_PatternSyntaxError = "Argument syntax error in pattern \"{0}\" at index {1}: {2}";
            public const string Argument_FewerThanMinArguments = "Fewer than minimum \"{0}\" arguments in pattern \"{1}\".";
            public const string Argument_MoreThanMaxArguments = "More than maximum \"{0}\" arguments in pattern \"{1}\".";
            public const string Argument_TooFewValues = "Too few values.";
            public const string Argument_SameMemoryLocation = "\"{0}\" cannot be the same memory location as \"{1}\".";
        }

        #region CompileToStringMinMaxArguments/TryCompileToStringMinMaxArguments

        /// <summary>
        /// Creates a compiled form of the pattern string, for use with appropriate static methods.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <returns>The compiled-pattern string.</returns>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        public static string CompileToStringMinMaxArguments(
            ReadOnlySpan<char> pattern, int min, int max)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                CompileToStringMinMaxArguments(pattern, ref sb, min, max);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Creates a compiled form of the pattern string, for use with appropriate static methods.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <param name="destination">When this method returns successfully, contains the compiled pattern string.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <returns><b>true</b> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        public static bool TryCompileToStringMinMaxArguments(
            ReadOnlySpan<char> pattern, Span<char> destination, out int charsLength, int min, int max)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                CompileToStringMinMaxArguments(pattern, ref sb, min, max);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Creates a compiled form of the pattern string, for use with appropriate static methods.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <param name="sb">A <see cref="StringBuilder"/> buffer that will contain the output in immutable form.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <returns>The compiled-pattern string.</returns>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        internal static void CompileToStringMinMaxArguments(
            ReadOnlySpan<char> pattern, ref ValueStringBuilder sb, int min, int max)
        {
            // Return some precompiled common two-argument patterns.
            if (min <= 2 && 2 <= max)
            {
                foreach (string[] pair in COMMON_PATTERNS)
                {
                    if (pattern.Equals(pair[0], StringComparison.Ordinal))
                    {
                        Debug.Assert(pair[1][0] == 2);
                        sb.Length = 0;
                        sb.Append(pair[1]);
                        return;
                    }
                }
            }
            // Parse consistent with MessagePattern, but
            // - support only simple numbered arguments
            // - build a simple binary structure into the result string
            int patternLength = pattern.Length;
            sb.EnsureCapacity(patternLength);
            // Reserve the first char for the number of arguments.
            sb.Length = 1;
            int textLength = 0;
            int maxArg = -1;
            bool inQuote = false;
            for (int i = 0; i < patternLength;)
            {
                char c = pattern[i++];
                if (c == '\'')
                {
                    if (i < patternLength && (c = pattern[i]) == '\'')
                    {
                        // double apostrophe, skip the second one
                        ++i;
                    }
                    else if (inQuote)
                    {
                        // skip the quote-ending apostrophe
                        inQuote = false;
                        continue;
                    }
                    else if (c == '{' || c == '}')
                    {
                        // Skip the quote-starting apostrophe, find the end of the quoted literal text.
                        ++i;
                        inQuote = true;
                    }
                    else
                    {
                        // The apostrophe is part of literal text.
                        c = '\'';
                    }
                }
                else if (!inQuote && c == '{')
                {
                    if (textLength > 0)
                    {
                        sb[sb.Length - textLength - 1] = (char)(ARG_NUM_LIMIT + textLength);
                        textLength = 0;
                    }
                    int argNumber;
                    if ((i + 1) < patternLength &&
                            0 <= (argNumber = pattern[i] - '0') && argNumber <= 9 &&
                            pattern[i + 1] == '}')
                    {
                        i += 2;
                    }
                    else
                    {
                        // Multi-digit argument number (no leading zero) or syntax error.
                        // MessagePattern permits PatternProps.skipWhiteSpace(pattern, index)
                        // around the number, but this class does not.
                        int argStart = i - 1;
                        argNumber = -1;
                        if (i < patternLength && '1' <= (c = pattern[i++]) && c <= '9')
                        {
                            argNumber = c - '0';
                            while (i < patternLength && '0' <= (c = pattern[i++]) && c <= '9')
                            {
                                argNumber = argNumber * 10 + (c - '0');
                                if (argNumber >= ARG_NUM_LIMIT)
                                {
                                    break;
                                }
                            }
                        }
                        if (argNumber < 0 || c != '}')
                        {
                            throw new ArgumentException(
                                string.Format(SR.Argument_PatternSyntaxError,
                                pattern.ToString(),
                                argStart,
                                pattern.Slice(argStart, i - argStart).ToString())); // ICU4N: Corrected 2nd parameter
                        }
                    }
                    if (argNumber > maxArg)
                    {
                        maxArg = argNumber;
                    }
                    sb.Append((char)argNumber);
                    continue;
                }  // else: c is part of literal text
                   // Append c and track the literal-text segment length.
                if (textLength == 0)
                {
                    // Reserve a char for the length of a new text segment, preset the maximum length.
                    sb.Append(SEGMENT_LENGTH_ARGUMENT_CHAR);
                }
                sb.Append(c);
                if (++textLength == MAX_SEGMENT_LENGTH)
                {
                    textLength = 0;
                }
            }
            if (textLength > 0)
            {
                sb[sb.Length - textLength - 1] = (char)(ARG_NUM_LIMIT + textLength);
            }
            int argCount = maxArg + 1;
            if (argCount < min)
            {
                throw new ArgumentException(string.Format(SR.Argument_FewerThanMinArguments, min, pattern.ToString()));
            }
            if (argCount > max)
            {
                throw new ArgumentException(string.Format(SR.Argument_MoreThanMaxArguments, max, pattern.ToString()));
            }
            sb[0] = (char)argCount;
        }

        #endregion CompileToStringMinMaxArguments/TryCompileToStringMinMaxArguments

        #region GetArgumentLimit

        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <returns>The max argument number + 1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetArgumentLimit(ReadOnlySpan<char> compiledPattern)
        {
            return compiledPattern[0];
        }

        #endregion GetArgumentLimit

        #region FormatCompiledPattern/TryFormatCompiledPattern

        /// <summary>
        /// Formats the given values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="values">The argument values.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.</param>
        /// <returns>The formatted text.</returns>
        public static string FormatCompiledPattern(ReadOnlySpan<char> compiledPattern, params string[] values)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                FormatAndAppend(compiledPattern, ref sb, offsets: null, values);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Formats the given values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="destination">When this method returns successfully, contains the formatted text.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <param name="values">The argument values.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public static bool TryFormatCompiledPattern(ReadOnlySpan<char> compiledPattern, Span<char> destination, out int charsLength, params string[] values)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                FormatAndAppend(compiledPattern, ref sb, offsets: null, values);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        #endregion FormatCompiledPattern/TryFormatCompiledPattern

        #region FormatRawPattern/TryFormatRawPattern

        /// <summary>
        /// Formats the not-compiled pattern with the given values.
        /// Equivalent to <see cref="CompileToStringMinMaxArguments(ReadOnlySpan{Char}, int, int)"/> 
        /// followed by <see cref="FormatCompiledPattern(ReadOnlySpan{Char}, string[])"/>.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">Not-compiled form of a pattern string.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <param name="values">
        /// The argument values.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.
        /// </param>
        /// <returns>The compiled pattern string.</returns>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        public static string FormatRawPattern(ReadOnlySpan<char> pattern, int min, int max, params string[] values)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                FormatRawPattern(pattern, ref sb, min, max, values);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Formats the not-compiled pattern with the given values.
        /// Equivalent to <see cref="CompileToStringMinMaxArguments(ReadOnlySpan{Char}, int, int)"/> 
        /// followed by <see cref="FormatCompiledPattern(ReadOnlySpan{Char}, string[])"/>.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">Not-compiled form of a pattern string.</param>
        /// <param name="destination">When this method returns successfully, contains the compiled pattern string.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <param name="values">
        /// The argument values.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.
        /// </param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        public static bool TryFormatRawPattern(ReadOnlySpan<char> pattern, Span<char> destination, out int charsLength, int min, int max, params string[] values)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                FormatRawPattern(pattern, ref sb, min, max, values);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Formats the not-compiled pattern with the given values.
        /// Equivalent to <see cref="CompileToStringMinMaxArguments(ReadOnlySpan{Char}, int, int)"/> 
        /// followed by <see cref="FormatCompiledPattern(ReadOnlySpan{Char}, string[])"/>.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">Not-compiled form of a pattern string.</param>
        /// <param name="appendTo">The builder to append the compiled pattern string to.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <param name="values">The argument values.</param>
        /// <exception cref="ArgumentException">for bad argument syntax and too few or too many arguments.</exception>
        internal static void FormatRawPattern(ReadOnlySpan<char> pattern, ref ValueStringBuilder appendTo, int min, int max, params string[] values)
        {
            ValueStringBuilder compiledPattern = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                CompileToStringMinMaxArguments(pattern, ref compiledPattern, min, max);
                FormatAndAppend(compiledPattern.AsSpan(), ref appendTo, offsets: null, values);
            }
            finally
            {
                compiledPattern.Dispose();
            }
        }

        #endregion FormatRawPattern/TryFormatRawPattern

        #region FormatAndAppend

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value must not be the same object as <paramref name="appendTo"/>.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.
        /// </param>
        /// <returns></returns>
        internal static StringBuilder FormatAndAppend(
            ReadOnlySpan<char> compiledPattern, StringBuilder appendTo, Span<int> offsets, params string[] values) // ICU4N TODO: API - factor out or change to IAppendable.
        {
            int valuesLength = values != null ? values.Length : 0;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                sb.Append(appendTo);
                Format(compiledPattern, values, ref sb /*, null, true*/, offsets);
                return appendTo.Append(sb.AsSpan(appendTo.Length));
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value must not be the same object as <paramref name="appendTo"/>.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// Can be <c>null</c> if <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>==0.
        /// </param>
        internal static void FormatAndAppend(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder appendTo, Span<int> offsets, params string[] values)
        {
            int valuesLength = values != null ? values.Length : 0;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }
            Format(compiledPattern, values, ref appendTo /*, null, true*/, offsets);
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </param>
        internal static void FormatAndAppend(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder appendTo, Span<int> offsets, ReadOnlySpan<char> value0)
        {
            int valuesLength = 1;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }
            Format(compiledPattern, value0, default, default, ref appendTo, resultCopy: default, forbidResultAsValue: true, offsets);
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// The number of arguments passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </param>
        /// <param name="value1">
        /// The second argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndAppend(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder appendTo, Span<int> offsets, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1)
        {
            int valuesLength = 2;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }
            Format(compiledPattern, value0, value1, default, ref appendTo, resultCopy: default, forbidResultAsValue: true, offsets);
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// </param>
        /// <param name="value1">
        /// The second argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// </param>
        /// <param name="value2">
        /// The third argument value.
        /// An argument value must not be the same memory location as <paramref name="appendTo"/>.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndAppend(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder appendTo, Span<int> offsets, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, ReadOnlySpan<char> value2)
        {
            int valuesLength = 3;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }
            Format(compiledPattern, value0, value1, value2, ref appendTo, resultCopy: default, forbidResultAsValue: true, offsets);
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value must not be the same object as <paramref name="appendTo"/>.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </param>
        internal static void FormatAndAppend(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder appendTo, Span<int> offsets, ReadOnlySpanArray<char> values)
        {
            int valuesLength = values.Length;
            if (valuesLength < GetArgumentLimit(compiledPattern))
            {
                throw new ArgumentException(SR.Argument_TooFewValues);
            }
            Format(compiledPattern, values, ref appendTo, resultCopy: default, forbidResultAsValue: true, offsets);
        }

        #endregion FormatAndAppend

        #region FormatAndReplace

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// If there is no {i} in the pattern, then offsets[i] is set to -1.
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// </param>
        /// <returns><paramref name="result"/></returns>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static StringBuilder FormatAndReplace(
            scoped ReadOnlySpan<char> compiledPattern, StringBuilder result, Span<int> offsets, params string[] values) // ICU4N TODO: API - factor out or change to IAppendable.
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                FormatAndReplace(compiledPattern, ref sb, offsets, values);
                result.Length = 0; // ICU4N: a string cannot be the same instance as StringBuilder in .NET
                return result.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// If there is no {i} in the pattern, then offsets[i] is set to -1.
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndReplace(scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, params string[] values)
        {
            int valuesLength = values != null ? values.Length : 0;
            if (valuesLength < GetArgumentLimit(compiledPattern))
                throw new ArgumentException(SR.Argument_TooFewValues);
            if (compiledPattern.Overlaps(result.AsSpan()))
                throw new ArgumentException(string.Format(SR.Argument_SameMemoryLocation, nameof(compiledPattern), nameof(result)));

            result.Length = 0;
            Format(compiledPattern, values, ref result, /*resultCopy, forbidResultAsValue: false,*/ offsets);
        }

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndReplace(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, ReadOnlySpan<char> value0)
        {
            FormatAndReplaceInternal(compiledPattern, ref result, offsets, valuesLength: 1, value0, default, default);
        }

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <param name="value1">
        /// The second argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndReplace(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1)
        {
            FormatAndReplaceInternal(compiledPattern, ref result, offsets, valuesLength: 2, value0, value1, default);
        }

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="value0">
        /// The first argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <param name="value1">
        /// The second argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <param name="value2">
        /// The third argument value.
        /// An argument value may be the same memory location as <paramref name="result"/>.
        /// </param>
        /// <remarks>
        /// The number of values passed must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </remarks>
        internal static void FormatAndReplace(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, ReadOnlySpan<char> value2)
        {
            FormatAndReplaceInternal(compiledPattern, ref result, offsets, valuesLength: 3, value0, value1, value2);
        }

        private static void FormatAndReplaceInternal(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, int valuesLength, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, ReadOnlySpan<char> value2)
        {
            if (valuesLength < GetArgumentLimit(compiledPattern))
                throw new ArgumentException(SR.Argument_TooFewValues);
            if (compiledPattern.Overlaps(result.AsSpan()))
                throw new ArgumentException(string.Format(SR.Argument_SameMemoryLocation, nameof(compiledPattern), nameof(result)));

            // If the pattern starts with an argument whose value is the same object
            // as the result, then we keep the result contents and append to it.
            // Otherwise we replace its contents.
            int firstArg = -1;
            // If any non-initial argument value is the same object as the result,
            // then we first copy its contents and use that instead while formatting.
            bool resultCopied = false;
            ValueStringBuilder resultCopy = default;
            try
            {
                if (GetArgumentLimit(compiledPattern) > 0)
                {
                    for (int i = 1; i < compiledPattern.Length;)
                    {
                        int n = compiledPattern[i++];

                        if (n < ARG_NUM_LIMIT)
                        {
                            ReadOnlySpan<char> value = n switch //values[n];
                            {
                                0 => value0,
                                1 => value1,
                                _ => default
                            };

                            if (value.Overlaps(result.AsSpan()))
                            {
                                if (i == 2)
                                {
                                    firstArg = n;
                                }
                                else if (!resultCopied)
                                {
                                    resultCopy = new ValueStringBuilder(result.Length);
                                    resultCopy.Append(result.AsSpan()); // = result.toString();
                                    resultCopied = true;
                                }
                            }
                        }
                        else
                        {
                            i += n - ARG_NUM_LIMIT;
                        }
                    }
                }
                if (firstArg < 0)
                {
                    result.Length = 0;
                }
                Format(compiledPattern, value0, value1, value2, ref result, resultCopy.AsSpan(), forbidResultAsValue: false, offsets);
            }
            finally
            {
                resultCopy.Dispose();
            }
        }


        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// <c>offsets[i]</c> receives the offset of where
        /// <c>value</c><i><b>i</b></i> replaced pattern argument <c>{i}</c>.
        /// Can be <c>null</c>, or can be shorter or longer than values.
        /// If there is no <c>{i}</c> in the pattern, then <c>offsets[i]</c> is set to <c>-1</c>.
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value may be the same memory location as result.
        /// values.Length must be at least <see cref="GetArgumentLimit(ReadOnlySpan{Char})"/>.
        /// </param>
        internal static void FormatAndReplace(
            scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder result, Span<int> offsets, ReadOnlySpanArray<char> values)
        {
            int valuesLength = values.Length;
            if (valuesLength < GetArgumentLimit(compiledPattern))
                throw new ArgumentException(SR.Argument_TooFewValues);
            if (compiledPattern.Overlaps(result.AsSpan()))
                throw new ArgumentException(string.Format(SR.Argument_SameMemoryLocation, nameof(compiledPattern), nameof(result)));

            // If the pattern starts with an argument whose value is the same object
            // as the result, then we keep the result contents and append to it.
            // Otherwise we replace its contents.
            int firstArg = -1;
            // If any non-initial argument value is the same object as the result,
            // then we first copy its contents and use that instead while formatting.
            bool resultCopied = false;
            ValueStringBuilder resultCopy = default;
            try
            {
                if (GetArgumentLimit(compiledPattern) > 0)
                {
                    for (int i = 1; i < compiledPattern.Length;)
                    {
                        int n = compiledPattern[i++];

                        if (n < ARG_NUM_LIMIT)
                        {
                            ReadOnlySpan<char> value = values[n];
                            if (value.Overlaps(result.AsSpan()))
                            {
                                if (i == 2)
                                {
                                    firstArg = n;
                                }
                                else if (!resultCopied)
                                {
                                    resultCopy = new ValueStringBuilder(result.Length);
                                    resultCopy.Append(result.AsSpan()); // = result.toString();
                                    resultCopied = true;
                                }
                            }
                        }
                        else
                        {
                            i += n - ARG_NUM_LIMIT;
                        }
                    }
                }
                if (firstArg < 0)
                {
                    result.Length = 0;
                }
                Format(compiledPattern, values, ref result, resultCopy.AsSpan(), forbidResultAsValue: false, offsets);
            }
            finally
            {
                resultCopy.Dispose();
            }
        }

        #endregion FormatAndReplace

        #region GetTextWithNoArguments/TryGetTextWithNoArguments

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <returns>The pattern text without arguments as if they are empty strings.</returns>
        public static string GetTextWithNoArguments(scoped ReadOnlySpan<char> compiledPattern)
        {
            int capacity = compiledPattern.Length - 1 - GetArgumentLimit(compiledPattern);
            ValueStringBuilder sb = capacity <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[capacity])
                : new ValueStringBuilder(capacity);
            try
            {
                GetTextWithNoArguments(compiledPattern, ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="destination">When this method returns successfully, contains the pattern text without
        /// arguments as if they are empty strings.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public static bool TryGetTextWithNoArguments(scoped ReadOnlySpan<char> compiledPattern, Span<char> destination, out int charsLength)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                GetTextWithNoArguments(compiledPattern, ref sb);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <param name="destination">When this method returns, contains the pattern text without
        /// arguments as if they are empty strings.</param>
        internal static void GetTextWithNoArguments(scoped ReadOnlySpan<char> compiledPattern, ref ValueStringBuilder destination)
        {
            for (int i = 1; i < compiledPattern.Length;)
            {
                int segmentLength = compiledPattern[i++] - ARG_NUM_LIMIT;
                if (segmentLength > 0)
                {
                    int limit = i + segmentLength;
                    destination.Append(compiledPattern.Slice(i, segmentLength)); // ICU4N: Corrected 2nd parameter
                    i = limit;
                }
            }
        }

        #endregion GetTextWithNoArguments/TryGetTextWithNoArguments

        #region Format

        // ICU4N: Removed the resultCopy and forbidResultAsValue parameters from this overload, since
        // they are never relevant to string data type in .NET.
        private static void Format(
            scoped ReadOnlySpan<char> compiledPattern, string[] values,
            ref ValueStringBuilder result,
            Span<int> offsets)
        {
            int offsetsLength = offsets.Length;
            if (offsetsLength > 0)
            {
                for (int i = 0; i < offsetsLength; i++)
                {
                    offsets[i] = -1;
                }
            }
            for (int i = 1; i < compiledPattern.Length;)
            {
                int n = compiledPattern[i++];
                if (n < ARG_NUM_LIMIT)
                {
                    var value = values[n];
                    if (n < offsetsLength)
                    {
                        offsets[n] = result.Length;
                    }
                    result.Append(value);
                }
                else
                {
                    int limit = i + (n - ARG_NUM_LIMIT);
                    result.Append(compiledPattern.Slice(i, limit - i)); // ICU4N: Corrected 3rd parameter logic
                    i = limit;
                }
            }
        }

        private static void Format(
            scoped ReadOnlySpan<char> compiledPattern, ReadOnlySpanArray<char> values,
            ref ValueStringBuilder result, scoped ReadOnlySpan<char> resultCopy, bool forbidResultAsValue,
            Span<int> offsets)
        {
            int offsetsLength = offsets.Length;
            if (offsetsLength > 0)
            {
                for (int i = 0; i < offsetsLength; i++)
                {
                    offsets[i] = -1;
                }
            }
            for (int i = 1; i < compiledPattern.Length;)
            {
                int n = compiledPattern[i++];
                if (n < ARG_NUM_LIMIT)
                {
                    ReadOnlySpan<char> value = values[n];
                    if (value.Overlaps(result.AsSpan()))
                    {
                        if (forbidResultAsValue)
                        {
                            throw new ArgumentException(string.Format(SR.Argument_SameMemoryLocation, $"value{n}", nameof(result)));
                        }
                        if (i == 2)
                        {
                            // We are appending to result which is also the first value object.
                            if (n < offsetsLength)
                            {
                                offsets[n] = 0;
                            }
                        }
                        else
                        {
                            if (n < offsetsLength)
                            {
                                offsets[n] = result.Length;
                            }
                            result.Append(resultCopy);
                        }
                    }
                    else
                    {
                        if (n < offsetsLength)
                        {
                            offsets[n] = result.Length;
                        }
                        result.Append(value);
                    }
                }
                else
                {
                    int limit = i + (n - ARG_NUM_LIMIT);
                    result.Append(compiledPattern.Slice(i, limit - i)); // ICU4N: Corrected 2nd parameter logic
                    i = limit;
                }
            }
        }

        private static void Format(
            scoped ReadOnlySpan<char> compiledPattern, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, ReadOnlySpan<char> value2,
            ref ValueStringBuilder result, scoped ReadOnlySpan<char> resultCopy, bool forbidResultAsValue,
            Span<int> offsets)
        {
            int offsetsLength = offsets.Length;
            if (offsetsLength > 0)
            {
                for (int i = 0; i < offsetsLength; i++)
                {
                    offsets[i] = -1;
                }
            }
            for (int i = 1; i < compiledPattern.Length;)
            {
                int n = compiledPattern[i++];
                if (n < ARG_NUM_LIMIT)
                {
                    ReadOnlySpan<char> value = n switch //values[n];
                    {
                        0 => value0,
                        1 => value1,
                        2 => value2,
                        _ => default
                    };
                    if (value.Overlaps(result.AsSpan()))
                    {
                        if (forbidResultAsValue)
                        {
                            throw new ArgumentException(string.Format(SR.Argument_SameMemoryLocation, $"value{n}", nameof(result)));
                        }
                        if (i == 2)
                        {
                            // We are appending to result which is also the first value object.
                            if (n < offsetsLength)
                            {
                                offsets[n] = 0;
                            }
                        }
                        else
                        {
                            if (n < offsetsLength)
                            {
                                offsets[n] = result.Length;
                            }
                            result.Append(resultCopy);
                        }
                    }
                    else
                    {
                        if (n < offsetsLength)
                        {
                            offsets[n] = result.Length;
                        }
                        result.Append(value);
                    }
                }
                else
                {
                    int limit = i + (n - ARG_NUM_LIMIT);
                    result.Append(compiledPattern.Slice(i, limit - i)); // ICU4N: Corrected 2nd parameter logic
                    i = limit;
                }
            }
        }

        #endregion Format
    }
}
