using System;
using System.Collections;
using System.Collections.Generic;

namespace ICU4N.Text
{
    /// <summary>
    /// Iteration modes that can be used with <see cref="UnicodeSetEnumerator"/>.
    /// </summary>
    public enum UnicodeSetEnumerationMode
    {
        /// <summary>
        /// Iterate over code points and multicharacter strings.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Iterate over code point ranges.
        /// </summary>
        Range = 1
    }

    /// <summary>
    /// <see cref="UnicodeSetEnumerator"/> iterates over the contents of a <see cref="UnicodeSet"/>. It
    /// iterates over either code points or code point ranges. After all
    /// code points or ranges have been returned, it returns the
    /// multicharacter strings of the <see cref="UnicodeSet"/>, if any.
    /// </summary>
    /// <remarks>
    /// To iterate over code points and multicharacter strings,
    /// use a loop like this:
    /// <code>
    /// for (UnicodeSetEnumerator enumerator = new UnicodeSetEnumerator(set, UnicodeSetEnumeratorMode.Default); enumerator.MoveNext();)
    /// {
    ///     ProcessString(enumerator.Current);
    /// }
    /// </code>
    /// <para/>
    /// To iterate over code point ranges, use a loop like this:
    /// <code>
    /// for (UnicodeSetEnumerator enumerator = new UnicodeSetEnumerator(set, UnicodeSetEnumeratorMode.Range); enumerator.MoveNext();)
    /// {
    ///     if (!enumerator.IsString)
    ///     {
    ///         ProcessCodePointRange(enumerator.CodePoint, enumerator.CodePointEnd);
    ///     }
    ///     else
    ///     {
    ///         ProcessString(enumerator.Current);
    ///     }
    /// }
    /// </code>
    /// <para/>
    /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification. 
    /// Do not alter the <see cref="UnicodeSet"/> while iterating.
    /// </remarks>
    /// <author>M. Davis</author>
    /// <stable>ICU 2.0</stable>
    public class UnicodeSetEnumerator : IEnumerator<string>
    {
        private int endRange = 0;
        private int range = 0;
        internal int endElement; // internal for testing
        internal int nextElement; // internal for testing
        /// <summary>
        /// Invariant: stringEnumerator is null when there are no (more) strings remaining
        /// </summary>
        private IEnumerator<string> stringEnumerator = null;
        private string str;

        /// <summary>
        /// Create an enumerator over nothing. <see cref="MoveNext()"/> returns <c>false</c>.
        /// This is a convenience constructor allowing the target to be set later in the
        /// <see cref="Reset(UnicodeSet, UnicodeSetEnumerationMode)"/> method.
        /// <para/>
        /// The mode is set to <see cref="UnicodeSetEnumerationMode.Default"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeSetEnumerator()
            : this(new UnicodeSet(), UnicodeSetEnumerationMode.Default) { }

        /// <summary>
        /// Create an enumerator over the given <paramref name="set"/> using
        /// <see cref="UnicodeSetEnumerationMode.Default"/> (iterating code points and multicharacter strings).
        /// </summary>
        /// <param name="set">Set to iterate over.</param>
        /// <stable>ICU 2.0</stable>
        public UnicodeSetEnumerator(UnicodeSet set) : this(set, UnicodeSetEnumerationMode.Default) { }

        /// <summary>
        /// Create an enumerator over the given <paramref name="set"/> using
        /// the specified <paramref name="mode"/>.
        /// </summary>
        /// <param name="set">Set to iterate over.</param>
        /// <param name="mode">The mode to use when iterating.</param>
        /// <stable>ICU 2.0</stable>
        public UnicodeSetEnumerator(UnicodeSet set, UnicodeSetEnumerationMode mode)
        {
            Reset(set, mode);
        }

        internal UnicodeSet Set { get; private set; } // ICU4N specific - marked internal instead of public, since the functionality is obsolete

        /// <summary>
        /// Gets the current iteration mode.
        /// <para/>
        /// This value can be changed by calling <see cref="Reset(UnicodeSet, UnicodeSetEnumerationMode)"/>,
        /// which also restarts the enumeration.
        /// </summary>
        public UnicodeSetEnumerationMode Mode { get; private set; }

        /// <summary>
        /// Gets whether the current enumerator points to a <see cref="string"/>.
        /// If <c>true</c>, then examine <see cref="Current"/> for the current iteration result.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public bool IsString { get; protected set; }

        /// <summary>
        /// Current code point, if <c><see cref="IsString"/> == false</c>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int CodePoint { get; protected set; }

        /// <summary>
        /// When iterating over ranges using <see cref="UnicodeSetEnumerationMode.Range"/>,
        /// <see cref="CodePointEnd"/> contains the inclusive end of the
        /// iteration range, if <c><see cref="IsString"/> == false</c>. If
        /// iterating over code points using <see cref="UnicodeSetEnumerationMode.Default"/>, or if
        /// <c><see cref="IsString"/> == true</c>, then the value of
        /// <see cref="CodePointEnd"/> is undefined.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int CodePointEnd { get; protected set; }

