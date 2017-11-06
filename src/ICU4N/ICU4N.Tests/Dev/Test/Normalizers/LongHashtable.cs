using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Tests.Dev.Test.Normalizers
{
    /// <summary>
    /// Hashtable storing ints addressed by longs. Used
    /// for storing of composition data.
    /// </summary>
    /// <author>Vladimir Weinstein</author>
    public class LongHashtable
    {
        public LongHashtable(int defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public void Put(long key, int value)
        {
            if (value == defaultValue)
            {
                table.Remove(new Long(key));
            }
            else
            {
                table[new Long(key)]=new Integer(value);
            }
        }

        public int Get(long key)
        {
            Integer value = table.Get(new Long(key));
            if (value == null) return defaultValue;
            return value.Value;
        }

        private int defaultValue;
        private IDictionary<Long, Integer> table = new Dictionary<Long, Integer>();

    }

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
    }
}
