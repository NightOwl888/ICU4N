using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal class StringBuilderCharSequence : ICharSequence, IComparable<ICharSequence>, IComparable
    {
        private readonly StringBuilder value;

        public StringBuilderCharSequence(StringBuilder value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.value = value;
        }

        public StringBuilderCharSequence()
            : this(new StringBuilder())
        {
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

        public static bool operator ==(StringBuilderCharSequence csq1, StringBuilder csq2)
        {
            if ((StringBuilderCharSequence)null == csq1)
                return (null == csq2);

            return csq1.value == csq2;
        }

        public static bool operator !=(StringBuilderCharSequence csq1, StringBuilder csq2)
        {
            if ((StringBuilderCharSequence)null == csq1)
                return (null != csq2);
            if (null == csq2) return true;

            return csq1.value != csq2;
        }

        public static bool operator ==(StringBuilder csq1, StringBuilderCharSequence csq2)
        {
            if (null == csq1)
                return null == csq2.value;

            return csq1 == csq2.value;
        }

        public static bool operator !=(StringBuilder csq1, StringBuilderCharSequence csq2)
        {
            if (null == csq1)
                return null != csq2.value;
            if ((StringBuilderCharSequence)null == csq2) return true;

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
                return this.value.Equals(obj);
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
            else if (obj is StringBuilderCharSequence)
            {
                var sbcsq = obj as StringBuilderCharSequence;
                return this.value.Equals(sbcsq.value);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // NOTE: For consistency, we match all char sequences to the same
            // hash code as the string. This unfortunately means it won't match
            // against StringBuilder or char[]
            return this.value.ToString().GetHashCode();
        }

        // ICU4N TODO: Figure out where this was used and fix the reference
        //public override bool Equals(object obj)
        //{
        //    return this.value == obj;
        //}

        public int CompareTo(ICharSequence other)
        {
            return this.value.ToString().CompareToOrdinal(other.ToString());
        }

        public int CompareTo(object other)
        {
            return this.value.ToString().CompareToOrdinal(other.ToString());
        }
    }
}
