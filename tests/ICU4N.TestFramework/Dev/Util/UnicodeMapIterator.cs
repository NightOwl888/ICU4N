using ICU4N.Text;
using System;
using System.Collections.Generic;

namespace ICU4N.Dev.Util
{
    public class UnicodeMapIterator<T> where T : class
    {
        /**
         * Value of <tt>codepoint</tt> if the iterator points to a string.
         * If <tt>codepoint == IS_STRING</tt>, then examine
         * <tt>string</tt> for the current iteration result.
         */
        public static int IS_STRING = -1;

        /**
         * Current code point, or the special value <tt>IS_STRING</tt>, if
         * the iterator points to a string.
         */
        public int Codepoint { get; set; }

        /**
         * When iterating over ranges using <tt>nextRange()</tt>,
         * <tt>codepointEnd</tt> contains the inclusive end of the
         * iteration range, if <tt>codepoint != IS_STRING</tt>.  If
         * iterating over code points using <tt>next()</tt>, or if
         * <tt>codepoint == IS_STRING</tt>, then the value of
         * <tt>codepointEnd</tt> is undefined.
         */
        public int CodepointEnd { get; set; }

        /**
         * If <tt>codepoint == IS_STRING</tt>, then <tt>string</tt> points
         * to the current string.  If <tt>codepoint != IS_STRING</tt>, the
         * value of <tt>string</tt> is undefined.
         */
        public string String { get; set; }

        /**
         * The value associated with this element or range.
         */
        public T Value { get; set; }

        /**
         * Create an iterator over the given set.
         * @param set set to iterate over
         */
        public UnicodeMapIterator(UnicodeMap<T> set)
        {
            Reset(set);
        }

        /**
         * Create an iterator over nothing.  <tt>next()</tt> and
         * <tt>nextRange()</tt> return false. This is a convenience
         * constructor allowing the target to be set later.
         */
        public UnicodeMapIterator()
        {
            Reset(new UnicodeMap<T>());
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
         *
         * @return true if there was another element in the set and this
         * object contains the element.
         */
        public bool Next()
        {
            if (nextElement <= endElement)
            {
                Codepoint = CodepointEnd = nextElement++;
                return true;
            }
            while (range < endRange)
            {
                if (LoadRange(++range) == null)
                {
                    continue;
                }
                Codepoint = CodepointEnd = nextElement++;
                return true;
            }

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null) return false;
            Codepoint = IS_STRING; // signal that value is actually a string
            if (stringIterator.MoveNext())
                String = stringIterator.Current;
            else
            {
                stringIterator = null;
                return false;
            }
            //string = (String)stringIterator.next();
            //if (!stringIterator.hasNext()) stringIterator = null;
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
            while (range < endRange)
            {
                if (LoadRange(++range) == null)
                {
                    continue;
                }
                CodepointEnd = endElement;
                Codepoint = nextElement;
                nextElement = endElement + 1;
                return true;
            }

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null) return false;
            Codepoint = IS_STRING; // signal that value is actually a string
            if (stringIterator.MoveNext())
                String = stringIterator.Current;
            else
            {
                stringIterator = null;
                return false;
            }
            //String = (String)stringIterator.next();
            //if (!stringIterator.hasNext()) stringIterator = null;
            return true;
        }

        /**
         * Sets this iterator to visit the elements of the given set and
         * resets it to the start of that set.  The iterator is valid only
         * so long as <tt>set</tt> is valid.
         * @param set the set to iterate over.
         */
        public void Reset(UnicodeMap<T> set)
        {
            this.map = set;
            Reset();
        }

        /**
         * Resets this iterator to the start of the set.
         * @return 
         */
        public UnicodeMapIterator<T> Reset()
        {
            endRange = map.RangeCount - 1;
            // both next*() methods will test: if (nextElement <= endElement)
            // we set them to fail this test, which will cause them to load the first range
            nextElement = 0;
            endElement = -1;
            range = -1;

            stringIterator = null;
            ICollection<String> strings = map.GetNonRangeStrings();
            if (strings != null)
            {
                stringIterator = strings.GetEnumerator();
                //if (!stringIterator.hasNext()) stringIterator = null;
            }
            Value = null;
            return this;
        }

        /**
         * Gets the current string from the iterator. Only use after calling next(), not nextRange().
         */
        public string GetString()
        {
            if (Codepoint != IS_STRING)
            {
                return UTF16.ValueOf(Codepoint);
            }
            return String;
        }

        // ======================= PRIVATES ===========================

        private UnicodeMap<T> map;
        private int endRange = 0;
        private int range = 0;
        private IEnumerator<string> stringIterator = null;
        protected int endElement;
        protected int nextElement;

        /*
         * Invariant: stringIterator is null when there are no (more) strings remaining
         */

        protected T LoadRange(int range)
        {
            nextElement = map.GetRangeStart(range);
            endElement = map.GetRangeEnd(range);
            Value = map.GetRangeValue(range);
            return Value;
        }
    }
}
