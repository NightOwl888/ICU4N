using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Globalization;

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

        ///CLOVER:OFF
        // The following method is not called by anything and can't be reached
        public virtual string Insertion
        {
            get { return insertion; }
            set { insertion = value; }
        }
        ///CLOVER:ON

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

        ///CLOVER:OFF
        // The following method is not called by anything and can't be reached
        public void SetBreakIterator(BreakIterator bi)
        {
            this.bi = bi;
        }
        ///CLOVER:ON

        internal static readonly int LETTER_OR_MARK_MASK =
              (1 << UCharacterCategory.UppercaseLetter.ToInt32())
            | (1 << UCharacterCategory.LowercaseLetter.ToInt32())
            | (1 << UCharacterCategory.TitlecaseLetter.ToInt32())
            | (1 << UCharacterCategory.ModifierLetter.ToInt32())
            | (1 << UCharacterCategory.OtherLetter.ToInt32())
            | (1 << UCharacterCategory.SpacingCombiningMark.ToInt32())
            | (1 << UCharacterCategory.NonSpacingMark.ToInt32())
            | (1 << UCharacterCategory.EnclosingMark.ToInt32())
            ;

        protected override void HandleTransliterate(IReplaceable text, TransliterationPosition pos, bool incremental)
        {
            lock (this)
            {
                boundaryCount = 0;
                int boundary = 0;
                GetBreakIterator(); // Lazy-create it if necessary
                bi.SetText(new ReplaceableCharacterIterator(text, pos.Start, pos.Limit, pos.Start));
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
                    int type = UCharacter.GetType(cp).ToInt32();
                    //System.out.println(Integer.toString(cp,16) + " (before): " + type);
                    if (((1 << type) & LETTER_OR_MARK_MASK) == 0) continue;

                    cp = UTF16.CharAt(text, boundary);
                    type = UCharacter.GetType(cp).ToInt32();
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
                        text.Replace(boundary, boundary, insertion);
                    }
                }

                // Now fix up the return values
                pos.ContextLimit += delta;
                pos.Limit += delta;
                pos.Start = incremental ? lastBoundary + delta : pos.Limit;
            }
        }


        /**
         * Registers standard variants with the system.  Called by
         * Transliterator during initialization.
         */
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

        // Hack, just to get a real character iterator.
        internal sealed class ReplaceableCharacterIterator : CharacterIterator
        {
            private IReplaceable text;
            private int begin;
            private int end;
            // invariant: begin <= pos <= end
            private int pos;

            /**
            * Constructs an iterator with an initial index of 0.
            */
            /*public ReplaceableCharacterIterator(Replaceable text)
            {
                this(text, 0);
            }*/

            /**
            * Constructs an iterator with the specified initial index.
            *
            * @param  text   The String to be iterated over
            * @param  pos    Initial iterator position
            */
            /*public ReplaceableCharacterIterator(Replaceable text, int pos)
            {
                this(text, 0, text.length(), pos);
            }*/

            /**
            * Constructs an iterator over the given range of the given string, with the
            * index set at the specified position.
            *
            * @param  text   The String to be iterated over
            * @param  begin  Index of the first character
            * @param  end    Index of the character following the last character
            * @param  pos    Initial iterator position
            */
            public ReplaceableCharacterIterator(IReplaceable text, int begin, int end, int pos)
            {
                if (text == null)
                {
                    throw new ArgumentNullException(nameof(text));
                }
                this.text = text;

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

            /**
            * Reset this iterator to point to a new string.  This package-visible
            * method is used by other java.text classes that want to avoid allocating
            * new ReplaceableCharacterIterator objects every time their setText method
            * is called.
            *
            * @param  text   The String to be iterated over
            */
            public void SetText(IReplaceable text)
            {
                if (text == null)
                {
                    throw new ArgumentNullException(nameof(text));
                }
                this.text = text;
                this.begin = 0;
                this.end = text.Length;
                this.pos = 0;
            }

            /**
            * Implements CharacterIterator.first() for String.
            * @see CharacterIterator#first
            */
            public override char First()
            {
                pos = begin;
                return Current;
            }

            /**
            * Implements CharacterIterator.last() for String.
            * @see CharacterIterator#last
            */
            public override char Last()
            {
                if (end != begin)
                {
                    pos = end - 1;
                }
                else
                {
                    pos = end;
                }
                return Current;
            }

            /**
            * Implements CharacterIterator.setIndex() for String.
            * @see CharacterIterator#setIndex
            */
            public override char SetIndex(int p)
            {
                if (p < begin || p > end)
                {
                    throw new ArgumentException("Invalid index");
                }
                pos = p;
                return Current;
            }

            /**
            * Implements CharacterIterator.current() for String.
            * @see CharacterIterator#current
            */
            public override char Current
            {
                get
                {
                    if (pos >= begin && pos < end)
                    {
                        return text[pos];
                    }
                    else
                    {
                        return Done;
                    }
                }
            }

            /**
            * Implements CharacterIterator.next() for String.
            * @see CharacterIterator#next
            */
            public override char Next()
            {
                if (pos < end - 1)
                {
                    pos++;
                    return text[pos];
                }
                else
                {
                    pos = end;
                    return Done;
                }
            }

            /**
            * Implements CharacterIterator.previous() for String.
            * @see CharacterIterator#previous
            */
            public override char Previous()
            {
                if (pos > begin)
                {
                    pos--;
                    return text[pos];
                }
                else
                {
                    return Done;
                }
            }

            /**
            * Implements CharacterIterator.getBeginIndex() for String.
            * @see CharacterIterator#getBeginIndex
            */
            public override int BeginIndex
            {
                get { return begin; }
            }

            /**
            * Implements CharacterIterator.getEndIndex() for String.
            * @see CharacterIterator#getEndIndex
            */
            public override int EndIndex
            {
                get { return end; }
            }

            /**
            * Implements CharacterIterator.getIndex() for String.
            * @see CharacterIterator#getIndex
            */
            public override int Index
            {
                get { return pos; }
            }

            /**
            * Compares the equality of two ReplaceableCharacterIterator objects.
            * @param obj the ReplaceableCharacterIterator object to be compared with.
            * @return true if the given obj is the same as this
            * ReplaceableCharacterIterator object; false otherwise.
            */
            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is ReplaceableCharacterIterator))
                {
                    return false;
                }

                ReplaceableCharacterIterator that = (ReplaceableCharacterIterator)obj;

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

            /**
            * Computes a hashcode for this iterator.
            * @return A hash code
            */
            public override int GetHashCode()
            {
                return text.GetHashCode() ^ pos ^ begin ^ end;
            }

            /**
            * Creates a copy of this iterator.
            * @return A copy of this
            */
            public override object Clone()
            {
                ReplaceableCharacterIterator other = (ReplaceableCharacterIterator)base.MemberwiseClone();
                return other;
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
