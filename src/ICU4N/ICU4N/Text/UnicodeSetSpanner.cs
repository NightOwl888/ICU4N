using ICU4N.Support.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A helper class used to count, replace, and trim <see cref="ICharSequence"/>s based on <see cref="Text.UnicodeSet"/> matches.
    /// </summary>
    /// <remarks>
    /// An instance is immutable (and thus thread-safe) iff the source UnicodeSet is frozen.
    /// <para/>
    /// <b>Note:</b> The counting, deletion, and replacement depend on alternating a <see cref="Text.UnicodeSet.SpanCondition"/> with
    /// its inverse. That is, the code spans, then spans for the inverse, then spans, and so on.
    /// For the inverse, the following mapping is used:
    /// <list type="table">
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.SIMPLE"/></term><description><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.CONTAINED"/></term><description><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></term><description><see cref="Text.UnicodeSet.SpanCondition.SIMPLE"/></description></item>
    /// </list>
    /// These are actually not complete inverses. However, the alternating works because there are no gaps.
    /// For example, with [a{ab}{bc}], you get the following behavior when scanning forward:
    /// <list type="table">
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.SIMPLE"/></term><description>xxx[ab]cyyy</description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.CONTAINED"/></term><description>xxx[abc]yyy</description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></term><description>[xxx]ab[cyyy]</description></item>
    /// </list>
    /// <para/>
    /// So here is what happens when you alternate:
    /// <list type="table">
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></term><description>|xxxabcyyy</description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.CONTAINED"/></term><description>xxx|abcyyy</description></item>
    ///     <item><term><see cref="Text.UnicodeSet.SpanCondition.NOT_CONTAINED"/></term><description>xxxabcyyy|</description></item>
    /// </list>
    /// <para/>
    /// The entire string is traversed.
    /// </remarks>
    /// <stable>ICU 54</stable>
    public partial class UnicodeSetSpanner
    {
        private readonly UnicodeSet unicodeSet;

        /// <summary>
        /// Create a spanner from a <see cref="Text.UnicodeSet"/>. For speed and safety, the <see cref="Text.UnicodeSet"/> should be frozen. However, this class
        /// can be used with a non-frozen version to avoid the cost of freezing.
        /// </summary>
        /// <param name="source">The original <see cref="Text.UnicodeSet"/>.</param>
        /// <stable>ICU 54</stable>
        public UnicodeSetSpanner(UnicodeSet source)
        {
            unicodeSet = source;
        }

        /// <summary>
        /// Gets the <see cref="Text.UnicodeSet"/> used for processing. It is frozen iff the original was.
        /// </summary>
        /// <stable>ICU 54</stable>
        public virtual UnicodeSet UnicodeSet
        {
            get { return unicodeSet; }
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        /// <stable>ICU 54</stable>
        public override bool Equals(object other)
        {
            return other is UnicodeSetSpanner && unicodeSet.Equals(((UnicodeSetSpanner)other).unicodeSet);
        }

        /// <summary>
        /// Gets a hash code that represents the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <stable>ICU 54</stable>
        public override int GetHashCode()
        {
            return unicodeSet.GetHashCode();
        }

        /// <summary>
        /// Options for <see cref="UnicodeSetSpanner.ReplaceFrom(ICharSequence, ICharSequence, CountMethod)"/> 
        /// and <see cref="UnicodeSetSpanner.CountIn(ICharSequence, CountMethod)"/> to control how to treat each matched span. 
        /// It is similar to whether one is replacing [abc] by x, or [abc]* by x.
        /// </summary>
        /// <stable>ICU 54</stable>
        public enum CountMethod // ICU4N TODO: API De-nest
        {
            /// <summary>
            /// Collapse spans. That is, modify/count the entire matching span as a single item, instead of separate
            /// set elements.
            /// </summary>
            /// <stable>ICU 54</stable>
            WHOLE_SPAN,

            /// <summary>
            /// Use the smallest number of elements in the spanned range for counting and modification,
            /// based on the <see cref="UnicodeSet.SpanCondition"/>.
            /// If the set has no strings, this will be the same as the number of spanned code points.
            /// <para/>
            /// For example, in the string "abab" with <see cref="UnicodeSet.SpanCondition.SIMPLE"/>:
            /// <list type="bullet">
            ///     <item><description>spanning with [ab] will count four <see cref="MIN_ELEMENTS"/>.</description></item>
            ///     <item><description>spanning with [{ab}] will count two <see cref="MIN_ELEMENTS"/>.</description></item>
            ///     <item><description>spanning with [ab{ab}] will also count two <see cref="MIN_ELEMENTS"/>.</description></item>
            /// </list>
            /// </summary>
            /// <stable>ICU 54</stable>
            MIN_ELEMENTS,
            // Note: could in the future have an additional option MAX_ELEMENTS
        }

        // ICU4N specific - moved all methods to UnicodeSetSpannerExtension.tt
        // so overloads for each of the charcter sequence types can be automatically
        // generated.

        /// <summary>
        /// Options for the <see cref="UnicodeSetSpanner.Trim(ICharSequence, TrimOption, UnicodeSet.SpanCondition)"/> method.
        /// </summary>
        /// <stable>ICU 54</stable>
        public enum TrimOption // ICU4N TODO: API De-nest
        {
            /// <summary>
            /// Trim leading spans.
            /// </summary>
            /// <stable>ICU 54</stable>
            LEADING,
            /// <summary>
            /// Trim leading and trailing spans.
            /// </summary>
            /// <stable>ICU 54</stable>
            BOTH,
            /// <summary>
            /// Trim trailing spans.
            /// </summary>
            /// <stable>ICU 54</stable>
            TRAILING
        }
    }
}
