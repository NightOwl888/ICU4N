using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;

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

        /**
         * @see UCharacterIterator#current()
         */
        
    public override int Current
        {
            get
            {
                int c = iterator.Current;
                if (c == CharacterIterator.DONE)
                {
                    return DONE;
                }
                return c;
            }
        }

        /**
         * @see UCharacterIterator#getLength()
         */
        
    public override int Length
        {
            get { return (iterator.EndIndex - iterator.BeginIndex); }
        }

        /**
         * @see UCharacterIterator#getIndex()
         */
        
    public override int Index
        {
            get { return iterator.Index; }
            set
            {
                try
                {
                    iterator.SetIndex(value);
                }
                catch (ArgumentException e)
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        /**
         * @see UCharacterIterator#next()
         */
        
    public override int Next()
        {
            int i = iterator.Current;
            iterator.Next();
            if (i == CharacterIterator.DONE)
            {
                return DONE;
            }
            return i;
        }

        /**
         * @see UCharacterIterator#previous()
         */
        
    public override int Previous()
        {
            int i = iterator.Previous();
            if (i == CharacterIterator.DONE)
            {
                return DONE;
            }
            return i;
        }

        /**
         * @see UCharacterIterator#setIndex(int)
         */
        

        /**
         * @see UCharacterIterator#setToLimit()
         */
        
    public override void SetToLimit()
        {
            iterator.SetIndex(iterator.EndIndex);
        }

        /**
         * @see UCharacterIterator#getText(char[])
         */
        
    public override int GetText(char[] fillIn, int offset)
        {
            int length = iterator.EndIndex - iterator.BeginIndex;
            int currentIndex = iterator.Index;
            if (offset < 0 || offset + length > fillIn.Length)
            {
                throw new IndexOutOfRangeException(length.ToString());
            }

            for (char ch = iterator.First(); ch != CharacterIterator.DONE; ch = iterator.Next())
            {
                fillIn[offset++] = ch;
            }
            iterator.SetIndex(currentIndex);

            return length;
        }

        /**
         * Creates a clone of this iterator.  Clones the underlying character iterator.
         * @see UCharacterIterator#clone()
         */
        
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

        /**
         * @see UCharacterIterator#getCharacterIterator()
         */
    public override CharacterIterator GetCharacterIterator()
        {
            return (CharacterIterator)iterator.Clone();
        }
    }
}
