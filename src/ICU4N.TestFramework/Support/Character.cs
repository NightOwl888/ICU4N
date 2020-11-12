using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    public class Character
    {
        public Character(char value)
        {
            this.Value = value;
        }

        public char Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Character)
            {
                return Value.Equals(((Character)obj).Value);
            }
            return Value.Equals(obj);
        }

        public static implicit operator char(Character ch) => ch.Value;
        public static implicit operator Character(char value) => new Character(value);
    }
}
