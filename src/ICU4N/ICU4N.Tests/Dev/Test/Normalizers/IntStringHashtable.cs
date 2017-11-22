using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// Integer-String hash table. Uses Java Hashtable for now.
    /// </summary>
    /// <author>Mark Davis</author>
    public class IntStringHashtable
    {
        public IntStringHashtable(String defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public void Put(int key, String value)
        {
            if (value == defaultValue)
            {
                table.Remove(new Integer(key));
            }
            else
            {
                table[new Integer(key)]= value;
            }
        }

        public String Get(int key)
        {
            String value = table.Get(new Integer(key));
            if (value == null) return defaultValue;
            return value;
        }

        private String defaultValue;
        private IDictionary<Integer, String> table = new Dictionary<Integer, String>();
    }
}
