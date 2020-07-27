using ICU4N.Globalization;
using ICU4N.Support.Text;
using System;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// Inserts the specified characters at word breaks. To restrict it to particular characters, use a filter.
    /// TODO: this is an internal class, and only temporary. Remove it once we have \b notation in Transliterator.
    /// </summary>
    internal class BreakTransliterator : Transliterator
    {
        private readonly object syncLock = new object();
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
            get => insertion;
            set => insertion = value;
        }
        ////CLOVER:ON

        public BreakIterator GetBreakIterator()
        {
            // Defer initialization of BreakIterator because it is slow,
            // typically over 2000 ms.
            if (bi == null)
                return LazyInitializer.EnsureInitialized(ref bi, () => BreakIterator.GetWordInstance(new UCultureInfo("th_TH")));

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
            lock (syncLock)
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

        // Hack, just to get a real character iterator.
        internal sealed class ReplaceableCharacterIterator : CharacterIterator
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
            //public ReplaceableCharacterIterator(IReplaceable text)
            //    : this(text, 0)
            //{
            //}

            ///// <summary>
            ///// Constructs an iterator with the specified initial index.
            ///// </summary>
            ///// <param name="text">The <see cref="string"/> to be iterated over.</param>
            ///// <param name="pos">Initial iterator position.</param>
            //public ReplaceableCharacterIterator(IReplaceable text, int pos)
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
            public ReplaceableCharacterIterator(IReplaceable text, int begin, int end, int pos)
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
            /// new <see cref="ReplaceableCharacterIterator"/> objects every 
            /// time their <see cref="SetText(IReplaceable)"/> method
            /// is called.
            /// </summary>
            /// <param name="text">The <see cref="string"/> to be iterated over.</param>
            public void SetText(IReplaceable text)
            {
                this.text = text ?? throw new ArgumentNullException(nameof(text));
                this.begin = 0;
                this.end = text.Length;
                this.pos = 0;
            }

            /// <summary>
            /// Implements <see cref="CharacterIterator.First()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.First()"/>
            public override char First()
            {
                pos = begin;
                return Current;
            }

            /// <summary>
            /// Implements <see cref="CharacterIterator.Last()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.Last()"/>
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

            /// <summary>
            /// Implements <see cref="CharacterIterator.SetIndex(int)"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.SetIndex(int)"/>
            public override char SetIndex(int p)
            {
                if (p < begin || p > end)
                {
                    throw new ArgumentException("Invalid index");
                }
                pos = p;
                return Current;
            }

            /// <summary>
            /// Implements <see cref="CharacterIterator.Current"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.Current"/>
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

            /// <summary>
            /// Implements <see cref="CharacterIterator.Next()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.Next()"/>
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

            /// <summary>
            /// Implements <see cref="CharacterIterator.Previous()"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.Previous()"/>
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

            /// <summary>
            /// Implements <see cref="CharacterIterator.BeginIndex"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.BeginIndex"/>
            public override int BeginIndex
            {
                get { return begin; }
            }

            /// <summary>
            /// Implements <see cref="CharacterIterator.EndIndex"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.EndIndex"/>
            public override int EndIndex
            {
                get { return end; }
            }

            /// <summary>
            /// Implements <see cref="CharacterIterator.Index"/> for <see cref="string"/>.
            /// </summary>
            /// <seealso cref="CharacterIterator.Index"/>
            public override int Index
            {
                get { return pos; }
            }

            /// <summary>
            /// Compares the equality of two <see cref="ReplaceableCharacterIterator"/> objects.
            /// </summary>
            /// <param name="obj">The <see cref="ReplaceableCharacterIterator"/> object to be compared with.</param>
            /// <returns><c>true</c> if the given obj is the same as this
            /// <see cref="ReplaceableCharacterIterator"/> object; <c>false</c> otherwise.</returns>
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
