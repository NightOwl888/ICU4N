using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Formats simple patterns like "{1} was born in {0}".
    /// Internal version of <see cref="Text.SimpleFormatter"/>
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

        // ICU4N specific - CompileToStringMinMaxArguments(
        //    ICharSequence pattern, StringBuilder sb, int min, int max) moved to SimpleFormatterImplExtension.tt


        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        /// <returns>The max argument number + 1.</returns>
        public static int GetArgumentLimit(string compiledPattern)
        {
            return compiledPattern[0];
        }

        // ICU4N specific - FormatCompiledPattern(string compiledPattern, 
        //    params ICharSequence[] values) moved to SimpleFormatterImplExtension.tt

        // ICU4N specific - FormatRawPattern(string pattern, int min, 
        //    int max, params ICharSequence[] values) moved to SimpleFormatterImplExtension.tt

        // ICU4N specific - FormatAndAppend(string compiledPattern, StringBuilder appendTo, 
        //    int[] offsets, params ICharSequence[] values) moved to SimpleFormatterImplExtension.tt

        // ICU4N specific - FormatAndReplace(string compiledPattern, StringBuilder result, 
        //    int[] offsets, params ICharSequence[] values) moved to SimpleFormatterImplExtension.tt


        /// <summary>
        /// Returns the pattern text with none of the arguments.
        /// Like formatting with all-empty string values.
        /// </summary>
        /// <param name="compiledPattern">Compiled form of a pattern string.</param>
        public static string GetTextWithNoArguments(string compiledPattern)
        {
            int capacity = compiledPattern.Length - 1 - GetArgumentLimit(compiledPattern);
            StringBuilder sb = new StringBuilder(capacity);
            for (int i = 1; i < compiledPattern.Length;)
            {
                int segmentLength = compiledPattern[i++] - ARG_NUM_LIMIT;
                if (segmentLength > 0)
                {
                    int limit = i + segmentLength;
                    sb.Append(compiledPattern, i, limit - i); // ICU4N: Corrected 3rd paramter
                    i = limit;
                }
            }
            return sb.ToString();
        }

        // ICU4N specific - Format(
        //    string compiledPattern, ICharSequence[] values,
        //    StringBuilder result, string resultCopy, bool forbidResultAsValue,
        //    int[] offsets) moved to SimpleFormatterImplExtension.tt
    }
}
