using ICU4N.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class contains utility functions so testing not needed
    /// </summary>
    public class UtilityExtensions
    {
        /// <summary>
        /// Append the given string to the rule.  Calls the single-character
        /// version of appendToRule for each character.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="text"></param>
        /// <param name="isLiteral"></param>
        /// <param name="escapeUnprintable"></param>
        /// <param name="quoteBuf"></param>
        public static void AppendToRule(StringBuffer rule,
                                        string text,
                                        bool isLiteral,
                                        bool escapeUnprintable,
                                        StringBuffer quoteBuf)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                // Okay to process in 16-bit code units here
                Utility.AppendToRule(rule, text[i], isLiteral, escapeUnprintable, quoteBuf);
            }
        }

        /// <summary>
        /// Given a <paramref name="matcher"/> reference, which may be null, append its
        /// pattern as a literal to the given <paramref name="rule"/>.
        /// </summary>
        public static void AppendToRule(StringBuffer rule,
                                        IUnicodeMatcher matcher,
                                        bool escapeUnprintable,
                                        StringBuffer quoteBuf)
        {
            if (matcher != null)
            {
                AppendToRule(rule, matcher.ToPattern(escapeUnprintable),
                             true, escapeUnprintable, quoteBuf);
            }
        }

        /// <summary>
        /// For debugging purposes; format the given text in the form
        /// aaa{bbb|ccc|ddd}eee, where the {} indicate the context start
        /// and limit, and the || indicate the start and limit.
        /// </summary>
        public static string FormatInput(ReplaceableString input,
                                         TransliterationPosition pos)
        {
            StringBuffer appendTo = new StringBuffer();
            FormatInput(appendTo, input, pos);
            return Utility.Escape(appendTo.ToString());
        }

        /// <summary>
        /// For debugging purposes; format the given text in the form
        /// aaa{bbb|ccc|ddd}eee, where the {} indicate the context start
        /// and limit, and the || indicate the start and limit.
        /// </summary>
        /// <param name="appendTo"></param>
        /// <param name="input"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static StringBuffer FormatInput(StringBuffer appendTo,
                                               ReplaceableString input,
                                               TransliterationPosition pos)
        {
            if (0 <= pos.ContextStart &&
                pos.ContextStart <= pos.Start &&
                pos.Start <= pos.Limit &&
                pos.Limit <= pos.ContextLimit &&
                pos.ContextLimit <= input.Length)
            {

                string b, c, d;
                //a = input.substring(0, pos.contextStart);
                b = input.Substring(pos.ContextStart, pos.Start - pos.ContextStart); // ICU4N: Corrected 2nd parameter
                c = input.Substring(pos.Start, pos.Limit - pos.Start); // ICU4N: Corrected 2nd parameter
                d = input.Substring(pos.Limit, pos.ContextLimit - pos.Limit); // ICU4N: Corrected 2nd parameter
                //e = input.substring(pos.contextLimit, input.length());
                appendTo.//Append(a).
                    Append('{').Append(b).
                    Append('|').Append(c).Append('|').Append(d).
                    Append('}')
                    //.Append(e)
                    ;
            }
            else
            {
                appendTo.Append("INVALID Position {cs=" +
                                pos.ContextStart + ", s=" + pos.Start + ", l=" +
                                pos.Limit + ", cl=" + pos.ContextLimit + "} on " +
                                input);
            }
            return appendTo;
        }

        /// <summary>
        /// Convenience method.
        /// </summary>
        public static string FormatInput(IReplaceable input,
                                         TransliterationPosition pos)
        {
            return FormatInput((ReplaceableString)input, pos);
        }

        /// <summary>
        /// Convenience method.
        /// </summary>
        public static StringBuffer FormatInput(StringBuffer appendTo,
                                               IReplaceable input,
                                               TransliterationPosition pos)
        {
            return FormatInput(appendTo, (ReplaceableString)input, pos);
        }
    }
}
