using ICU4N.Impl;
using System;
using System.Text;

namespace ICU4N.Text
{
    // ICU4N TODO: API - Add overloads for object[]? This is the way that string.Concat() works, but need to consider the culture
    // aware implications of converting dates and numbers.

    /// <summary>
    /// Formats simple patterns like "{1} was born in {0}".
    /// </summary>
    /// <remarks>
    /// Minimal subset of <see cref="MessageFormat"/>; fast, simple, minimal dependencies.
    /// Supports only numbered arguments with no type nor style parameters,
    /// and formats only string values.
    /// Quoting via ASCII apostrophe compatible with ICU <see cref="MessageFormat"/> default behavior.
    /// <para/>
    /// Factory methods throw exceptions for syntax errors
    /// and for too few or too many arguments/placeholders.
    /// <para/>
    /// <see cref="SimpleFormatter"/> objects are immutable and can be safely cached like strings.
    /// <para/>
    /// Example:
    /// <code>
    /// SimpleFormatter fmt = SimpleFormatter.Compile("{1} '{born}' in {0}");
    /// 
    /// // Output: "paul {born} in england"
    /// Console.WriteLine(fmt.Format("england", "paul"));
    /// </code>
    /// </remarks>
    /// <seealso cref="MessageFormat"/>
    /// <seealso cref="ApostropheMode"/>
    /// <stable>ICU 57</stable>
    public sealed partial class SimpleFormatter
    {
        // For internal use in Java, use SimpleFormatterImpl directly instead:
        // It is most efficient to compile patterns to compiled-pattern strings
        // and use them with static methods.
        // (Avoids allocating SimpleFormatter wrapper objects.)

        /// <summary>
        /// Binary representation of the compiled pattern.
        /// Index 0: One more than the highest argument number.
        /// Followed by zero or more arguments or literal-text segments.
        /// <para/>
        /// An argument is stored as its number, less than ARG_NUM_LIMIT.
        /// A literal-text segment is stored as its length (at least 1) offset by ARG_NUM_LIMIT,
        /// followed by that many chars.
        /// </summary>
        /// <seealso cref="SimpleFormatterImpl"/>
        private readonly string compiledPattern;

        private int? argumentLimit;

        private SimpleFormatter(string compiledPattern)
        {
            this.compiledPattern = compiledPattern;
        }

        /// <summary>
        /// Creates a formatter from the pattern string.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <returns>The new <see cref="SimpleFormatter"/> object.</returns>
        /// <exception cref="ArgumentException">For bad argument syntax.</exception>
        /// <stable>ICU 57</stable>
        public static SimpleFormatter Compile(string pattern)
        {
            return CompileMinMaxArguments(pattern, 0, int.MaxValue);
        }

        /// <summary>
        /// Creates a formatter from the pattern string.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <returns>The new <see cref="SimpleFormatter"/> object.</returns>
        /// <exception cref="ArgumentException">For bad argument syntax.</exception>
        /// <stable>ICU 57</stable>
        public static SimpleFormatter Compile(ReadOnlySpan<char> pattern)
        {
            return CompileMinMaxArguments(pattern, 0, int.MaxValue);
        }

        /// <summary>
        /// Creates a formatter from the pattern string.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <returns>The new <see cref="SimpleFormatter"/> object.</returns>
        /// <exception cref="ArgumentException">For bad argument syntax and too few or too many arguments.</exception>
        /// <stable>ICU 57</stable>
        public static SimpleFormatter CompileMinMaxArguments(string pattern, int min, int max)
        {
            if (pattern is null)
                throw new ArgumentNullException(nameof(pattern));

            return CompileMinMaxArguments(pattern.AsSpan(), min, max);
        }

        /// <summary>
        /// Creates a formatter from the pattern string.
        /// The number of arguments checked against the given limits is the
        /// highest argument number plus one, not the number of occurrences of arguments.
        /// </summary>
        /// <param name="pattern">The pattern string.</param>
        /// <param name="min">The pattern must have at least this many arguments.</param>
        /// <param name="max">The pattern must have at most this many arguments.</param>
        /// <returns>The new <see cref="SimpleFormatter"/> object.</returns>
        /// <exception cref="ArgumentException">For bad argument syntax and too few or too many arguments.</exception>
        /// <stable>ICU 57</stable>
        public static SimpleFormatter CompileMinMaxArguments(ReadOnlySpan<char> pattern, int min, int max)
        {
            string compiledPattern = SimpleFormatterImpl.CompileToStringMinMaxArguments(pattern, min, max);
            return new SimpleFormatter(compiledPattern);
        }

