using ICU4N.Impl;
using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Text
{
    // ICU4N TODO: Either phase this out or replace the values params with object (similar to string.Format() in .NET).
    // This basically does the same thing a string.Format(), but with different escape characters, so we can probably do without this.

    // ICU4N TODO: If we do keep this, this and all other formatters should implement IFormatProvider so they can be used directly
    // with string.Format()

    // ICU4N TODO: API - The format methods should replace ICharSequence with object so we can mix different types
    // in the same array. Internally, we can just call ToString() on each of the objects (including ICharSequence) to get an array of strings.
    // Without doing this, we have no way of passing string and StringBuilder in the same array as was done in Java.

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
    internal sealed partial class SimpleFormatter // ICU4N: Removed from public API until this can be refactored. Ideally, we would format using static methods and pass in a state object that is cloned to the current thread, and this sort of API would call the static API.
    {
        // For internal use in Java, use SimpleFormatterImpl directly instead:
        // It is most efficient to compile patterns to compiled-pattern strings
        // and use them with static methods.
        // (Avoids allocating SimpleFormatter wrapper objects.)

        /// <summary>
        /// Binary representation of the compiled pattern.
        /// </summary>
        /// <seealso cref="SimpleFormatterImpl"/>
        private readonly string compiledPattern;

        private SimpleFormatter(string compiledPattern)
        {
            this.compiledPattern = compiledPattern;
        }

        // ICU4N specific - Compile(ICharSequence pattern) moved to SimpleFormatter.generated.tt

        // ICU4N specific - CompileMinMaxArguments(ICharSequence pattern, int min, int max) moved to SimpleFormatter.generated.tt

        /// <summary>
        /// The max argument number + 1.
        /// </summary>
        /// <stable>ICU 57</stable>
        public int ArgumentLimit => SimpleFormatterImpl.GetArgumentLimit(compiledPattern);

        // ICU4N specific - Format(params ICharSequence[] values) moved to SimpleFormatter.generated.tt

        // ICU4N specific - FormatAndAppend(
        //    StringBuilder appendTo, int[] offsets, params ICharSequence[] values) moved to SimpleFormatter.generated.tt

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
        /// <returns><paramref name="appendTo"/></returns>
        /// <stable>ICU4N 60.1</stable>
        public StringBuilder FormatAndAppend(
            StringBuilder appendTo, int[] offsets) // ICU4N specific overload to avoid ambiguity when no params are passed
        {
            return SimpleFormatterImpl.FormatAndAppend(compiledPattern, appendTo, offsets, new ICharSequence[0]);
        }

        // ICU4N specific - FormatAndReplace(
        //    StringBuilder result, int[] offsets, params ICharSequence[] values) moved to SimpleFormatter.generated.tt

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
        /// <returns><paramref name="result"/></returns>
        /// <stable>ICU4N 60.1</stable>
        public StringBuilder FormatAndReplace(
            StringBuilder result, int[] offsets) // ICU4N specific overload to avoid ambiguity when no params are passed
        {
            return SimpleFormatterImpl.FormatAndReplace(compiledPattern, result, offsets, new ICharSequence[0]);
        }

        /// <summary>
        /// Returns a string similar to the original pattern, only for debugging.
        /// </summary>
        /// <stable>ICU 57</stable>
        public override string ToString()
        {
            string[] values = new String[ArgumentLimit];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = "{" + i + '}';
            }
            return FormatAndAppend(new StringBuilder(), null, values).ToString();
        }

        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <stable>ICU 57</stable>
        public string GetTextWithNoArguments()
        {
            return SimpleFormatterImpl.GetTextWithNoArguments(compiledPattern);
        }
    }
}
