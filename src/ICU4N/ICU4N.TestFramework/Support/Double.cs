using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support
{
    /// <summary>
    /// Very simple wrapper for <see cref="double"/> to make it into a reference type.
    /// </summary>
    public class Double
    {
        public Double(double value)
        {
            this.Value = value;
        }

        public double Value { get; set; }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Double)
            {
                return Value.Equals(((Double)obj).Value);
            }
            return Value.Equals(obj);
        }

        public override string ToString()
        {
            return Value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public virtual string ToString(string format, IFormatProvider provider)
        {
            return Value.ToString(format, provider);
        }
    }
}
