using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Text;
using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class is a wrapper around <see cref="ICharacterEnumerator"/> and implements the
    /// <see cref="UCharacterIterator"/> protocol
    /// </summary>
    /// <author>ram</author>
    public class CharacterEnumeratorWrapper : UCharacterIterator
    {
        private ICharacterEnumerator iterator;
        // ICU4N: Since our ICharacterEnumerator's EndIndex is the end of the string
        // and in ICU4J it is one past the end of the string, we keep track of whether
        // we are past the end of the string with this boolean flag.
        private bool pastEnd = false;

        public CharacterEnumeratorWrapper(ICharacterEnumerator characterEnumerator)
        {
            iterator = characterEnumerator ?? throw new ArgumentException(nameof(characterEnumerator));
        }

        /// <seealso cref="UCharacterIterator.Current"/>
        public override int Current
        {
            get
            {
                if (pastEnd)
                    return Done;
                return iterator.Current;
            }
        }

        public override int CurrentCodePoint
        {
            get
            {
                if (pastEnd)
                    return Done;
                return base.CurrentCodePoint;
            }
        }

        /// <seealso cref="UCharacterIterator.Length"/>
        public override int Length => iterator.Length;


        /// <seealso cref="UCharacterIterator.Index"/>
        public override int Index
        {
            get => iterator.Index + (pastEnd ? 1 : 0);
            set
            {
                if (value == iterator.EndIndex + 1)
                {
                    iterator.Index = iterator.EndIndex;
                    pastEnd = true;
                }
                else
                {
                    iterator.Index = value;
                    pastEnd = false;
                }
            }
        }

        /// <seealso cref="UCharacterIterator.Next()"/>
        public override int Next()
        {
            int i = iterator.Current;
            bool nextDone = !iterator.MoveNext();
            if (pastEnd)
            {
                return Done;
            }
            pastEnd = nextDone;
            return i;
        }

        /// <seealso cref="UCharacterIterator.Previous()"/>
        public override int Previous()
        {
            if (pastEnd)
            {
                pastEnd = false;
            }
            else if (!iterator.MovePrevious())
            {
                return Done;
            }
            return iterator.Current;
        }

        /// <seealso cref="UCharacterIterator.SetToLimit()"/>
        public override void SetToLimit()
        {
            iterator.Index = iterator.EndIndex;
            pastEnd = true;
        }

        /// <seealso cref="UCharacterIterator.GetText(char[])"/>
        public override int GetText(char[] destination, int offset)
        {
            // ICU4N: Reworked guard clauses
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            int length = iterator.Length;
            if (offset < 0 || offset > length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > destination.Length)
                throw new ArgumentException($"Not enough space in the destination array: {length}", nameof(destination));

            int currentIndex = iterator.Index;
            if (iterator.MoveFirst())
                destination[offset++] = iterator.Current;
            while (iterator.MoveNext())
                destination[offset++] = iterator.Current;

            iterator.Index = currentIndex;

            return length;
        }

        /// <summary>
        /// Creates a clone of this iterator.  Clones the underlying character iterator.
        /// </summary>
        /// <seealso cref="UCharacterIterator.Clone()"/>
        public override object Clone()
        {
            CharacterEnumeratorWrapper result = (CharacterEnumeratorWrapper)base.Clone();
            result.iterator = (ICharacterEnumerator)this.iterator.Clone();
            return result;
        }

        public override int MoveIndex(int delta)
        {
            int length = iterator.Length;
            int idx = Index + delta;
            pastEnd = false;

            if (idx < 0)
            {
                idx = 0;
            }
            else if (idx >= length)
            {
                idx = length;
            }
            if (!iterator.TrySetIndex(idx))
            {
                pastEnd = true;
                return Done;
            }
            return iterator.Current;
        }

        /// <seealso cref="UCharacterIterator.GetCharacterEnumerator()"/>
        public override ICharacterEnumerator GetCharacterEnumerator()
        {
            return (ICharacterEnumerator)iterator.Clone();
        }
    }

    /// <summary>
    /// This class is a wrapper around <see cref="CharacterIterator"/> and implements the
    /// <see cref="UCharacterIterator"/> protocol
    /// </summary>
    /// <author>ram</author>
    internal class CharacterIteratorWrapper : UCharacterIterator // ICU4N: Marked internal because CharacterIterator was marked internal
    {
        private CharacterIterator iterator;


        public CharacterIteratorWrapper(CharacterIterator iterator)
        {
            this.iterator = iterator ?? throw new ArgumentNullException(nameof(iterator));
        }

        /// <seealso cref="UCharacterIterator.Current"/>
        public override int Current
        {
            get
            {
                int c = iterator.Current;
                if (c == CharacterIterator.Done)
                {
                    return Done;
                }
                return c;
            }
        }

        /// <seealso cref="UCharacterIterator.Length"/>
        public override int Length
        {
            get { return (iterator.EndIndex - iterator.BeginIndex); }
        }

        /// <seealso cref="UCharacterIterator.Index"/>
        public override int Index
        {
            get { return iterator.Index; }
            set
            {
                try
                {
                    iterator.SetIndex(value);
                }
                catch (ArgumentException)
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /// <seealso cref="UCharacterIterator.Next()"/>
        public override int Next()
        {
            int i = iterator.Current;
            iterator.Next();
            if (i == CharacterIterator.Done)
            {
                return Done;
            }
            return i;
        }

        /// <seealso cref="UCharacterIterator.Previous()"/>
        public override int Previous()
        {
            int i = iterator.Previous();
            if (i == CharacterIterator.Done)
            {
                return Done;
            }
            return i;
        }

        /// <seealso cref="UCharacterIterator.SetToLimit()"/>
        public override void SetToLimit()
        {
            iterator.SetIndex(iterator.EndIndex);
        }

        /// <seealso cref="UCharacterIterator.GetText(char[])"/>
        public override int GetText(char[] fillIn, int offset)
        {
            int length = iterator.EndIndex - iterator.BeginIndex;
            int currentIndex = iterator.Index;
            if (offset < 0 || offset + length > fillIn.Length)
            {
                throw new IndexOutOfRangeException(length.ToString());
            }

            for (char ch = iterator.First(); ch != CharacterIterator.Done; ch = iterator.Next())
            {
                fillIn[offset++] = ch;
            }
            iterator.SetIndex(currentIndex);

            return length;
        }

        /// <summary>
        /// Creates a clone of this iterator.  Clones the underlying character iterator.
        /// </summary>
        /// <seealso cref="UCharacterIterator.Clone()"/>
        public override Object Clone()
        {
            CharacterIteratorWrapper result = (CharacterIteratorWrapper)base.Clone();
            result.iterator = (CharacterIterator)this.iterator.Clone();
            return result;
        }

        public override int MoveIndex(int delta)
        {
            int length = iterator.EndIndex - iterator.BeginIndex;
            int idx = iterator.Index + delta;

            if (idx < 0)
            {
                idx = 0;
            }
            else if (idx > length)
            {
                idx = length;
            }
            return iterator.SetIndex(idx);
        }

        /// <seealso cref="UCharacterIterator.GetCharacterEnumerator()"/>
        public override ICharacterEnumerator GetCharacterEnumerator()
        {
            return (ICharacterEnumerator)iterator.Clone();
        }
    }
}