        /// <summary>
        /// Gets the current string, if <see cref="IsString"/> returned <c>true</c>.
        /// <para/>
        /// If the current iteration item is a code point, a <see cref="string"/> with that single code point is returned.
        /// <para/>
        /// Ownership of the returned string remains with the enumerator. The string is guaranteed to remain valid only
        /// until the enumerator is advanced to the next item, or until the enumerator is destroyed.
        /// </summary>
        /// <stable>ICU 4.0</stable>
        public virtual string Current
        {
            get
            {
                if (!IsString)
                {
                    return str ?? (str = UTF16.ValueOf(CodePoint));
                }
                return str;
            }
        }

        object IEnumerator.Current => Current;

        /// <summary>
        /// Disposes all resources associated with <see cref="UnicodeSetEnumerator"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// May be overridden to dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> indicates to dispose managed resources;
        /// <c>false</c> indicates to dispose unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                str = null;
                stringEnumerator?.Dispose();
                stringEnumerator = null;
            }
        }

        /// <summary>
        /// Returns the next element in the set, either a single code point
        /// or a string. If there are no more elements in the set, returns
        /// <c>false</c>. If <c><see cref="IsString"/> == true</c>, the value is a
        /// string in the <see cref="Current"/> property. Otherwise the value is a
        /// single code point in the <see cref="CodePoint"/> property.
        /// </summary>
        /// <remarks>
        /// The order of iteration is all code points in sorted order,
        /// followed by all strings sorted order. When using <see cref="UnicodeSetEnumerationMode.Range"/>, <see cref="Current"/> is
        /// undefined unless <c><see cref="IsString"/> == true</c>.
        /// <para/>
        /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification.
        /// Do not alter the <see cref="UnicodeSet"/> while iterating.
        /// </remarks>
        /// <returns><c>true</c> if there was another element in the set and this
        /// object contains the element.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool MoveNext()
        {
            if (nextElement <= endElement)
            {
                if (Mode == UnicodeSetEnumerationMode.Default)
                {
                    CodePoint = CodePointEnd = nextElement++;
                }
                else
                {
                    CodePointEnd = endElement;
                    CodePoint = nextElement;
                    nextElement = endElement + 1;
                }
                str = null;
                return true;
            }
            if (range < endRange)
            {
                LoadRange(++range);
                if (Mode == UnicodeSetEnumerationMode.Default)
                {
                    CodePoint = CodePointEnd = nextElement++;
                }
                else
                {
                    CodePointEnd = endElement;
                    CodePoint = nextElement;
                    nextElement = endElement + 1;
                }
                str = null;
                return true;
            }

            CodePoint = default;
            CodePointEnd = default;

            // stringEnumerator == null iff there are no string elements remaining
            if (stringEnumerator == null)
            {
                str = null;
                return false;
            }
            IsString = true; // signal that value is actually a string
            if (!stringEnumerator.MoveNext())
            {
                stringEnumerator = null;
                str = null;
                return false;
            }
            str = stringEnumerator.Current;
            return true;
        }

        /// <summary>
        /// Sets this enumerator to visit the elements of the given set and
        /// resets it to the start of that set. The enumerator is valid only
        /// so long as <paramref name="unicodeSet"/> is valid.
        /// </summary>
        /// <param name="unicodeSet">The set to iterate over.</param>
        /// <param name="mode">The mode to use when iterating.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void Reset(UnicodeSet unicodeSet, UnicodeSetEnumerationMode mode)
        {
            Set = unicodeSet ?? throw new ArgumentNullException(nameof(unicodeSet));
            this.Mode = mode;
            Reset();
        }

        /// <summary>
        /// Sets this enumerator to visit the elements of the given set and
        /// resets it to the start of that set. The enumerator is valid only
        /// so long as <paramref name="unicodeSet"/> is valid.
        /// <para/>
        /// The enumerator continues to use the same mode that was passed into
        /// the constructor on the specified <paramref name="unicodeSet"/>.
        /// </summary>
        /// <param name="unicodeSet">The set to iterate over.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void Reset(UnicodeSet unicodeSet)
        {
            Reset(unicodeSet, Mode);
        }

        /// <summary>
        /// Resets this enumerator to the start of the set.
        /// </summary>
        /// <stable>ICU 2.0</stable>

        public virtual void Reset()
        {
            endRange = Set.RangeCount - 1;
            range = 0;
#pragma warning disable 612, 618
            endElement = -1;
            nextElement = 0;
            if (endRange >= 0)
            {
                LoadRange(range);
            }
#pragma warning restore 612, 618
            stringEnumerator = null;
            if (Set.Strings != null && Set.Strings.Count > 0)
            {
                stringEnumerator = Set.Strings.GetEnumerator();
            }
        }

        internal virtual void LoadRange(int aRange) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            nextElement = Set.GetRangeStart(aRange);
            endElement = Set.GetRangeEnd(aRange);
        }
    }
}
