using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Implement the CharacterIterator abstract class on a ICharSequence.
    /// Intended for internal use by ICU only.
    /// </summary>
    internal class CSCharacterIterator : CharacterIterator
    {
        private int index;
        private ICharSequence seq;


        /**
         * Constructor.
         * @param text The CharSequence to iterate over.
         */
        public CSCharacterIterator(ICharSequence text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            seq = text;
            index = 0;
        }

        /** @{inheritDoc} */
        public override char First()
        {
            index = 0;
            return Current;
        }

        /** @{inheritDoc} */
        public override char Last()
        {
            index = seq.Length;
            return Previous();
        }

        /** @{inheritDoc} */
        public override char Current
        {
            get
            {
                if (index == seq.Length)
                {
                    return DONE;
                }
                return seq[index];
            }
        }

        /** @{inheritDoc} */
        public override char Next()
        {
            if (index < seq.Length)
            {
                ++index;
            }
            return Current;
        }

        /** @{inheritDoc} */
        public override char Previous()
        {
            if (index == 0)
            {
                return DONE;
            }
            --index;
            return Current;
        }

        /** @{inheritDoc} */
        public override char SetIndex(int position)
        {
            if (position < 0 || position > seq.Length)
            {
                throw new ArgumentException();
            }
            index = position;
            return Current;
        }

        /** @{inheritDoc} */
        public override int BeginIndex
        {
            get { return 0; }
        }

        /** @{inheritDoc} */
        public override int EndIndex
        {
            get { return seq.Length; }
        }

        /** @{inheritDoc} */
        public override int Index
        {
            get { return index; }
        }

        /** @{inheritDoc} */
        public override object Clone()
        {
            CSCharacterIterator copy = new CSCharacterIterator(seq);
            copy.SetIndex(index);
            return copy;
        }
    }
}
