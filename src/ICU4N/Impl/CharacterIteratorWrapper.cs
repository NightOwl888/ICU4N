using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Text;
using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class is a wrapper around <see cref="CharacterIterator"/> and implements the
    /// <see cref="UCharacterIterator"/> protocol
    /// </summary>
    /// <author>ram</author>
    public class CharacterIteratorWrapper : UCharacterIterator
    {
        private CharacterIterator iterator;


        public CharacterIteratorWrapper(CharacterIterator iter)
        {
            if (iter == null)
            {
                throw new ArgumentException(nameof(iter));
            }
            iterator = iter;
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
        public override int Length => (iterator.EndIndex - iterator.BeginIndex);

        /// <seealso cref="UCharacterIterator.Index"/>
        public override int Index
        {
            get => iterator.Index;
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

        /// <seealso cref="UCharacterIterator.GetCharacterIterator()"/>
        public override CharacterIterator GetCharacterIterator()
        {
            return (CharacterIterator)iterator.Clone();
        }
    }
}
