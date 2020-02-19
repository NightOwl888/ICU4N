using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections;

namespace ICU4N.Text
{
    /// <summary>
    /// Inserts the specified characters at word breaks. To restrict it to particular characters, use a filter.
    /// TODO: this is an internal class, and only temporary. Remove it once we have \b notation in Transliterator.
    /// </summary>
    internal class BreakTransliterator : Transliterator
    {
        private BreakIterator bi;
        private string insertion;
        private int[] boundaries = new int[50];
        private int boundaryCount = 0;

        public BreakTransliterator(string id, UnicodeFilter filter, BreakIterator bi, string insertion)
            : base(id, filter)
        {
            this.bi = bi;
            this.insertion = insertion;
        }

        public BreakTransliterator(string id, UnicodeFilter filter)
            : this(id, filter, null, " ")
        {
        }

        ////CLOVER:OFF
        // The following method is not called by anything and can't be reached
        public virtual string Insertion
        {
            get { return insertion; }
            set { insertion = value; }
        }
        ////CLOVER:ON

        /////CLOVER:OFF
        //// The following method is not called by anything and can't be reached
        //public void setInsertion(String insertion)
        //{
        //    this.insertion = insertion;
        //}
        /////CLOVER:ON

        public BreakIterator GetBreakIterator()
        {
            // Defer initialization of BreakIterator because it is slow,
            // typically over 2000 ms.
            if (bi == null) bi = BreakIterator.GetWordInstance(new ULocale("th_TH"));
            return bi;
        }

        ////CLOVER:OFF
        // The following method is not called by anything and can't be reached
        public void SetBreakIterator(BreakIterator bi)
        {
            this.bi = bi;
        }
        ////CLOVER:ON

        internal const int LETTER_OR_MARK_MASK =
              (1 << (int)UUnicodeCategory.UppercaseLetter)
            | (1 << (int)UUnicodeCategory.LowercaseLetter)
            | (1 << (int)UUnicodeCategory.TitlecaseLetter)
            | (1 << (int)UUnicodeCategory.ModifierLetter)
            | (1 << (int)UUnicodeCategory.OtherLetter)
            | (1 << (int)UUnicodeCategory.SpacingCombiningMark)
            | (1 << (int)UUnicodeCategory.NonSpacingMark)
            | (1 << (int)UUnicodeCategory.EnclosingMark)
            ;

        protected override void HandleTransliterate(IReplaceable text, TransliterationPosition pos, bool incremental)
        {
            lock (this)
            {
                boundaryCount = 0;
                int boundary = 0;
                GetBreakIterator(); // Lazy-create it if necessary
                bi.SetText(new ReplaceableCharacterEnumerator(text, pos.Start, pos.Limit, pos.Start));
                // TODO: fix clumsy workaround used below.
                /*
                char[] tempBuffer = new char[text.length()];
                text.getChars(0, text.length(), tempBuffer, 0);
                bi.setText(new StringCharacterIterator(new String(tempBuffer), pos.start, pos.limit, pos.start));
                */
                // end debugging

                // To make things much easier, we will stack the boundaries, and then insert at the end.
                // generally, we won't need too many, since we will be filtered.

                for (boundary = bi.First(); boundary != BreakIterator.Done && boundary < pos.Limit; boundary = bi.Next())
                {
                    if (boundary == 0) continue;
                    // HACK: Check to see that preceeding item was a letter

                    int cp = UTF16.CharAt(text, boundary - 1);
                    int type = UChar.GetUnicodeCategory(cp).ToInt32();
                    //System.out.println(Integer.toString(cp,16) + " (before): " + type);
                    if (((1 << type) & LETTER_OR_MARK_MASK) == 0) continue;

                    cp = UTF16.CharAt(text, boundary);
                    type = UChar.GetUnicodeCategory(cp).ToInt32();
                    //System.out.println(Integer.toString(cp,16) + " (after): " + type);
                    if (((1 << type) & LETTER_OR_MARK_MASK) == 0) continue;

                    if (boundaryCount >= boundaries.Length)
                    {       // realloc if necessary
                        int[] temp = new int[boundaries.Length * 2];
                        System.Array.Copy(boundaries, 0, temp, 0, boundaries.Length);
                        boundaries = temp;
                    }

                    boundaries[boundaryCount++] = boundary;
                    //System.out.println(boundary);
                }

                int delta = 0;
                int lastBoundary = 0;

                if (boundaryCount != 0)
                { // if we found something, adjust
                    delta = boundaryCount * insertion.Length;
                    lastBoundary = boundaries[boundaryCount - 1];

                    // we do this from the end backwards, so that we don't have to keep updating.

                    while (boundaryCount > 0)
                    {
                        boundary = boundaries[--boundaryCount];
                        text.Replace(boundary, boundary - boundary, insertion); // ICU4N: Corrected 2nd parameter
                    }
                }

                // Now fix up the return values
                pos.ContextLimit += delta;
                pos.Limit += delta;
                pos.Start = incremental ? lastBoundary + delta : pos.Limit;
            }
        }

        /// <summary>
        /// Registers standard variants with the system.  Called by
        /// Transliterator during initialization.
        /// </summary>
        internal static void Register()
        {
            // false means that it is invisible
            Transliterator trans = new BreakTransliterator("Any-BreakInternal", null);
            Transliterator.RegisterInstance(trans, false);
            /*
            Transliterator.registerFactory("Any-Break", new Transliterator.Factory() {
                public Transliterator getInstance(String ID) {
                    return new BreakTransliterator("Any-Break", null);
                }
            });
            */
        }

