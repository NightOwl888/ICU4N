using ICU4N.Support.Text;
using ICU4N.Util;
using System.Text;
using static ICU4N.Text.UnicodeSet;

namespace ICU4N.Text
{
    public partial class UnicodeSetSpanner
    {
        private readonly UnicodeSet unicodeSet;

        /**
         * Create a spanner from a UnicodeSet. For speed and safety, the UnicodeSet should be frozen. However, this class
         * can be used with a non-frozen version to avoid the cost of freezing.
         * 
         * @param source
         *            the original UnicodeSet
         *
         * @stable ICU 54
         */
        public UnicodeSetSpanner(UnicodeSet source)
        {
            unicodeSet = source;
        }

        /**
         * Returns the UnicodeSet used for processing. It is frozen iff the original was.
         * 
         * @return the construction set.
         *
         * @stable ICU 54
         */
        public virtual UnicodeSet UnicodeSet
        {
            get { return unicodeSet; }
        }


        /**
         * {@inheritDoc}
         * 
         * @stable ICU 54
         */
        public override bool Equals(object other)
        {
            return other is UnicodeSetSpanner && unicodeSet.Equals(((UnicodeSetSpanner)other).unicodeSet);
        }

        /**
         * {@inheritDoc}
         * 
         * @stable ICU 54
         */
        public override int GetHashCode()
        {
            return unicodeSet.GetHashCode();
        }

        /**
         * Options for replaceFrom and countIn to control how to treat each matched span. 
         * It is similar to whether one is replacing [abc] by x, or [abc]* by x.
         * 
         * @stable ICU 54
         */
        public enum CountMethod 
        {
            /**
             * Collapse spans. That is, modify/count the entire matching span as a single item, instead of separate
             * set elements.
             *
             * @stable ICU 54
             */
            WHOLE_SPAN,
            /**
             * Use the smallest number of elements in the spanned range for counting and modification,
             * based on the {@link UnicodeSet.SpanCondition}.
             * If the set has no strings, this will be the same as the number of spanned code points.
             * <p>For example, in the string "abab" with SpanCondition.SIMPLE:
             * <ul>
             * <li>spanning with [ab] will count four MIN_ELEMENTS.</li>
             * <li>spanning with [{ab}] will count two MIN_ELEMENTS.</li>
             * <li>spanning with [ab{ab}] will also count two MIN_ELEMENTS.</li>
             * </ul>
             *
             * @stable ICU 54
             */
            MIN_ELEMENTS,
            // Note: could in the future have an additional option MAX_ELEMENTS
        }

        // ICU4N specific - moved all methods to UnicodeSetSpannerExtension.tt
        // so overloads for each of the charcter sequence types can be automatically
        // generated.

        /// <summary>
        /// Options for the <see cref="Trim(ICharSequence, TrimOption, SpanCondition)"/> method.
        /// </summary>
        /// <stable>ICU 54</stable>
        public enum TrimOption
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
