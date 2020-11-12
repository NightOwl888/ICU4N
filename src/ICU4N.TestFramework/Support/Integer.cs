using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support
{
    /// <summary>
    /// Very simple wrapper for <see cref="int"/> to make it into a reference type.
    /// </summary>
    public class Integer : IComparable<Integer>
    {
        public Integer(int value)
        {
            this.Value = value;
        }

        public int Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Integer)
            {
                return Value.Equals(((Integer)obj).Value);
            }
            return Value.Equals(obj);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public int CompareTo(Integer other)
        {
            if (other == null)
                return 1;
            return this.Value.CompareTo(other.Value);
        }

        public static implicit operator int(Integer integer) => integer.Value;
        public static implicit operator Integer(int value) => new Integer(value);
    }
}
