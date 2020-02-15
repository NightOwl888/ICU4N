using System;
using System.Collections.Generic;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="UnicodeSetIterator"/> iterates over the contents of a <see cref="UnicodeSet"/>.  It
    /// iterates over either code points or code point ranges.  After all
    /// code points or ranges have been returned, it returns the
    /// multicharacter strings of the <see cref="UnicodeSet"/>, if any.
    /// </summary>
    /// <remarks>
    /// To iterate over code points and multicharacter strings,
    /// use a loop like this:
    /// <code>
    /// for (UnicodeSetIterator it = new UnicodeSetIterator(set); it.Next();)
    /// {
    ///     ProcessString(it.GetString());
    /// }
    /// </code>
    /// <para/>
    /// To iterate over code point ranges, use a loop like this:
    /// <code>
    /// for (UnicodeSetIterator it = new UnicodeSetIterator(set); it.NextRange();)
    /// {
    ///     if (it.Codepoint != UnicodeSetIterator.IS_STRING)
    ///     {
    ///         ProcessCodepointRange(it.Codepoint, it.CodepointEnd);
    ///     }
    ///     else
    ///     {
    ///         ProcessString(it.GetString());
    ///     }
    /// }
    /// </code>
    /// <para/>
    /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification. 
    /// Do not alter the <see cref="UnicodeSet"/> while iterating.
    /// </remarks>
    /// <author>M. Davis</author>
    /// <stable>ICU 2.0</stable>
    public class UnicodeSetIterator // ICU4N TODO: API - refactor into UnicodeSetStringEnumerator and UnicodeSetCodepointEnumerator ?
    {
        /// <summary>
        /// Value of <see cref="Codepoint"/> if the iterator points to a string.
        /// If <c><see cref="Codepoint"/> == <see cref="IsString"/></c>, then examine
        /// <c>string</c> for the current iteration result.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const int IsString = -1;

        /// <summary>
        /// Current code point, or the special value <tt>IS_STRING</tt>, if
        /// the iterator points to a string.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int Codepoint { get; set; }

        /// <summary>
        /// When iterating over ranges using <see cref="NextRange()"/>,
        /// <see cref="CodepointEnd"/> contains the inclusive end of the
        /// iteration range, if <c><see cref="Codepoint"/> != <see cref="IsString"/></c>. If
        /// iterating over code points using <see cref="Next()"/>, or if
        /// <c><see cref="Codepoint"/> == <see cref="IsString"/></c>, then the value of
        /// <see cref="CodepointEnd"/> is undefined.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int CodepointEnd { get; set; }

        /// <summary>
        /// If <c><see cref="Codepoint"/> == <see cref="IsString"/></c>, then <see cref="String"/> points
        /// to the current string. If <c><see cref="Codepoint"/> != <see cref="IsString"/></c>, the
        /// value of <see cref="String"/> is undefined.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public string String { get; set; }

        /// <summary>
        /// Create an iterator over the given set.
        /// </summary>
        /// <param name="set">Set to iterate over.</param>
        /// <stable>ICU 2.0</stable>
        public UnicodeSetIterator(UnicodeSet set)
        {
            Reset(set);
        }

        /// <summary>
        /// Create an iterator over nothing.  <see cref="Next()"/> and
        /// <see cref="NextRange()"/> return false. This is a convenience
        /// constructor allowing the target to be set later.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeSetIterator()
        {
            Reset(new UnicodeSet());
        }

        /// <summary>
        /// Returns the next element in the set, either a single code point
        /// or a string.  If there are no more elements in the set, return
        /// false.  If <c><see cref="Codepoint"/> == <see cref="IsString"/></c>, the value is a
        /// string in the <see cref="String"/> field.  Otherwise the value is a
        /// single code point in the <see cref="Codepoint"/> field.
        /// </summary>
        /// <remarks>
        /// The order of iteration is all code points in sorted order,
        /// followed by all strings sorted order.  <see cref="String"/> is
        /// undefined unless <c><see cref="Codepoint"/> == <see cref="IsString"/></c>.  Do not mix
        /// calls to <see cref="Next()"/> and <see cref="NextRange()"/> without
        /// calling <see cref="Reset()"/> between them.  The results of doing so
        /// are undefined.
        /// <para/>
        /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification. 
        /// Do not alter the <see cref="UnicodeSet"/> while iterating.
        /// </remarks>
        /// <returns>true if there was another element in the set and this
        /// object contains the element.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool Next()
        {
#pragma warning disable 612, 618
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
#pragma warning restore 612, 618

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null)
            {
                return false;
            }
            Codepoint = IsString; // signal that value is actually a string
            if (!stringIterator.MoveNext())
            {
                stringIterator = null;
                return false;
            }
            String = stringIterator.Current;
            return true;
        }

        /// <summary>
        /// Returns the next element in the set, either a code point range
        /// or a string.  If there are no more elements in the set, return
        /// false.  If <c><see cref="Codepoint"/> == <see cref="IsString"/></c>, the value is a
        /// string in the <see cref="String"/> property.  Otherwise the value is a
        /// range of one or more code points from <see cref="Codepoint"/> to
        /// <see cref="CodepointEnd"/> inclusive.
        /// </summary>
        /// <remarks>
        /// The order of iteration is all code points in sorted order,
        /// followed by all strings sorted order.  <see cref="String"/> is
        /// undefined unless <c><see cref="Codepoint"/> == <see cref="IsString"/></c>.  Do not mix
        /// calls to <see cref="Next()"/> and <see cref="NextRange()"/> without
        /// calling <see cref="Reset()"/> between them.  The results of doing so
        /// are undefined.
        /// </remarks>
        /// <returns>true if there was another element in the set and this
        /// object contains the element.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool NextRange()
        {
#pragma warning disable 612, 618
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
#pragma warning restore 612, 618

            // stringIterator == null iff there are no string elements remaining

            if (stringIterator == null)
            {
                return false;
            }
            Codepoint = IsString; // signal that value is actually a string
            if (!stringIterator.MoveNext())
            {
                stringIterator = null;
                return false;
            }
            String = stringIterator.Current;
            return true;
        }

        /// <summary>
        /// Sets this iterator to visit the elements of the given set and
        /// resets it to the start of that set.  The iterator is valid only
        /// so long as <paramref name="uset"/> is valid.
        /// </summary>
        /// <param name="uset">The set to iterate over.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void Reset(UnicodeSet uset)
        {
            set = uset;
            Reset();
        }

        /// <summary>
        /// Resets this iterator to the start of the set.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual void Reset()
        {
            endRange = set.RangeCount - 1;
            range = 0;
#pragma warning disable 612, 618
            endElement = -1;
            nextElement = 0;
            if (endRange >= 0)
            {
                LoadRange(range);
            }
#pragma warning restore 612, 618
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

        /// <summary>
        /// Gets the current string from the iterator. Only use after calling <see cref="Next()"/>,
        /// not <see cref="NextRange()"/>.
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public virtual string GetString() // ICU4N TODO: API String vs GetString() - confusing. This should be made into String property and the current string property made into a private field.
        {
            if (Codepoint != IsString)
            {
                return UTF16.ValueOf(Codepoint);
            }
            return String;
        }

        // ======================= PRIVATES ===========================

        private UnicodeSet set;
        private int endRange = 0;
        private int range = 0;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual UnicodeSet Set // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            get { return set; }
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal int endElement; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal int nextElement; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        /// <summary>
        /// Invariant: stringIterator is null when there are no (more) strings remaining
        /// </summary>
        private IEnumerator<string> stringIterator = null;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal virtual void LoadRange(int aRange) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            nextElement = set.GetRangeStart(aRange);
            endElement = set.GetRangeEnd(aRange);
        }
    }
}
