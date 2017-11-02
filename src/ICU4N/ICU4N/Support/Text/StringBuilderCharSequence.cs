using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal class StringBuilderCharSequence : ICharSequence
    {
        private readonly StringBuilder value;

        public StringBuilderCharSequence(StringBuilder value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.value = value;
        }

        internal string Value { get { return value.ToString(); } } // ICU4N TODO: API - replace with StringBuilder property?

        public StringBuilder StringBuilder { get { return value; } }

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
                throw new IndexOutOfRangeException(nameof(start));
            }
            else if (start > end)
            {
                throw new IndexOutOfRangeException("end - start");
            }
            else if (end > value.Length)
            {
                throw new IndexOutOfRangeException(nameof(end));
            }

            return value.ToString(start, end - start).ToCharSequence();
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.value == obj;
        }
    }
}