        /// <summary>
        /// The max argument number + 1.
        /// </summary>
        /// <stable>ICU 57</stable>
        public int ArgumentLimit => argumentLimit.HasValue ? argumentLimit.Value : (argumentLimit = SimpleFormatterImpl.GetArgumentLimit(compiledPattern.AsSpan())).Value;

        /// <summary>
        /// Formats the given values.
        /// </summary>
        /// <param name="values">The argument values.</param>
        /// <stable>ICU 57</stable>
        public string Format(params string[] values)
        {
            return SimpleFormatterImpl.FormatCompiledPattern(compiledPattern.AsSpan(), values);
        }

        /// <summary>
        /// Formats the given values.
        /// </summary>
        /// <param name="destination">When this method returns successfully, contains the formatted text.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <param name="values">The argument values.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        public bool TryFormat(Span<char> destination, out int charsLength, params string[] values)
        {
            return SimpleFormatterImpl.TryFormatCompiledPattern(compiledPattern.AsSpan(), destination, out charsLength, values);
        }

        /// <summary>
        /// Formats the given values, appending to the <paramref name="appendTo"/> builder.
        /// </summary>
        /// <param name="appendTo">Gets the formatted pattern and values appended.</param>
        /// <param name="offsets">
        /// offsets[i] receives the offset of where
        /// values[i] replaced pattern argument {i}.
        /// Can be null, or can be shorter or longer than values.
        /// If there is no {i} in the pattern, then offsets[i] is set to -1.
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value must not be the same object as appendTo.
        /// values.Length must be at least <see cref="ArgumentLimit"/>.
        /// Can be null if <see cref="ArgumentLimit"/>==0.
        /// </param>
        /// <returns><paramref name="appendTo"/></returns>
        /// <stable>ICU 57</stable>
        internal StringBuilder FormatAndAppend(
            StringBuilder appendTo, Span<int> offsets, params string[] values) // ICU4N TODO: API - If we do this, we should probably make this IAppendable instead of StringBuilder
        {
            return SimpleFormatterImpl.FormatAndAppend(compiledPattern.AsSpan(), appendTo, offsets, values);
        }

        /// <summary>
        /// Formats the given values, replacing the contents of the result builder.
        /// May optimize by actually appending to the result if it is the same object
        /// as the value corresponding to the initial argument in the pattern.
        /// </summary>
        /// <param name="result">Gets its contents replaced by the formatted pattern and values.</param>
        /// <param name="offsets">
        /// offsets[i] receives the offset of where
        /// values[i] replaced pattern argument {i}.
        /// Can be null, or can be shorter or longer than values.
        /// If there is no {i} in the pattern, then offsets[i] is set to -1.
        /// </param>
        /// <param name="values">
        /// The argument values.
        /// An argument value may be the same object as result.
        /// values.Length must be at least <see cref="ArgumentLimit"/>.
        /// </param>
        /// <returns><paramref name="result"/></returns>
        /// <stable>ICU 57</stable>
        internal StringBuilder FormatAndReplace(
            StringBuilder result, Span<int> offsets, params string[] values) // ICU4N TODO: API - If we do this, we should probably make this IAppendable instead of StringBuilder
        {
            return SimpleFormatterImpl.FormatAndReplace(compiledPattern.AsSpan(), result, offsets, values);
        }

        /// <summary>
        /// Returns a string similar to the original pattern, only for debugging.
        /// </summary>
        /// <stable>ICU 57</stable>
        public override string ToString()
        {
            string[] values = new string[ArgumentLimit];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = $"{{{i}}}";
            }
            return Format(values);
        }

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <stable>ICU 57</stable>
        public string GetTextWithNoArguments()
        {
            return SimpleFormatterImpl.GetTextWithNoArguments(compiledPattern.AsSpan());
        }

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <param name="destination">When this method returns successfully, contains the pattern text without
        /// arguments as if they are empty strings.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are
        /// usable in <paramref name="destination"/>; otherwise, this is the length of <paramref name="destination"/> 
        /// that will need to be allocated to succeed in another attempt.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        public bool TryGetTextWithNoArguments(Span<char> destination, out int charsLength)
        {
            return SimpleFormatterImpl.TryGetTextWithNoArguments(compiledPattern.AsSpan(), destination, out charsLength);
        }
    }
}
