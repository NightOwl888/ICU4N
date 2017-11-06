using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public class UnicodeSetIterator
    {
        /**
         * Value of <tt>codepoint</tt> if the iterator points to a string.
         * If <tt>codepoint == IS_STRING</tt>, then examine
         * <tt>string</tt> for the current iteration result.
         * @stable ICU 2.0
         */
        public static int IS_STRING = -1;

        /**
         * Current code point, or the special value <tt>IS_STRING</tt>, if
         * the iterator points to a string.
         * @stable ICU 2.0
         */
        public int Codepoint { get; set; }

        /**
         * When iterating over ranges using <tt>nextRange()</tt>,
         * <tt>codepointEnd</tt> contains the inclusive end of the
         * iteration range, if <tt>codepoint != IS_STRING</tt>.  If
         * iterating over code points using <tt>next()</tt>, or if
         * <tt>codepoint == IS_STRING</tt>, then the value of
         * <tt>codepointEnd</tt> is undefined.
         * @stable ICU 2.0
         */
        public int CodepointEnd { get; set; }

        /**
         * If <tt>codepoint == IS_STRING</tt>, then <tt>string</tt> points
         * to the current string.  If <tt>codepoint != IS_STRING</tt>, the
         * value of <tt>string</tt> is undefined.
         * @stable ICU 2.0
         */
        public string String { get; set; }

        /**
         * Create an iterator over the given set.
         * @param set set to iterate over
         * @stable ICU 2.0
         */
        public UnicodeSetIterator(UnicodeSet set)
        {
            Reset(set);
        }

        /**
         * Create an iterator over nothing.  <tt>next()</tt> and
         * <tt>nextRange()</tt> return false. This is a convenience
         * constructor allowing the target to be set later.
         * @stable ICU 2.0
         */
        public UnicodeSetIterator()
        {
            Reset(new UnicodeSet());
        }

        /**
         * Returns the next element in the set, either a single code point
         * or a string.  If there are no more elements in the set, return
         * false.  If <tt>codepoint == IS_STRING</tt>, the value is a
         * string in the <tt>string</tt> field.  Otherwise the value is a
         * single code point in the <tt>codepoint</tt> field.
         * 
         * <p>The order of iteration is all code points in sorted order,
         * followed by all strings sorted order.  <tt>codepointEnd</tt> is
         * undefined after calling this method.  <tt>string</tt> is
         * undefined unless <tt>codepoint == IS_STRING</tt>.  Do not mix
         * calls to <tt>next()</tt> and <tt>nextRange()</tt> without
         * calling <tt>reset()</tt> between them.  The results of doing so
         * are undefined.
         * <p><b>Warning: </b>For speed, UnicodeSet iteration does not check for concurrent modification. 
         * Do not alter the UnicodeSet while iterating.
         * @return true if there was another element in the set and this
         * object contains the element.
         * @stable ICU 2.0
         */
        public bool Next()
        {
            if (nextElement <= endElement)
            {
                Codepoint = CodepointEnd = nextElement++;
                return true;
            }
            if (range < endRange)
            {
                LoadRange(++range);
                Codepoint = CodepointEnd = nextElement++;
                return true;
            }

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null)
            {
                return false;
            }
            Codepoint = IS_STRING; // signal that value is actually a string
            if (!stringIterator.MoveNext())
            {
                stringIterator = null;
                return false;
            }
            String = stringIterator.Current;
            return true;
        }

        /**
         * Returns the next element in the set, either a code point range
         * or a string.  If there are no more elements in the set, return
         * false.  If <tt>codepoint == IS_STRING</tt>, the value is a
         * string in the <tt>string</tt> field.  Otherwise the value is a
         * range of one or more code points from <tt>codepoint</tt> to
         * <tt>codepointeEnd</tt> inclusive.
         * 
         * <p>The order of iteration is all code points ranges in sorted
         * order, followed by all strings sorted order.  Ranges are
         * disjoint and non-contiguous.  <tt>string</tt> is undefined
         * unless <tt>codepoint == IS_STRING</tt>.  Do not mix calls to
         * <tt>next()</tt> and <tt>nextRange()</tt> without calling
         * <tt>reset()</tt> between them.  The results of doing so are
         * undefined.
         *
         * @return true if there was another element in the set and this
         * object contains the element.
         * @stable ICU 2.0
         */
        public bool NextRange()
        {
            if (nextElement <= endElement)
            {
                CodepointEnd = endElement;
                Codepoint = nextElement;
                nextElement = endElement + 1;
                return true;
            }
            if (range < endRange)
            {
                LoadRange(++range);
                CodepointEnd = endElement;
                Codepoint = nextElement;
                nextElement = endElement + 1;
                return true;
            }

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null)
            {
                return false;
            }
            Codepoint = IS_STRING; // signal that value is actually a string
            if (!stringIterator.MoveNext())
            {
                stringIterator = null;
                return false;
            }
            String = stringIterator.Current;
            return true;
        }

        /**
         * Sets this iterator to visit the elements of the given set and
         * resets it to the start of that set.  The iterator is valid only
         * so long as <tt>set</tt> is valid.
         * @param uset the set to iterate over.
         * @stable ICU 2.0
         */
        public void Reset(UnicodeSet uset)
        {
            set = uset;
            Reset();
        }

        /**
         * Resets this iterator to the start of the set.
         * @stable ICU 2.0
         */
        public void Reset()
        {
            endRange = set.GetRangeCount() - 1;
            range = 0;
            endElement = -1;
            nextElement = 0;
            if (endRange >= 0)
            {
                LoadRange(range);
            }
            stringIterator = null;
            if (set.Strings != null)
            {
                stringIterator = set.Strings.GetEnumerator();
                // ICU4N: We can't peek whether there is another element
                // so we can safely skip that step. It is repeated anyway
                // in Next() and NextRange().
                //if (!stringIterator.MoveNext())
                //{
                //    stringIterator = null;
                //}
            }
        }

        /**
         * Gets the current string from the iterator. Only use after calling next(), not nextRange().
         * @stable ICU 4.0
         */
        public string GetString() // ICU4N TODO: String vs GetString() - confusing
        {
            if (Codepoint != IS_STRING)
            {
                return UTF16.ValueOf(Codepoint);
            }
            return String;
        }

        // ======================= PRIVATES ===========================

        private UnicodeSet set;
        private int endRange = 0;
        private int range = 0;

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public UnicodeSet Set
        {
            get { return set; }
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //@Deprecated
        internal int endElement; // ICU4N specific - made internal because of comment
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //@Deprecated
        internal int nextElement; // ICU4N specific - made internal because of comment
        private IEnumerator<string> stringIterator = null;

        /**
         * Invariant: stringIterator is null when there are no (more) strings remaining
         */

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        //@Deprecated
        internal void LoadRange(int aRange) // ICU4N specific - made internal because of comment
        {
            nextElement = set.GetRangeStart(aRange);
            endElement = set.GetRangeEnd(aRange);
        }
    }
}
