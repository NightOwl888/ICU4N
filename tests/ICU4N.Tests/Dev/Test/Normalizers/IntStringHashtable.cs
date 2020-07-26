using ICU4N.Support;
using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// Integer-String hash table. Uses Java Hashtable for now.
    /// </summary>
    /// <author>Mark Davis</author>
    public class IntStringHashtable
    {
        public IntStringHashtable(string defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public void Put(int key, string value)
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

        public string Get(int key)
        {
            return !table.TryGetValue(key, out string value) || value == null ? defaultValue : value;
        }

        private readonly string defaultValue;
        private readonly IDictionary<Integer, string> table = new Dictionary<Integer, string>();
    }
}
