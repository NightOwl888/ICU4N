using ICU4N.Support;
using ICU4N.Support.Collections;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Normalizers
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
}
