using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal class CharArrayCharSequence : ICharSequence
    {
        private readonly char[] value;

        public CharArrayCharSequence(char[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.value = value;
        }

        internal char[] Value { get { return value; } } // ICU4N TODO: API - replace with CharArray property?

        public char[] CharArray { get { return value; } }

        public char this[int index]
        {
            get { return value[index]; }
        }

        public int Length
        {
            get { return value.Length; }
        }

        public ICharSequence SubSequence(int start, int end)
        {
            // From Apache Harmony String class
            if (start == 0 && end == value.Length)
            {
                return value.ToCharSequence();
            }
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            else if (start > end)
            {
                throw new ArgumentOutOfRangeException("end - start");
            }
            else if (end > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }

            return new string(value, start, end - start).ToCharSequence();
        }
    }
}
