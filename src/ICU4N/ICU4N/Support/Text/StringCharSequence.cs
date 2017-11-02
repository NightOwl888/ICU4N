using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal sealed class StringCharSequence : ICharSequence
    {
        private readonly string value;

        public StringCharSequence(string value)
        {
            this.value = value;
        }

        internal string Value { get { return value; } } // ICU4N TODO: API - replace with String property?

        public string String { get { return value; } }

        public char this[int index]
        {
            get { return (value == null) ? (char)0 : value[index]; }
        }

        public int Length
        {
            get { return (value == null) ? 0 : value.Length; }
        }

        public ICharSequence SubSequence(int start, int end)
        {
            if (value == null)
                return null;

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
            // NOTE last character not copied!
            return value.Substring(start, end - start).ToCharSequence();
        }

        public override string ToString()
        {
            return (value == null) ? string.Empty : value.ToString();
        }
    }
}
