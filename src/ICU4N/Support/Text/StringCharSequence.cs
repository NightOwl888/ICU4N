using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal sealed class StringCharSequence : ICharSequence, IComparable<ICharSequence>, IComparable
    {
        private string value;

        public StringCharSequence(string value)
        {
            this.value = value;
        }

        internal string Value { get { return value; } } // ICU4N TODO: API - replace with String property?

        public string String
        {
            get { return value; }
            set { this.value = value; } // setting allows the object to be reused multiple times
        }

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

        public static bool operator ==(StringCharSequence csq1, string csq2)
        {
            if ((StringCharSequence)null == csq1)
                return (null == csq2);

            return csq1.value == csq2;
        }

        public static bool operator !=(StringCharSequence csq1, string csq2)
        {
            if ((StringCharSequence)null == csq1)
                return (null != csq2);
            if (null == csq2) return true;

            return csq1.value != csq2;
        }

        public static bool operator ==(string csq1, StringCharSequence csq2)
        {
            if (null == csq1)
                return null == csq2.value;

            return csq1 == csq2.value;
        }

        public static bool operator !=(string csq1, StringCharSequence csq2)
        {
            if (null == csq1)
                return null != csq2.value;
            if ((StringCharSequence)null == csq2) return true;

            return csq1 != csq2.value;
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return this.value.Equals(obj);
            }
            else if (obj is StringBuilder)
            {
                StringBuilder sb = obj as StringBuilder;
                int len = this.value.Length;
                if (len != sb.Length) return false;
                for (int i = 0; i < len; i++)
                {
                    if (!this.value[i].Equals(sb[i])) return false;
                }
                return true;
            }
            else if (obj is char[])
            {
                char[] chars = obj as char[];
                int len = this.value.Length;
                if (len != chars.Length) return false;
                for (int i = 0; i < len; i++)
                {
                    if (!this.value[i].Equals(chars[i])) return false;
                }
                return true;
            }
            else if (obj is StringCharSequence)
            {
                var sbcsq = obj as StringCharSequence;
                return this.value.Equals(sbcsq.value);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // NOTE: For consistency, we match all char sequences to the same
            // hash code as the string. This unfortunately means it won't match
            // against StringBuilder or char[]
            return this.value.GetHashCode();
        }

        public int CompareTo(ICharSequence other)
        {
            return this.value.CompareToOrdinal(other.ToString());
        }

        public int CompareTo(object other)
        {
            return this.value.CompareToOrdinal(other.ToString());
        }
    }
}
