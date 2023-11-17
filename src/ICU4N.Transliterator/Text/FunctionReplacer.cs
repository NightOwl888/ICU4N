using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A replacer that calls a transliterator to generate its output text.
    /// The input text to the transliterator is the output of another
    /// <see cref="IUnicodeReplacer"/> object.  That is, this replacer wraps another
    /// replacer with a transliterator.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class FunctionReplacer : IUnicodeReplacer
    {
        /// <summary>
        /// The transliterator.  Must not be null.
        /// </summary>
        private Transliterator translit;

        /// <summary>
        /// The replacer object.  This generates text that is then
        /// processed by 'translit'.  Must not be null.
        /// </summary>
        private IUnicodeReplacer replacer;

        /// <summary>
        /// Construct a replacer that takes the output of the given
        /// <paramref name="replacer"/>, passes it through the given <paramref name="transliterator"/>, and emits
        /// the result as output.
        /// </summary>
        public FunctionReplacer(Transliterator transliterator,
                                IUnicodeReplacer replacer)
        {
            translit = transliterator;
            this.replacer = replacer;
        }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual int Replace(IReplaceable text,
                           int start,
                           int limit,
                           out int cursor) // ICU4N: Changed cursor parameter from int[] to out int
        {

            // First delegate to subordinate replacer
            int len = replacer.Replace(text, start, limit, out cursor);
            limit = start + len;

            // Now transliterate
            limit = translit.Transliterate(text, start, limit);

            return limit - start;
        }

        /// <summary>
        /// <see cref="IUnicodeReplacer"/> API
        /// </summary>
        public virtual string ToReplacerPattern(bool escapeUnprintable)
        {
            StringBuilder rule = new StringBuilder("&");
            rule.Append(translit.ID);
            rule.Append("( ");
            rule.Append(replacer.ToReplacerPattern(escapeUnprintable));
            rule.Append(" )");
            return rule.ToString();
        }

        /// <summary>
        /// Union the set of all characters that may output by this object
        /// into the given set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the output characters.</param>
        public virtual void AddReplacementSetTo(UnicodeSet toUnionTo)
        {
            toUnionTo.AddAll(translit.GetTargetSet());
        }
    }
}
