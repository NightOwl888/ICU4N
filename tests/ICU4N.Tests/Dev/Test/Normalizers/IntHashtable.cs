using ICU4N.Support;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// Integer hash table.
    /// </summary>
    /// <author>Mark Davis</author>
    public class IntHashtable
    {
        public IntHashtable(int defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public void Put(int key, int value)
        {
            if (value == defaultValue)
            {
                table.Remove(new Integer(key));
            }
            else
            {
                table[new Integer(key)]= new Integer(value);
            }
        }

        public int Get(int key)
        {
            return !table.TryGetValue(key, out Integer value) || value == null ? defaultValue : value.Value;
        }

        private readonly int defaultValue;
        private readonly IDictionary<Integer, Integer> table = new Dictionary<Integer, Integer>();
    }
}
