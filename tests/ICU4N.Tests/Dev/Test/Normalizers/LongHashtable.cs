using ICU4N.Support;
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
            return !table.TryGetValue(key, out Integer value) || value == null ? defaultValue : value.Value;
        }

        private readonly int defaultValue;
        private readonly IDictionary<Long, Integer> table = new Dictionary<Long, Integer>();

    }
}