        // Hack, just to get a real character enumerator.
        private class ReplaceableCharacterEnumerator : ICharacterEnumerator
        {
            private IReplaceable text;
            private int begin;
            private int end;
            // invariant: begin <= pos <= end
            private int pos;

            ///// <summary>
            ///// Constructs an iterator with an initial index of 0.
            ///// </summary>
            ///// <param name="text">The <see cref="string"/> to be iterated over.</param>
            //public ReplaceableCharacterEnumerator(IReplaceable text)
            //    : this(text, 0)
            //{
            //}

            ///// <summary>
            ///// Constructs an iterator with the specified initial index.
            ///// </summary>
            ///// <param name="text">The <see cref="string"/> to be iterated over.</param>
            ///// <param name="pos">Initial iterator position.</param>
            //public ReplaceableCharacterEnumerator(IReplaceable text, int pos)
            //    : this(text, 0, text.Length, pos)
            //{
            //}

            /// <summary>
            /// Constructs an iterator over the given range of the given string, with the
            /// index set at the specified position.
            /// </summary>
            /// <param name="text">The <see cref="string"/> to be iterated over.</param>
            /// <param name="begin">Index of the first character.</param>
            /// <param name="end">Index of the character following the last character.</param>
            /// <param name="pos">Initial iterator position.</param>
            public ReplaceableCharacterEnumerator(IReplaceable text, int begin, int end, int pos)
            {
                this.text = text ?? throw new ArgumentNullException(nameof(text));

                if (begin < 0 || begin > end || end > text.Length)
                {
                    throw new ArgumentException("Invalid substring range");
                }

                if (pos < begin || pos > end)
                {
                    throw new ArgumentException("Invalid position");
                }

                this.begin = begin;
                this.end = end;
                this.pos = pos;
            }

            /// <summary>
            /// Reset this iterator to point to a new string.  This public
            /// method is used by classes that want to avoid allocating
            /// new <see cref="ReplaceableCharacterEnumerator"/> objects every 
            /// time their <see cref="Reset(IReplaceable)"/> method
            /// is called.
            /// </summary>
            /// <param name="text">The <see cref="string"/> to be iterated over.</param>
            public void Reset(IReplaceable text)
            {
                this.text = text ?? throw new ArgumentNullException(nameof(text));
                this.begin = 0;
                this.end = text.Length;
                this.pos = 0;
            }

            /// <summary>
            /// Implements <see cref="ICharacterEnumerator.MoveFirst()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="ICharacterEnumerator.MoveFirst()"/>
            public bool MoveFirst()
            {
                pos = begin;
                return true;
            }

            /// <summary>
            /// Implements <see cref="ICharacterEnumerator.MoveLast()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="ICharacterEnumerator.MoveLast()"/>
            public bool MoveLast()
            {
                if (end != begin)
                {
                    pos = end - 1;
                }
                else
                {
                    pos = end;
                }
                return true;
            }

            /// <summary>
            /// Implements <see cref="ICharacterEnumerator.Index"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="ICharacterEnumerator.Index"/>
            public int Index
            {
                get => pos;
                set
                {
                    if (value < begin || value > end)
                    {
                        throw new ArgumentException("Invalid index");
                    }
                    pos = value;
                }
            }

            public bool TrySetIndex(int value)
            {
                if (value < StartIndex)
                {
                    pos = StartIndex;
                    return false;
                }
                if (value > EndIndex)
                {
                    pos = end;
                    return false;
                }
                pos = value;
                return true;
            }

            public char Current => pos < text.Length ? text[pos] : unchecked((char)-1);

            object IEnumerator.Current => Current;

            /// <summary>
            /// Implements <see cref="IEnumerator.MoveNext()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="IEnumerator.MoveNext()"/>
            public bool MoveNext()
            {
                if (pos < end /*- 1*/)
                {
                    pos++;
                    return true;
                }
                else
                {
                    pos = end;
                    return false;
                }
            }

            /// <summary>
            /// Implements <see cref="ICharacterEnumerator.MovePrevious()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="ICharacterEnumerator.MovePrevious()"/>
            public bool MovePrevious()
            {
                if (pos > begin)
                {
                    pos--;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int StartIndex => begin;

            public int EndIndex => end /*- (end == begin ? 0 : 1)*/;

            public int Length => end - begin;

            /// <summary>
            /// Compares the equality of two <see cref="ReplaceableCharacterEnumerator"/> objects.
            /// </summary>
            /// <param name="obj">The <see cref="ReplaceableCharacterEnumerator"/> object to be compared with.</param>
            /// <returns><c>true</c> if the given obj is the same as this
            /// <see cref="ReplaceableCharacterEnumerator"/> object; <c>false</c> otherwise.</returns>
            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is ReplaceableCharacterEnumerator that))
                {
                    return false;
                }

                if (GetHashCode() != that.GetHashCode())
                {
                    return false;
                }
                if (!text.Equals(that.text))
                {
                    return false;
                }
                if (pos != that.pos || begin != that.begin || end != that.end)
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Computes a hash code for this iterator.
            /// </summary>
            /// <returns>A hash code.</returns>
            public override int GetHashCode()
            {
                return text.GetHashCode() ^ pos ^ begin ^ end;
            }

            /// <summary>
            /// Creates a copy of this iterator.
            /// </summary>
            /// <returns>A copy of this iterator.</returns>
            public object Clone()
            {
                return MemberwiseClone();
            }

            public void Dispose() { }

            void IEnumerator.Reset()
            {
                pos = StartIndex;
            }
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
#pragma warning disable 612, 618
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
#pragma warning restore 612, 618
            // Doesn't actually modify the source characters, so leave them alone.
            // add the characters inserted
            if (myFilter.Count != 0)
            {
                targetSet.AddAll(insertion);
            }
        }
    }
}
