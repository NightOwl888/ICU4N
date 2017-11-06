using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Tests.Dev.Test.Normalizers
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
            Integer value = table.Get(new Integer(key));
            if (value == null) return defaultValue;
            return value.Value;
        }

        private int defaultValue;
        private IDictionary<Integer, Integer> table = new Dictionary<Integer, Integer>();
    }

    internal class Integer
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
    }
}
