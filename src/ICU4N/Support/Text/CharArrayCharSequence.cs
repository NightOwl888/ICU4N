using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal class CharArrayCharSequence : ICharSequence, IComparable<ICharSequence>, IComparable
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

        public override string ToString()
        {
            return new string(value);
        }

        public static bool operator ==(CharArrayCharSequence csq1, char[] csq2)
        {
            if ((CharArrayCharSequence)null == csq1)
                return (null == csq2);

            return csq1.value == csq2;
        }

        public static bool operator !=(CharArrayCharSequence csq1, char[] csq2)
        {
            if ((CharArrayCharSequence)null == csq1)
                return (null != csq2);
            if (null == csq2) return true;

            return csq1.value != csq2;
        }

        public static bool operator ==(char[] csq1, CharArrayCharSequence csq2)
        {
            if (null == csq1)
                return null == csq2.value;

            return csq1 == csq2.value;
        }

        public static bool operator !=(char[] csq1, CharArrayCharSequence csq2)
        {
            if (null == csq1)
                return null != csq2.value;
            if ((CharArrayCharSequence)null == csq2) return true;

            return csq1 != csq2.value;
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                string str = obj as string;
                int len = this.value.Length;
                if (len != str.Length) return false;
                for (int i = 0; i < len; i++)
                {
                    if (!this.value[i].Equals(str[i])) return false;
                }
                return true;
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
            else if (obj is CharArrayCharSequence)
            {
                var sbcsq = obj as CharArrayCharSequence;
                int len = this.value.Length;
                if (len != sbcsq.Length) return false;
                for (int i = 0; i < len; i++)
                {
                    if (!this.value[i].Equals(sbcsq[i])) return false;
                }
                return true;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // NOTE: For consistency, we match all char sequences to the same
            // hash code as the string. This unfortunately means it won't match
            // against StringBuilder or char[]
            return new string(this.value).GetHashCode();
        }

        public int CompareTo(ICharSequence other)
        {
            return new string(this.value).CompareToOrdinal(other.ToString());
        }

        public int CompareTo(object other)
        {
            return new string(this.value).CompareToOrdinal(other.ToString());
        }
    }
}
