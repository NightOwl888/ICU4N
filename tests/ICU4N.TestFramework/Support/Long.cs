using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    /// <summary>
    /// Very simple wrapper for <see cref="long"/> to make it into a reference type.
    /// </summary>
    public class Long
    {
        public Long(long value)
        {
            this.Value = value;
        }

        public long Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Long)
            {
                return Value.Equals(((Long)obj).Value);
            }
            return Value.Equals(obj);
        }

        public static implicit operator long(Long lng) => lng.Value;
        public static implicit operator Long(long value) => new Long(value);
    }
}
